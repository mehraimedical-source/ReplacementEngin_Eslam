using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    /// <summary>
    /// Regressionها را مستقیماً از بانک Sample اجرا می‌کند.
    /// هیچ OCR یا Expected به صورت Hard-code داخل کد تست نگهداری نمی‌شود.
    /// </summary>
    public sealed class SpatialOcrRegressionTests
    {
        public string Run(RowDetectionOptions options)
        {
            if (options == null) options = new RowDetectionOptions();

            SpatialOcrSampleRepository repository = new SpatialOcrSampleRepository();
            List<string> folders = repository.GetSampleFolders();
            StringBuilder report = new StringBuilder();
            int passed = 0;
            int failed = 0;

            for (int i = 0; i < folders.Count; i++)
            {
                string folder = folders[i];
                string name = Path.GetFileName(folder);

                try
                {
                    string ocrJson = File.ReadAllText(Path.Combine(folder, "ocr.json"), Encoding.UTF8);
                    string expectedJson = File.ReadAllText(Path.Combine(folder, "expected.json"), Encoding.UTF8);
                    string[] expectedRows = ReadExpectedRows(expectedJson);

                    List<SpatialOcrItem> items = SpatialOcrJsonParser.Parse(ocrJson, options);
                    List<SpatialRow> rows = new AdaptiveRowDetector(options).Detect(items);

                    // Regression فقط RowDetector را تست نمی‌کند. Expected Rows در Sample Bank
                    // همان ساختار منطقی مورد انتظار هر خط است؛ بنابراین خروجی Resolved Cluster
                    // نیز باید بتواند دقیقاً همان خط را بازسازی کند. این تست تغییراتی را که
                    // Row سالم را در ContextualClusterResolver دوباره Split می‌کنند آشکار می‌کند.
                    HorizontalClusterDetector clusterDetector = new HorizontalClusterDetector(
                        new HorizontalClusteringOptions());
                    List<ContextualClusterResolution> resolved =
                        new ContextualClusterResolver(new ContextualClusterResolverOptions())
                        .Resolve(rows, clusterDetector);

                    string rowError;
                    string resolvedError;
                    bool rowsPassed = RowsEqual(rows, expectedRows, out rowError);
                    bool resolvedPassed = ResolvedRowsEqual(resolved, expectedRows, out resolvedError);

                    if (rowsPassed && resolvedPassed)
                    {
                        passed++;
                        report.AppendLine("PASS  " + name);
                        report.AppendLine("      Rows: PASS");
                        report.AppendLine("      ResolvedClusters: PASS");
                    }
                    else
                    {
                        failed++;
                        report.AppendLine("FAIL  " + name);
                        report.AppendLine("      Rows: " + (rowsPassed ? "PASS" : "FAIL - " + rowError));
                        report.AppendLine("      ResolvedClusters: " +
                            (resolvedPassed ? "PASS" : "FAIL - " + resolvedError));
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    report.AppendLine("FAIL  " + name);
                    report.AppendLine("      Exception: " + ex.Message);
                }

                report.AppendLine();
            }

            report.Insert(0,
                "Spatial OCR Sample Regression Tests" + Environment.NewLine +
                "Sample Bank: " + repository.RootPath + Environment.NewLine +
                "Tests: " + folders.Count + "   Passed: " + passed + "   Failed: " + failed +
                Environment.NewLine + Environment.NewLine);

            if (folders.Count == 0)
                report.AppendLine("No samples found. Save the first sample with 'Save Sample'.");

            return report.ToString();
        }

        /// <summary>
        /// Regression واقعی باید تمام Rowهای Expected را دقیقاً و به همان ترتیب بازسازی کند.
        /// برخلاف تست قدیمی، Row اضافی نیز Failure است تا تغییر رفتار Engine مخفی نماند.
        /// </summary>
        private static bool RowsEqual(IList<SpatialRow> actualRows, string[] expectedRows, out string error)
        {
            if (actualRows.Count != expectedRows.Length)
            {
                error = "Row count: expected " + expectedRows.Length + ", actual " + actualRows.Count;
                return false;
            }

            for (int i = 0; i < expectedRows.Length; i++)
            {
                string actual = NormalizeRow(actualRows[i].GetText());
                string expected = NormalizeRow(expectedRows[i]);
                if (actual != expected)
                {
                    error = "Row " + (i + 1) + ": expected [" + expectedRows[i] +
                        "] actual [" + actualRows[i].GetText() + "]";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Clusterهای Resolve شده هر Row باید دوباره همان Expected Row را بسازند.
        /// اگر یک Row مانند BPD به دو Cluster شکسته شود، حتی با سالم بودن RowDetector
        /// Regression باید FAIL شود تا خرابی لایه Spatial پنهان نماند.
        /// </summary>
        private static bool ResolvedRowsEqual(IList<ContextualClusterResolution> resolved,
            string[] expectedRows, out string error)
        {
            if (resolved.Count != expectedRows.Length)
            {
                error = "Resolved row count: expected " + expectedRows.Length +
                    ", actual " + resolved.Count;
                return false;
            }

            for (int i = 0; i < expectedRows.Length; i++)
            {
                List<SpatialCluster> clusters = resolved[i].ResolvedClusters;

                // در Expected فعلی هر Row یک ساختار منطقی کامل است. تا وقتی Contract
                // دقیق Cell/Region به Sample Bank اضافه نشده، بیش از یک Cluster یعنی
                // Engine همان Row صحیح را دوباره شکسته و باید Failure گزارش شود.
                if (clusters.Count != 1)
                {
                    StringBuilder actual = new StringBuilder();
                    for (int j = 0; j < clusters.Count; j++)
                    {
                        if (j > 0) actual.Append(" || ");
                        actual.Append("[");
                        actual.Append(clusters[j].GetText());
                        actual.Append("]");
                    }

                    error = "Row " + (i + 1) + " split into " + clusters.Count +
                        " clusters. Expected [" + expectedRows[i] + "] Actual " + actual.ToString();
                    return false;
                }

                string actualText = NormalizeRow(clusters[0].GetText());
                string expectedText = NormalizeRow(expectedRows[i]);
                if (actualText != expectedText)
                {
                    error = "Row " + (i + 1) + ": expected [" + expectedRows[i] +
                        "] actual [" + clusters[0].GetText() + "]";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        /// آرایه Rows را از expected.json می‌خواند.
        /// فایل Expected توسط Sample Repository تولید می‌شود؛ Parser کوچک زیر فقط همین Contract کنترل‌شده را می‌خواند.
        /// </summary>
        private static string[] ReadExpectedRows(string json)
        {
            // Regex قبلی در اولین ] متوقف می‌شد؛ بنابراین متنی مثل [OB] یا [2D]
            // باعث می‌شد فقط چند Row اول خوانده شوند. این Parser کوچک مرز Array را
            // با آگاهی از Stringهای JSON پیدا می‌کند و ] داخل متن را اشتباه نمی‌گیرد.
            int rowsName = json.IndexOf("\"Rows\"");
            if (rowsName < 0)
                throw new FormatException("expected.json does not contain Rows.");

            int arrayStart = json.IndexOf('[', rowsName);
            if (arrayStart < 0)
                throw new FormatException("Rows array is invalid.");

            int arrayEnd = FindJsonArrayEnd(json, arrayStart);
            if (arrayEnd < 0)
                throw new FormatException("Rows array is not closed.");

            string body = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
            List<string> rows = new List<string>();
            MatchCollection values = Regex.Matches(body,
                "\\\"((?:\\\\\\\\.|[^\\\"\\\\\\\\])*)\\\"");

            for (int i = 0; i < values.Count; i++)
                rows.Add(UnescapeJson(values[i].Groups[1].Value));

            return rows.ToArray();
        }

        private static int FindJsonArrayEnd(string json, int arrayStart)
        {
            bool inString = false;
            bool escaped = false;
            int depth = 0;

            for (int i = arrayStart; i < json.Length; i++)
            {
                char ch = json[i];

                if (inString)
                {
                    if (escaped) escaped = false;
                    else if (ch == '\\') escaped = true;
                    else if (ch == '\"') inString = false;
                    continue;
                }

                if (ch == '\"')
                {
                    inString = true;
                    continue;
                }

                if (ch == '[') depth++;
                else if (ch == ']')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }

            return -1;
        }

        private static string UnescapeJson(string value)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] != '\\' || i + 1 >= value.Length)
                {
                    b.Append(value[i]);
                    continue;
                }

                char n = value[++i];
                if (n == 'n') b.Append('\n');
                else if (n == 'r') b.Append('\r');
                else if (n == 't') b.Append('\t');
                else if (n == '\"') b.Append('\"');
                else if (n == '\\') b.Append('\\');
                else { b.Append('\\'); b.Append(n); }
            }
            return b.ToString();
        }

        private static string NormalizeRow(string value)
        {
            if (value == null) return "";
            string[] parts = value.Split(new char[] { '|' });
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) b.Append("|");
                b.Append(parts[i].Trim());
            }
            return b.ToString();
        }
    }
}

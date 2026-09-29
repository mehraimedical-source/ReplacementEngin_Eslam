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

                    string error;
                    if (RowsEqual(rows, expectedRows, out error))
                    {
                        passed++;
                        report.AppendLine("PASS  " + name);
                    }
                    else
                    {
                        failed++;
                        report.AppendLine("FAIL  " + name);
                        report.AppendLine("      " + error);
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
        /// آرایه Rows را از expected.json می‌خواند.
        /// فایل Expected توسط Sample Repository تولید می‌شود؛ Parser کوچک زیر فقط همین Contract کنترل‌شده را می‌خواند.
        /// </summary>
        private static string[] ReadExpectedRows(string json)
        {
            Match section = Regex.Match(json,
                "\\\"Rows\\\"\\s*:\\s*\\[(.*?)\\]",
                RegexOptions.Singleline);

            if (!section.Success)
                throw new FormatException("expected.json does not contain Rows.");

            List<string> rows = new List<string>();
            MatchCollection values = Regex.Matches(section.Groups[1].Value,
                "\\\"((?:\\\\.|[^\\\"\\\\])*)\\\"");

            for (int i = 0; i < values.Count; i++)
                rows.Add(UnescapeJson(values[i].Groups[1].Value));

            return rows.ToArray();
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

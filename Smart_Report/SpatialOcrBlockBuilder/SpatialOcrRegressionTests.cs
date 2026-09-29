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
                    List<string[]> expectedClusters = ReadExpectedClusters(expectedJson);
                    List<string[]> expectedRegions = ReadExpectedRegions(expectedJson);

                    List<SpatialOcrItem> items = SpatialOcrJsonParser.Parse(ocrJson, options);
                    List<SpatialRow> rows = new AdaptiveRowDetector(options).Detect(items);

                    // هر لایه با Ground Truth مستقل خودش تست می‌شود:
                    // Rows برای RowDetector و ResolvedClusters برای ContextualClusterResolver.
                    // بنابراین سالم بودن Rowها نمی‌تواند خرابی Clusterها را پنهان کند.
                    HorizontalClusterDetector clusterDetector = new HorizontalClusterDetector(
                        new HorizontalClusteringOptions());
                    List<ContextualClusterResolution> resolved =
                        new ContextualClusterResolver(new ContextualClusterResolverOptions())
                        .Resolve(rows, clusterDetector);

                    // RegionDetector همان ResolvedClusters تأییدشده را مصرف می‌کند؛
                    // دوباره Resolve نمی‌کنیم تا هر لایه دقیقاً روی خروجی لایه قبل Regression شود.
                    List<SpatialRegion> regions = new SpatialRegionDetector(
                        new SpatialRegionOptions()).DetectResolved(resolved);

                    string rowError;
                    string resolvedError;
                    string regionError;
                    bool rowsPassed = RowsEqual(rows, expectedRows, out rowError);
                    bool resolvedPassed = ResolvedRowsEqual(resolved, expectedClusters, out resolvedError);
                    bool regionsPassed = RegionsEqual(resolved, regions, expectedRegions, out regionError);

                    if (rowsPassed && resolvedPassed && regionsPassed)
                    {
                        passed++;
                        report.AppendLine("PASS  " + name);
                        report.AppendLine("      Rows: PASS");
                        report.AppendLine("      ResolvedClusters: PASS");
                        report.AppendLine("      Regions: PASS");
                    }
                    else
                    {
                        failed++;
                        report.AppendLine("FAIL  " + name);
                        report.AppendLine("      Rows: " + (rowsPassed ? "PASS" : "FAIL - " + rowError));
                        report.AppendLine("      ResolvedClusters: " +
                            (resolvedPassed ? "PASS" : "FAIL - " + resolvedError));
                        report.AppendLine("      Regions: " +
                            (regionsPassed ? "PASS" : "FAIL - " + regionError));
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
        /// خروجی Resolver فقط با Ground Truth مستقل ResolvedClusters مقایسه می‌شود.
        /// Rows جواب RowDetector است و هرگز به عنوان Expected لایه Cluster استفاده نمی‌شود.
        /// </summary>
        private static bool ResolvedRowsEqual(IList<ContextualClusterResolution> resolved,
            IList<string[]> expectedClusters, out string error)
        {
            if (expectedClusters == null)
            {
                error = "expected.json does not contain ResolvedClusters.";
                return false;
            }

            if (resolved.Count != expectedClusters.Count)
            {
                error = "Resolved row count: expected " + expectedClusters.Count +
                    ", actual " + resolved.Count;
                return false;
            }

            for (int i = 0; i < expectedClusters.Count; i++)
            {
                IList<SpatialCluster> actual = resolved[i].ResolvedClusters;
                string[] expected = expectedClusters[i];

                if (actual.Count != expected.Length)
                {
                    error = "Row " + (i + 1) + " cluster count: expected " +
                        expected.Length + ", actual " + actual.Count +
                        ". Actual " + BuildClusterList(actual);
                    return false;
                }

                for (int j = 0; j < expected.Length; j++)
                {
                    if (NormalizeRow(actual[j].GetText()) != NormalizeRow(expected[j]))
                    {
                        error = "Row " + (i + 1) + " Cluster " + (j + 1) +
                            ": expected [" + expected[j] + "] actual [" +
                            actual[j].GetText() + "]";
                        return false;
                    }
                }
            }

            error = null;
            return true;
        }

        private static string BuildClusterList(IList<SpatialCluster> clusters)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < clusters.Count; i++)
            {
                if (i > 0) b.Append(" || ");
                b.Append("[");
                b.Append(clusters[i].GetText());
                b.Append("]");
            }
            return b.ToString();
        }

        /// <summary>
        /// Regionها را با Ground Truth مستقل مقایسه می‌کند.
        /// ترتیب خود Regionها مهم نیست، ولی ترتیب Clusterهای داخل هر Region باید حفظ شود.
        /// همچنین هر Cluster Resolve شده باید دقیقاً یک بار داخل Regionها مصرف شده باشد.
        /// </summary>
        private static bool RegionsEqual(IList<ContextualClusterResolution> resolved,
            IList<SpatialRegion> actualRegions, IList<string[]> expectedRegions, out string error)
        {
            if (expectedRegions == null)
            {
                error = "expected.json does not contain Regions.";
                return false;
            }

            string coverageError;
            if (!RegionCoverageIsValid(resolved, actualRegions, out coverageError))
            {
                error = coverageError;
                return false;
            }

            if (actualRegions.Count != expectedRegions.Count)
            {
                error = "Region count: expected " + expectedRegions.Count +
                    ", actual " + actualRegions.Count + ". Actual " +
                    BuildRegionList(actualRegions);
                return false;
            }

            bool[] used = new bool[actualRegions.Count];

            for (int i = 0; i < expectedRegions.Count; i++)
            {
                int matched = -1;
                for (int j = 0; j < actualRegions.Count; j++)
                {
                    if (used[j]) continue;
                    if (RegionMatches(actualRegions[j], expectedRegions[i]))
                    {
                        matched = j;
                        break;
                    }
                }

                if (matched < 0)
                {
                    error = "Expected Region " + (i + 1) + " not found: [" +
                        JoinExpectedRegion(expectedRegions[i]) + "]. Actual " +
                        BuildRegionList(actualRegions);
                    return false;
                }

                used[matched] = true;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// تضمین می‌کند RegionDetector هیچ Cluster را گم یا Duplicate نکرده باشد.
        /// این تست مستقل از متن Expected است و خود Contract لایه Region را کنترل می‌کند.
        /// </summary>
        private static bool RegionCoverageIsValid(IList<ContextualClusterResolution> resolved,
            IList<SpatialRegion> regions, out string error)
        {
            List<SpatialCluster> source = new List<SpatialCluster>();
            for (int i = 0; i < resolved.Count; i++)
                for (int j = 0; j < resolved[i].ResolvedClusters.Count; j++)
                    source.Add(resolved[i].ResolvedClusters[j]);

            int actualCount = 0;
            for (int i = 0; i < regions.Count; i++)
                actualCount += regions[i].Clusters.Count;

            if (actualCount != source.Count)
            {
                error = "Region coverage count: expected " + source.Count +
                    " resolved clusters, actual " + actualCount;
                return false;
            }

            for (int i = 0; i < source.Count; i++)
            {
                int hits = 0;
                for (int r = 0; r < regions.Count; r++)
                    for (int c = 0; c < regions[r].Clusters.Count; c++)
                        if (object.ReferenceEquals(source[i], regions[r].Clusters[c]))
                            hits++;

                if (hits != 1)
                {
                    error = "Resolved cluster [" + source[i].GetText() +
                        "] appears in Regions " + hits + " times.";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private static bool RegionMatches(SpatialRegion region, string[] expected)
        {
            if (region.Clusters.Count != expected.Length) return false;

            for (int i = 0; i < expected.Length; i++)
                if (NormalizeRow(region.Clusters[i].GetText()) != NormalizeRow(expected[i]))
                    return false;

            return true;
        }

        private static string JoinExpectedRegion(string[] region)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < region.Length; i++)
            {
                if (i > 0) b.Append(" / ");
                b.Append(region[i]);
            }
            return b.ToString();
        }

        private static string BuildRegionList(IList<SpatialRegion> regions)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < regions.Count; i++)
            {
                if (i > 0) b.Append(" || ");
                b.Append("{");
                for (int j = 0; j < regions[i].Clusters.Count; j++)
                {
                    if (j > 0) b.Append(" / ");
                    b.Append(regions[i].Clusters[j].GetText());
                }
                b.Append("}");
            }
            return b.ToString();
        }

        private static List<string[]> ReadExpectedRegions(string json)
        {
            return ReadExpectedMatrix(json, "Regions");
        }

        /// <summary>
        /// ماتریس ResolvedClusters را از expected.json می‌خواند.
        /// هر عضو بیرونی یک Physical Row و هر String داخلی یک Cluster مورد انتظار است.
        /// </summary>
        private static List<string[]> ReadExpectedClusters(string json)
        {
            return ReadExpectedMatrix(json, "ResolvedClusters");
        }

        /// <summary>
        /// یک Property دوبعدی String[][] را از expected.json می‌خواند.
        /// Parser عمداً کوچک و محدود به Contract بانک Sample است تا با .NET 2.0 سازگار بماند.
        /// </summary>
        private static List<string[]> ReadExpectedMatrix(string json, string propertyName)
        {
            int name = json.IndexOf("\"" + propertyName + "\"");
            if (name < 0) return null;

            int outerStart = json.IndexOf('[', name);
            if (outerStart < 0)
                throw new FormatException(propertyName + " array is invalid.");

            int outerEnd = FindJsonArrayEnd(json, outerStart);
            if (outerEnd < 0)
                throw new FormatException(propertyName + " array is not closed.");

            List<string[]> result = new List<string[]>();
            int position = outerStart + 1;

            while (position < outerEnd)
            {
                int rowStart = json.IndexOf('[', position);
                if (rowStart < 0 || rowStart >= outerEnd) break;

                int rowEnd = FindJsonArrayEnd(json, rowStart);
                if (rowEnd < 0 || rowEnd > outerEnd)
                    throw new FormatException(propertyName + " row is invalid.");

                string body = json.Substring(rowStart + 1, rowEnd - rowStart - 1);
                MatchCollection values = Regex.Matches(body,
                    "\\\"((?:\\\\\\\\.|[^\\\"\\\\\\\\])*)\\\"");

                List<string> row = new List<string>();
                for (int i = 0; i < values.Count; i++)
                    row.Add(UnescapeJson(values[i].Groups[1].Value));

                result.Add(row.ToArray());
                position = rowEnd + 1;
            }

            return result;
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

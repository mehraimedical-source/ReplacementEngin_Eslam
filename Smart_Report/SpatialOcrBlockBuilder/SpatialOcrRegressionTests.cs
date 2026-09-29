using System;
using System.Collections.Generic;
using System.Text;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    /// <summary>
    /// یک تست رگرسیون برای Spatial OCR.
    /// ورودی تست همان JSON واقعی OCR شامل Text و BoxPoints است.
    /// </summary>
    public sealed class SpatialOcrRegressionCase
    {
        public string Name;
        public string OcrJson;
        public string[] ExpectedRows;
    }

    /// <summary>
    /// تست‌های رگرسیون مرحله Row Detection را اجرا می‌کند.
    /// هدف این کلاس این است که تغییر Heuristicها باعث خراب شدن Layoutهای قبلی نشود.
    /// </summary>
    public sealed class SpatialOcrRegressionTests
    {
        /// <summary>
        /// تمام Caseهای ثبت شده را با تنظیمات فعلی اجرا کرده و گزارش متنی PASS/FAIL می‌سازد.
        /// </summary>
        public string Run(RowDetectionOptions options)
        {
            if (options == null)
                options = new RowDetectionOptions();

            List<SpatialOcrRegressionCase> cases = CreateCases();
            StringBuilder report = new StringBuilder();

            int passed = 0;
            int failed = 0;

            for (int i = 0; i < cases.Count; i++)
            {
                SpatialOcrRegressionCase test = cases[i];

                try
                {
                    List<SpatialOcrItem> items = SpatialOcrJsonParser.Parse(test.OcrJson, options);
                    List<SpatialRow> rows = new AdaptiveRowDetector(options).Detect(items);

                    string error;
                    if (ContainsExpectedRows(rows, test.ExpectedRows, out error))
                    {
                        passed++;
                        report.AppendLine("PASS  " + test.Name);
                    }
                    else
                    {
                        failed++;
                        report.AppendLine("FAIL  " + test.Name);
                        report.AppendLine("      " + error);
                        report.AppendLine("      Actual rows:");

                        for (int r = 0; r < rows.Count; r++)
                            report.AppendLine("        [" + rows[r].GetText() + "]");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    report.AppendLine("FAIL  " + test.Name);
                    report.AppendLine("      Exception: " + ex.Message);
                }

                report.AppendLine();
            }

            report.Insert(0,
                "Spatial OCR Regression Tests" + Environment.NewLine +
                "Tests: " + cases.Count +
                "   Passed: " + passed +
                "   Failed: " + failed +
                Environment.NewLine + Environment.NewLine);

            return report.ToString();
        }

        /// <summary>
        /// بررسی می‌کند تمام Rowهای مورد انتظار در خروجی وجود داشته باشند.
        /// فعلاً ترتیب Rowها نیز باید حفظ شود، ولی وجود Rowهای اضافی مجاز است؛
        /// چون هنوز وارد مرحله Region/Noise Filtering نشده‌ایم.
        /// </summary>
        private static bool ContainsExpectedRows(
            IList<SpatialRow> actualRows,
            string[] expectedRows,
            out string error)
        {
            int searchFrom = 0;

            for (int e = 0; e < expectedRows.Length; e++)
            {
                string expected = NormalizeRow(expectedRows[e]);
                bool found = false;

                for (int a = searchFrom; a < actualRows.Count; a++)
                {
                    string actual = NormalizeRow(actualRows[a].GetText());

                    if (actual == expected)
                    {
                        found = true;
                        searchFrom = a + 1;
                        break;
                    }
                }

                if (!found)
                {
                    error = "Expected row not found: [" + expectedRows[e] + "]";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        /// فقط فاصله‌های غیرضروری را برای مقایسه تست یکسان می‌کند.
        /// محتوای واقعی OCR تغییر داده نمی‌شود.
        /// </summary>
        private static string NormalizeRow(string value)
        {
            if (value == null)
                return "";

            string[] parts = value.Split(new char[] { '|' });
            StringBuilder b = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                    b.Append("|");

                b.Append(parts[i].Trim());
            }

            return b.ToString();
        }

        /// <summary>
        /// Caseهای Spatial واقعی پروژه در این قسمت نگهداری می‌شوند.
        /// با دریافت JSONهای واقعی Vendorهای مختلف، همین مجموعه را گسترش می‌دهیم.
        /// </summary>
        private static List<SpatialOcrRegressionCase> CreateCases()
        {
            List<SpatialOcrRegressionCase> cases = new List<SpatialOcrRegressionCase>();

            cases.Add(new SpatialOcrRegressionCase
            {
                Name = "WS80 - BPD and HC measurement panel",
                OcrJson = @"[
{""BoxPoints"":[{""X"":604,""Y"":330},{""X"":652,""Y"":330},{""X"":652,""Y"":356},{""X"":604,""Y"":356}],""Score"":0.9995,""Text"":""GA""},
{""BoxPoints"":[{""X"":794,""Y"":326},{""X"":900,""Y"":328},{""X"":900,""Y"":357},{""X"":794,""Y"":355}],""Score"":0.9997,""Text"":""17w6d""},
{""BoxPoints"":[{""X"":604,""Y"":364},{""X"":668,""Y"":364},{""X"":668,""Y"":390},{""X"":604,""Y"":390}],""Score"":0.9999,""Text"":""BPD""},
{""BoxPoints"":[{""X"":735,""Y"":365},{""X"":895,""Y"":365},{""X"":895,""Y"":392},{""X"":735,""Y"":392}],""Score"":0.9526,""Text"":""39.35 mm""},
{""BoxPoints"":[{""X"":613,""Y"":398},{""X"":660,""Y"":398},{""X"":660,""Y"":425},{""X"":613,""Y"":425}],""Score"":0.9998,""Text"":""GA""},
{""BoxPoints"":[{""X"":735,""Y"":399},{""X"":899,""Y"":399},{""X"":899,""Y"":423},{""X"":735,""Y"":423}],""Score"":0.9999,""Text"":""17w6d±8d""},
{""BoxPoints"":[{""X"":611,""Y"":429},{""X"":685,""Y"":429},{""X"":685,""Y"":458},{""X"":611,""Y"":458}],""Score"":0.9987,""Text"":""Pctl.""},
{""BoxPoints"":[{""X"":735,""Y"":430},{""X"":871,""Y"":430},{""X"":871,""Y"":457},{""X"":735,""Y"":457}],""Score"":0.9994,""Text"":""55.48%""},
{""BoxPoints"":[{""X"":613,""Y"":464},{""X"":681,""Y"":464},{""X"":681,""Y"":489},{""X"":613,""Y"":489}],""Score"":0.9999,""Text"":""EDD""},
{""BoxPoints"":[{""X"":714,""Y"":463},{""X"":899,""Y"":463},{""X"":899,""Y"":490},{""X"":714,""Y"":490}],""Score"":0.9999,""Text"":""28-02-2027""},
{""BoxPoints"":[{""X"":604,""Y"":497},{""X"":650,""Y"":497},{""X"":650,""Y"":524},{""X"":604,""Y"":524}],""Score"":0.9999,""Text"":""HC""},
{""BoxPoints"":[{""X"":715,""Y"":496},{""X"":897,""Y"":498},{""X"":897,""Y"":525},{""X"":715,""Y"":523}],""Score"":0.9577,""Text"":""151.65 mm""},
{""BoxPoints"":[{""X"":611,""Y"":531},{""X"":659,""Y"":531},{""X"":659,""Y"":558},{""X"":611,""Y"":558}],""Score"":0.9997,""Text"":""GA""},
{""BoxPoints"":[{""X"":713,""Y"":531},{""X"":900,""Y"":531},{""X"":900,""Y"":558},{""X"":713,""Y"":558}],""Score"":0.9997,""Text"":""18w1d±10d""},
{""BoxPoints"":[{""X"":613,""Y"":564},{""X"":679,""Y"":564},{""X"":679,""Y"":590},{""X"":613,""Y"":590}],""Score"":0.9953,""Text"":""Pctl.""},
{""BoxPoints"":[{""X"":735,""Y"":563},{""X"":871,""Y"":563},{""X"":871,""Y"":590},{""X"":735,""Y"":590}],""Score"":0.9418,""Text"":""59.46 %""},
{""BoxPoints"":[{""X"":1,""Y"":592},{""X"":33,""Y"":592},{""X"":33,""Y"":611},{""X"":1,""Y"":611}],""Score"":0.9467,""Text"":""-10""},
{""BoxPoints"":[{""X"":613,""Y"":597},{""X"":683,""Y"":597},{""X"":683,""Y"":623},{""X"":613,""Y"":623}],""Score"":0.9999,""Text"":""EDD""},
{""BoxPoints"":[{""X"":710,""Y"":596},{""X"":899,""Y"":596},{""X"":899,""Y"":624},{""X"":710,""Y"":624}],""Score"":0.9999,""Text"":""26-02-2027""}
]",
                ExpectedRows = new string[]
                {
                    "GA | 17w6d",
                    "BPD | 39.35 mm",
                    "GA | 17w6d±8d",
                    "Pctl. | 55.48%",
                    "EDD | 28-02-2027",
                    "HC | 151.65 mm",
                    "GA | 18w1d±10d",
                    "Pctl. | 59.46 %",
                    "-10 | EDD | 26-02-2027"
                }
            });

            cases.Add(new SpatialOcrRegressionCase
            {
                Name = "Ratio table",
                OcrJson = @"[
{""BoxPoints"":[{""X"":23,""Y"":151},{""X"":106,""Y"":151},{""X"":106,""Y"":189},{""X"":23,""Y"":189}],""Score"":0.9995,""Text"":""Ratio""},
{""BoxPoints"":[{""X"":238,""Y"":149},{""X"":327,""Y"":151},{""X"":325,""Y"":189},{""X"":236,""Y"":187}],""Score"":0.9998,""Text"":""Value""},
{""BoxPoints"":[{""X"":510,""Y"":153},{""X"":709,""Y"":155},{""X"":709,""Y"":187},{""X"":510,""Y"":184}],""Score"":0.9999,""Text"":""Normal Range""},
{""BoxPoints"":[{""X"":23,""Y"":194},{""X"":140,""Y"":194},{""X"":140,""Y"":229},{""X"":23,""Y"":229}],""Score"":0.9999,""Text"":""FL/BPD""},
{""BoxPoints"":[{""X"":240,""Y"":193},{""X"":324,""Y"":193},{""X"":324,""Y"":231},{""X"":240,""Y"":231}],""Score"":0.9999,""Text"":""65.56""},
{""BoxPoints"":[{""X"":385,""Y"":196},{""X"":421,""Y"":196},{""X"":421,""Y"":231},{""X"":385,""Y"":231}],""Score"":0.9996,""Text"":""%""},
{""BoxPoints"":[{""X"":477,""Y"":194},{""X"":745,""Y"":197},{""X"":745,""Y"":231},{""X"":477,""Y"":228}],""Score"":0.9913,""Text"":""(71.0~87.0%, >23w)""},
{""BoxPoints"":[{""X"":23,""Y"":236},{""X"":120,""Y"":236},{""X"":120,""Y"":272},{""X"":23,""Y"":272}],""Score"":0.9998,""Text"":""FL/HC""},
{""BoxPoints"":[{""X"":241,""Y"":236},{""X"":325,""Y"":236},{""X"":325,""Y"":272},{""X"":241,""Y"":272}],""Score"":0.9999,""Text"":""17.13""},
{""BoxPoints"":[{""X"":385,""Y"":237},{""X"":422,""Y"":237},{""X"":422,""Y"":272},{""X"":385,""Y"":272}],""Score"":0.9996,""Text"":""%""},
{""BoxPoints"":[{""X"":473,""Y"":240},{""X"":751,""Y"":240},{""X"":751,""Y"":270},{""X"":473,""Y"":270}],""Score"":0.9886,""Text"":""(13.30~23.90%, 15...""},
{""BoxPoints"":[{""X"":23,""Y"":279},{""X"":124,""Y"":279},{""X"":124,""Y"":315},{""X"":23,""Y"":315}],""Score"":0.9999,""Text"":""HC/AC""},
{""BoxPoints"":[{""X"":246,""Y"":277},{""X"":315,""Y"":277},{""X"":315,""Y"":316},{""X"":246,""Y"":316}],""Score"":0.9999,""Text"":""1.24""},
{""BoxPoints"":[{""X"":476,""Y"":281},{""X"":750,""Y"":281},{""X"":750,""Y"":315},{""X"":476,""Y"":315}],""Score"":0.9701,""Text"":""(0.87~1.39, 13~42w)""}
]",
                ExpectedRows = new string[]
                {
                    "Ratio | Value | Normal Range",
                    "FL/BPD | 65.56 | % | (71.0~87.0%, >23w)",
                    "FL/HC | 17.13 | % | (13.30~23.90%, 15...",
                    "HC/AC | 1.24 | (0.87~1.39, 13~42w)"
                }
            });

            return cases;
        }
    }
}
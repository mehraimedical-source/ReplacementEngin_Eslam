using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Smart_Report
{
    // این کلاس فقط ساختار معنایی Blockهای OCR را تشخیص می‌دهد.
    // فعلاً Value Parsing انجام نمی‌دهد؛ یعنی مثلاً BPD=8.63 را استخراج نمی‌کند.
    // هدف مرحله اول:
    // ContentType -> Category -> Section -> Field
    //
    // مثال:
    // [OB]                      => Category = OB
    // [Fetal Biom... Last ...]  => Section = FetalBiometry
    // [BPD 8.63 ...]            => Field = BPD
    // [Fetal HR Last ...]       => Section = FetalHR
    // [Ratio Value Normal Range]=> Section = Ratio
    public class OcrContentAnalyzer
    {
        // خروجی موقت برای Debug و مشاهده نتیجه Analyzer است.
        // بعداً اگر ساختار مورد تایید بود، خروجی را به کلاس‌های مدل و JSON تبدیل می‌کنیم.
        public string Analyze(List<OcrBlock> blocks)
        {
            if (blocks == null || blocks.Count == 0)
                return "";

            string contentType = DetectContentType(blocks);

            string currentCategory = "PatientData";
            string currentSection = "";

            StringBuilder output = new StringBuilder();

            output.AppendLine("ContentType = " + contentType);

            for (int i = 0; i < blocks.Count; i++)
            {
                OcrBlock block = blocks[i];

                if (block == null || String.IsNullOrEmpty(block.Text))
                    continue;

                string text = block.Text.Trim();

                // تا قبل از رسیدن به OB همه Blockها را PatientData در نظر می‌گیریم.
                if (IsObMarker(text))
                {
                    currentCategory = "OB";
                    currentSection = "General";

                    AppendResult(
                        output,
                        i,
                        text,
                        currentCategory,
                        currentSection,
                        "SectionMarker");

                    continue;
                }

                if (currentCategory == "PatientData")
                {
                    int ratioHeaderIndex = IndexOf(text, "Ratio Value Normal Range");

                    // بعضی صفحات Report فقط Patient Header + یک Section تخصصی دارند
                    // و [OB] روی تصویر چاپ نشده است.
                    // اگر Marker تخصصی داخل همان Block بیمار دیده شد، متن قبل از Marker
                    // همچنان PatientData است و از Marker به بعد وارد Category=OB می‌شویم.
                    if (ratioHeaderIndex >= 0)
                    {
                        string patientPart = text.Substring(0, ratioHeaderIndex).Trim();

                        if (patientPart.Length > 0)
                        {
                            AppendResult(
                                output,
                                i,
                                patientPart,
                                "PatientData",
                                "",
                                DetectPatientField(patientPart));
                        }

                        currentCategory = "OB";
                        currentSection = "Ratio";

                        AppendResult(
                            output,
                            i,
                            "Ratio Value Normal Range",
                            currentCategory,
                            currentSection,
                            "SectionMarker");

                        string afterRatioHeader = text.Substring(
                            ratioHeaderIndex + "Ratio Value Normal Range".Length).Trim();

                        if (afterRatioHeader.Length > 0)
                        {
                            AnalyzeObBlock(
                                output,
                                i,
                                afterRatioHeader,
                                ref currentSection);
                        }

                        continue;
                    }

                    AppendResult(
                        output,
                        i,
                        text,
                        currentCategory,
                        "",
                        DetectPatientField(text));

                    continue;
                }

                // ممکن است Marker یک Section وسط همان Block آمده باشد.
                // مثال:
                // FL ... 14.25 Hadl... Fetal HR Last 1 2 3
                //
                // بنابراین Block را در صورت نیاز به چند قسمت معنایی تقسیم می‌کنیم.
                AnalyzeObBlock(
                    output,
                    i,
                    text,
                    ref currentSection);
            }

            return output.ToString();
        }

        // ContentType کلی را از مجموعه Blockها حدس می‌زند.
        // بعضی صفحات Report فقط یک Sub-Section مثل Ratio را نشان می‌دهند و [OB] ندارند.
        // بنابراین برای UltrasoundReport بودن، وجود [OB] الزامی نیست؛
        // ترکیب Patient Header + شواهد تخصصی Report نیز کافی است.
        private string DetectContentType(List<OcrBlock> blocks)
        {
            bool hasOb = false;
            bool hasPatientHeader = false;
            bool hasReportData = false;

            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] == null)
                    continue;

                string text = blocks[i].Text ?? "";

                if (IsObMarker(text))
                    hasOb = true;

                if (Contains(text, "Name") &&
                    Contains(text, "ID") &&
                    Contains(text, "Exam. Date"))
                {
                    hasPatientHeader = true;
                }

                if (Contains(text, "Fetal Biom") ||
                    Contains(text, "Fetal Long") ||
                    Contains(text, "BPD") ||
                    Contains(text, "HC") ||
                    Contains(text, "AC") ||
                    Contains(text, "EFW") ||
                    Contains(text, "Fetal HR") ||
                    Contains(text, "Ratio Value Normal Range") ||
                    Contains(text, "FL/AC") ||
                    Contains(text, "FL/BPD") ||
                    Contains(text, "FL/HC") ||
                    Contains(text, "HC/AC"))
                {
                    hasReportData = true;
                }
            }

            if ((hasOb && hasReportData) ||
                (hasPatientHeader && hasReportData))
            {
                return "UltrasoundReport";
            }

            return "Unknown";
        }

        // Blockهای داخل Category=OB را بر اساس Section جاری تحلیل می‌کند.
        private void AnalyzeObBlock(
            StringBuilder output,
            int blockIndex,
            string text,
            ref string currentSection)
        {
            if (String.IsNullOrEmpty(currentSection))
                currentSection = "General";

            string remaining = text;

            while (remaining.Length > 0)
            {
                string fetalBiometryMarker = null;
                int fetalBiometryIndex = IndexOf(remaining, "Fetal Biometry");

                if (fetalBiometryIndex >= 0)
                {
                    fetalBiometryMarker = "Fetal Biometry";
                }
                else
                {
                    fetalBiometryIndex = IndexOf(remaining, "Fetal Biom");

                    if (fetalBiometryIndex >= 0)
                        fetalBiometryMarker = "Fetal Biom";
                }

                int fetalLongIndex = IndexOf(remaining, "Fetal Long");
                int fetalHrIndex = FindFetalHrHeaderIndex(remaining);
                int ratioIndex = IndexOf(remaining, "Ratio Value Normal Range");

                int markerIndex = FindFirstPositiveIndex(
                    fetalBiometryIndex,
                    fetalLongIndex,
                    fetalHrIndex,
                    ratioIndex);

                // اگر Marker جدیدی در Block نیست، کل باقی‌مانده متعلق به Section جاری است.
                if (markerIndex < 0)
                {
                    AppendResult(
                        output,
                        blockIndex,
                        remaining,
                        "OB",
                        currentSection,
                        DetectObField(remaining, currentSection));

                    break;
                }

                // اگر قبل از Marker متن وجود دارد، آن قسمت هنوز متعلق به Section قبلی است.
                if (markerIndex > 0)
                {
                    string before = remaining.Substring(0, markerIndex).Trim();

                    if (before.Length > 0)
                    {
                        AppendResult(
                            output,
                            blockIndex,
                            before,
                            "OB",
                            currentSection,
                            DetectObField(before, currentSection));
                    }
                }

                // Marker پیدا شده را تشخیص می‌دهیم و State را عوض می‌کنیم.
                if (markerIndex == fetalBiometryIndex)
                {
                    currentSection = "FetalBiometry";

                    AppendResult(
                        output,
                        blockIndex,
                        "Fetal Biometry",
                        "OB",
                        currentSection,
                        "SectionMarker");

                    remaining = remaining.Substring(
                        markerIndex + fetalBiometryMarker.Length).Trim();

                    // OCRهای ناقص ممکن است Marker را به شکل "Fetal Biom..." بدهند.
                    // نقطه‌های باقی‌مانده بخشی از Header نیستند.
                    remaining = remaining.TrimStart('.', ' ', '\t');

                    continue;
                }

                if (markerIndex == fetalLongIndex)
                {
                    currentSection = "FetalLong";

                    AppendResult(
                        output,
                        blockIndex,
                        "Fetal Long Bones",
                        "OB",
                        currentSection,
                        "SectionMarker");

                    remaining = RemoveMarkerAndContinue(
                        remaining,
                        markerIndex,
                        "Fetal Long");

                    continue;
                }

                if (markerIndex == fetalHrIndex)
                {
                    currentSection = "FetalHR";

                    AppendResult(
                        output,
                        blockIndex,
                        "Fetal HR",
                        "OB",
                        currentSection,
                        "SectionMarker");

                    // Header معمولاً به صورت Fetal HR Last 1 2 3 دیده می‌شود.
                    // فقط Marker را حذف می‌کنیم و اگر Data دیگری باقی مانده باشد ادامه می‌دهیم.
                    Match hrHeader = Regex.Match(
                        remaining.Substring(markerIndex),
                        @"^Fetal\s+HR\s+Last\b[^\r\n]*$",
                        RegexOptions.IgnoreCase);

                    if (hrHeader.Success)
                        break;

                    remaining = remaining.Substring(markerIndex + "Fetal HR".Length).Trim();
                    continue;
                }

                if (markerIndex == ratioIndex)
                {
                    currentSection = "Ratio";

                    AppendResult(
                        output,
                        blockIndex,
                        "Ratio Value Normal Range",
                        "OB",
                        currentSection,
                        "SectionMarker");

                    remaining = remaining.Substring(
                        markerIndex + "Ratio Value Normal Range".Length).Trim();

                    continue;
                }
            }
        }

        // Field اولیه داخل Section را تشخیص می‌دهد.
        // این فقط Label است؛ Value Extraction در Analyzerهای بعدی انجام می‌شود.
        private string DetectObField(string text, string section)
        {
            if (section == "General")
            {
                if (StartsWith(text, "EDD(GA)")) return "EDD_GA";
                if (StartsWith(text, "EDD(AUA)")) return "EDD_AUA";
                if (StartsWith(text, "EDD(LMP)")) return "EDD_LMP";
                if (StartsWith(text, "AUA")) return "AUA";
                if (StartsWith(text, "EFW")) return "EFW";
                if (StartsWith(text, "GA(EFW)")) return "GA_EFW";
                return "GeneralData";
            }

            if (section == "FetalBiometry")
            {
                if (IsFetalBiometryTableHeader(text)) return "TableHeader";
                if (StartsWith(text, "BPD")) return "BPD";
                if (StartsWith(text, "HC")) return "HC";
                if (StartsWith(text, "AC")) return "AC";
                if (StartsWith(text, "FL")) return "FL";
                return "BiometryData";
            }

            if (section == "FetalLong")
            {
                if (StartsWith(text, "LT. HUM") || StartsWith(text, "LT HUM"))
                    return "LT_HUM";

                if (StartsWith(text, "ULNA"))
                    return "ULNA";

                if (StartsWith(text, "RT. TIB") || StartsWith(text, "RT TIB"))
                    return "RT_TIB";

                if (StartsWith(text, "RT. HUM") || StartsWith(text, "RT HUM"))
                    return "RT_HUM";

                if (StartsWith(text, "LT. TIB") || StartsWith(text, "LT TIB"))
                    return "LT_TIB";

                return "FetalLongData";
            }

            if (section == "FetalHR")
            {
                if (StartsWith(text, "Fetal HR") || StartsWith(text, "FHR"))
                    return "FHR";

                return "FetalHRData";
            }

            if (section == "Ratio")
            {
                if (StartsWith(text, "FL/AC")) return "FL_AC";
                if (StartsWith(text, "FL/BPD")) return "FL_BPD";
                if (StartsWith(text, "FL/HC")) return "FL_HC";
                if (StartsWith(text, "HC/AC")) return "HC_AC";
                return "RatioData";
            }

            return "Unknown";
        }

        // Header ستون‌های جدول Biometry داده پزشکی نیست، اما برای حفظ Raw Data
        // آن را حذف نمی‌کنیم و با Label مستقل نگه می‌داریم.
        private bool IsFetalBiometryTableHeader(string text)
        {
            if (String.IsNullOrEmpty (text))
                return false;

            string value = text.Trim();

            // Samsung: m1 m2 m3 GA GP
            if (Regex.IsMatch(
                value,
                @"^m1\s+m2\s+m3\s+GA\s+GP\b",
                RegexOptions.IgnoreCase))
            {
                return true;
            }

            // نمونه‌های OCR قبلی: Last 1 2 3 GA Pctl.
            if (Regex.IsMatch(
                value,
                @"^Last\s+1\s+2\s+3\s+GA\s+Pctl\.?\b",
                RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }

        // فعلاً PatientData را به صورت کلی Label می‌کنیم.
        // بعداً در کلاس جداگانه Name, ID, ExamDate, Gender و ... Parse می‌شوند.
        private string DetectPatientField(string text)
        {
            if (Contains(text, "Name") &&
                Contains(text, "ID") &&
                Contains(text, "Exam. Date"))
            {
                return "PatientStudyInfo";
            }

            return "PatientData";
        }

        // [OB]، [[OB]] و OB را به عنوان Marker اصلی Category می‌شناسد.
        private bool IsObMarker(string text)
        {
            if (text == null)
                return false;

            string clean = text.Trim();
            clean = clean.Trim('[', ']', ' ');

            return String.Compare(clean, "OB", true) == 0;
        }

        // Fetal HR فقط وقتی Header Section است که بعد از آن Last دیده شود.
        // این کار مانع می‌شود Data واقعی مثل "Fetal HR 123 bpm" اشتباهاً Marker شناخته شود.
        private int FindFetalHrHeaderIndex(string text)
        {
            Match m = Regex.Match(
                text,
                @"\bFetal\s+HR\s+Last\b",
                RegexOptions.IgnoreCase);

            if (!m.Success)
                return -1;

            return m.Index;
        }

        private int FindFirstPositiveIndex(int a, int b, int c, int d)
        {
            int result = -1;

            if (a >= 0)
                result = a;

            if (b >= 0 && (result < 0 || b < result))
                result = b;

            if (c >= 0 && (result < 0 || c < result))
                result = c;

            if (d >= 0 && (result < 0 || d < result))
                result = d;

            return result;
        }

        private string RemoveMarkerAndContinue(
            string text,
            int markerIndex,
            string markerStart)
        {
            string fromMarker = text.Substring(markerIndex);

            // اگر کل باقی‌مانده Header باشد، چیزی برای ادامه وجود ندارد.
            if (Contains(fromMarker, "Last") &&
                !Contains(fromMarker, "BPD") &&
                !Contains(fromMarker, "HC") &&
                !Contains(fromMarker, "AC") &&
                !Contains(fromMarker, "FL "))
            {
                return "";
            }

            return text.Substring(markerIndex + markerStart.Length).Trim();
        }

        private void AppendResult(
            StringBuilder output,
            int blockIndex,
            string text,
            string category,
            string section,
            string field)
        {
            output.Append("Block ");
            output.Append(blockIndex);
            output.Append(" | Category=");
            output.Append(category);
            output.Append(" | Section=");
            output.Append(section);
            output.Append(" | Field=");
            output.Append(field);
            output.Append(" | Text=");
            output.AppendLine(text);
        }

        private bool Contains(string text, string value)
        {
            return IndexOf(text, value) >= 0;
        }

        private int IndexOf(string text, string value)
        {
            if (text == null || value == null)
                return -1;

            return text.IndexOf(
                value,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool StartsWith(string text, string value)
        {
            if (text == null || value == null)
                return false;

            return text.TrimStart().StartsWith(
                value,
                StringComparison.OrdinalIgnoreCase);
        }

        // خروجی فعلی Analyzer را به JSON قابل مشاهده برای Debug تبدیل می‌کند.
        // فعلاً این JSON برای بررسی ساختار ContentType/Category/Section/Field است
        // و هنوز Valueهای پزشکی مثل BPD=8.63 یا GA=34w5d را Parse نمی‌کند.
        public string AnalyzeJson(List<OcrBlock> blocks)
        {
            string analyzed = Analyze(blocks);

            if (String.IsNullOrEmpty(analyzed))
                return "{\r\n  \"contentType\": \"Unknown\",\r\n  \"items\": []\r\n}";

            string[] lines = analyzed.Replace("\r\n", "\n").Split('\n');

            string contentType = "Unknown";
            List<string> itemJson = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.Length == 0)
                    continue;

                if (line.StartsWith("ContentType = "))
                {
                    contentType = line.Substring("ContentType = ".Length).Trim();
                    continue;
                }

                Match m = Regex.Match(
                    line,
                    @"^Block\s+(\d+)\s+\|\s+Category=(.*?)\s+\|\s+Section=(.*?)\s+\|\s+Field=(.*?)\s+\|\s+Text=(.*)$");

                if (!m.Success)
                    continue;

                StringBuilder item = new StringBuilder();
                item.Append("    {");
                item.Append("\"blockIndex\": ");
                item.Append(m.Groups[1].Value);
                item.Append(", \"category\": \"");
                item.Append(JsonEscape(m.Groups[2].Value));
                item.Append("\", \"section\": \"");
                item.Append(JsonEscape(m.Groups[3].Value));
                item.Append("\", \"field\": \"");
                item.Append(JsonEscape(m.Groups[4].Value));
                item.Append("\", \"text\": \"");
                item.Append(JsonEscape(m.Groups[5].Value));
                item.Append("\"}");

                itemJson.Add(item.ToString());
            }

            StringBuilder json = new StringBuilder();
            json.AppendLine("{");
            json.Append("  \"contentType\": \"");
            json.Append(JsonEscape(contentType));
            json.AppendLine("\",");
            json.AppendLine("  \"items\": [");

            for (int i = 0; i < itemJson.Count; i++)
            {
                json.Append(itemJson[i]);

                if (i < itemJson.Count - 1)
                    json.Append(",");

                json.AppendLine();
            }

            json.AppendLine("  ]");
            json.Append("}");

            return json.ToString();
        }

        // کاراکترهای خاص را Escape می‌کند تا Text خام OCR باعث خراب شدن JSON نشود.
        private string JsonEscape(string value)
        {
            if (value == null)
                return "";

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

    }
}

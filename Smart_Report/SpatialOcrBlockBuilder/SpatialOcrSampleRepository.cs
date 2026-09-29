using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    /// <summary>
    /// بانک دائمی Sampleهای OCR را مدیریت می‌کند.
    /// هر Sample مستقل از الگوریتم است و تصویر اصلی، JSON خام OCR و Expected تأییدشده را کنار هم نگه می‌دارد.
    /// </summary>
    public sealed class SpatialOcrSampleRepository
    {
        private readonly string rootPath;

        public SpatialOcrSampleRepository()
        {
            // هنگام اجرای پروژه از bin\Debug یا bin\Release دو سطح بالا می‌رویم تا Sampleها داخل خود پروژه بمانند.
            rootPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "OcrSamples"));
        }

        public string RootPath { get { return rootPath; } }

        /// <summary>
        /// تصویر، OCR خام و Rowهای فعلی را به عنوان یک Sample قابل تکرار ذخیره می‌کند.
        /// نام پوشه فقط از کاراکترهای معتبر فایل ساخته می‌شود تا Sample در Git نیز قابل نگهداری باشد.
        /// </summary>
        public string Save(string sampleName, Image image, string ocrJson, IList<SpatialRow> expectedRows)
        {
            if (image == null) throw new InvalidOperationException("Sample image is empty.");
            if (string.IsNullOrEmpty(ocrJson) || ocrJson.Trim().Length == 0)
                throw new InvalidOperationException("OCR JSON is empty.");
            if (expectedRows == null || expectedRows.Count == 0)
                throw new InvalidOperationException("Analyze the sample before saving it.");

            string safeName = MakeSafeName(sampleName);
            if (safeName.Length == 0)
                throw new InvalidOperationException("Sample name is empty.");

            string folder = Path.Combine(rootPath, safeName);
            if (Directory.Exists(folder))
                throw new InvalidOperationException("A sample with this name already exists: " + safeName);

            Directory.CreateDirectory(folder);
            image.Save(Path.Combine(folder, "image.png"), ImageFormat.Png);
            File.WriteAllText(Path.Combine(folder, "ocr.json"), ocrJson, Encoding.UTF8);
            File.WriteAllText(Path.Combine(folder, "expected.json"), BuildExpectedJson(expectedRows), Encoding.UTF8);
            return folder;
        }

        /// <summary>
        /// تمام پوشه‌هایی را برمی‌گرداند که حداقل OCR و Expected دارند.
        /// وجود تصویر برای Regression محاسباتی اجباری نیست، ولی در Sampleهای ذخیره‌شده توسط فرم همیشه وجود دارد.
        /// </summary>
        public List<string> GetSampleFolders()
        {
            List<string> result = new List<string>();
            if (!Directory.Exists(rootPath)) return result;

            string[] folders = Directory.GetDirectories(rootPath);
            Array.Sort(folders);
            for (int i = 0; i < folders.Length; i++)
            {
                if (File.Exists(Path.Combine(folders[i], "ocr.json")) &&
                    File.Exists(Path.Combine(folders[i], "expected.json")))
                    result.Add(folders[i]);
            }
            return result;
        }

        /// <summary>
        /// Expected را عمداً ساده و مستقل از Engine نگه می‌داریم تا در آینده Cluster/Region/Table نیز به آن افزوده شود.
        /// </summary>
        private static string BuildExpectedJson(IList<SpatialRow> rows)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("{");
            b.AppendLine("  \"Rows\": [");
            for (int i = 0; i < rows.Count; i++)
            {
                b.Append("    \"");
                b.Append(EscapeJson(rows[i].GetText()));
                b.Append("\"");
                if (i < rows.Count - 1) b.Append(",");
                b.AppendLine();
            }
            b.AppendLine("  ]");
            b.AppendLine("}");
            return b.ToString();
        }

        private static string EscapeJson(string value)
        {
            if (value == null) return "";
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"")
                .Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }

        private static string MakeSafeName(string value)
        {
            if (value == null) return "";
            string result = value.Trim();
            char[] invalid = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalid.Length; i++)
                result = result.Replace(invalid[i], '_');
            return result;
        }
    }
}

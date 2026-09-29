using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    public sealed partial class SpatialOcrTestForm : Form
    {
        /// <summary>
        /// سازنده فرم فقط اجزای ساخته شده توسط Windows Forms Designer را مقداردهی اولیه می‌کند.
        /// تمام کنترل‌ها، اندازه‌ها و چیدمان فرم در فایل Designer نگهداری می‌شوند.
        /// </summary>
        public SpatialOcrTestForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// تمام اطلاعات لازم برای بررسی یک Sample را در قالب متن استاندارد می‌سازد.
        /// این متن عمداً JSON خام را نیز نگه می‌دارد تا هیچ اطلاعات Spatial هنگام ارسال از بین نرود.
        /// </summary>
        private string BuildTestData()
        {
            RowDetectionOptions options = new RowDetectionOptions();
            options.MinVerticalOverlapRatio = (double)nudOverlap.Value;
            options.MaxCenterDistanceFactor = (double)nudCenter.Value;

            List<SpatialOcrItem> items = SpatialOcrJsonParser.Parse(txtJson.Text, options);
            List<SpatialRow> rows = new AdaptiveRowDetector(options).Detect(items);

            StringBuilder b = new StringBuilder();
            b.AppendLine("===== SPATIAL OCR TEST =====");
            b.AppendLine();
            b.AppendLine("[SETTINGS]");
            b.AppendLine("MinVerticalOverlapRatio=" +
                options.MinVerticalOverlapRatio.ToString("0.00", CultureInfo.InvariantCulture));
            b.AppendLine("MaxCenterDistanceFactor=" +
                options.MaxCenterDistanceFactor.ToString("0.00", CultureInfo.InvariantCulture));
            b.AppendLine("OCR Items=" + items.Count.ToString(CultureInfo.InvariantCulture));
            b.AppendLine("Detected Rows=" + rows.Count.ToString(CultureInfo.InvariantCulture));
            b.AppendLine();
            b.AppendLine("[OCR JSON]");
            b.AppendLine(txtJson.Text);
            b.AppendLine();
            b.AppendLine("[DETECTED ROWS]");

            for (int i = 0; i < rows.Count; i++)
            {
                SpatialRow row = rows[i];
                b.Append("Row ");
                b.Append((i + 1).ToString(CultureInfo.InvariantCulture));
                b.Append("  Y=");
                b.Append(row.Bounds.Top.ToString("0.0", CultureInfo.InvariantCulture));
                b.Append("..");
                b.Append(row.Bounds.Bottom.ToString("0.0", CultureInfo.InvariantCulture));
                b.Append("  H=");
                b.Append(row.MedianHeight.ToString("0.0", CultureInfo.InvariantCulture));
                b.Append("  Items=");
                b.Append(row.Items.Count.ToString(CultureInfo.InvariantCulture));
                b.Append("  : ");
                b.AppendLine(row.GetText());
            }

            return b.ToString();
        }

        /// <summary>
        /// اطلاعات Sample جاری را در Clipboard قرار می‌دهد تا کاربر بتواند مستقیم در ChatGPT Paste کند.
        /// </summary>
        private void CopyTestData()
        {
            try
            {
                string data = BuildTestData();
                Clipboard.SetText(data);
                lblStatus.Text = "Test data copied to Clipboard";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Copy Test Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// اطلاعات Sample جاری را بدون تغییر در یک فایل متنی ذخیره می‌کند تا برای Regressionهای بعدی آرشیو شود.
        /// </summary>
        private void SaveTestData()
        {
            try
            {
                string data = BuildTestData();

                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Title = "Save Spatial OCR Test Data";
                    dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    dialog.DefaultExt = "txt";
                    dialog.FileName = "SpatialOcrTestData.txt";

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        File.WriteAllText(dialog.FileName, data, Encoding.UTF8);
                        lblStatus.Text = "Test data saved";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Save Test Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// تست‌های رگرسیون Spatial را اجرا می‌کند و نتیجه را در یک پنجره متنی مستقل نمایش می‌دهد.
        /// این تست‌ها با توسعه مراحل بعدی Engine (Cluster/Region/Table) گسترش خواهند یافت.
        /// </summary>
        private void RunRegressionTests()
        {
            RowDetectionOptions options = new RowDetectionOptions();
            options.MinVerticalOverlapRatio = (double)nudOverlap.Value;
            options.MaxCenterDistanceFactor = (double)nudCenter.Value;

            SpatialOcrRegressionTests tests = new SpatialOcrRegressionTests();
            string result = tests.Run(options);

            Form reportForm = new Form();
            reportForm.Text = "Spatial OCR Regression Tests";
            reportForm.StartPosition = FormStartPosition.CenterParent;
            reportForm.Width = 950;
            reportForm.Height = 700;

            TextBox output = new TextBox();
            output.Multiline = true;
            output.ReadOnly = true;
            output.ScrollBars = ScrollBars.Both;
            output.WordWrap = false;
            output.Dock = DockStyle.Fill;
            output.Font = new Font("Consolas", 9F);
            output.Text = result;

            reportForm.Controls.Add(output);
            reportForm.ShowDialog(this);
        }

        /// <summary>
        /// JSON خام OCR را Parse کرده و Rowهای فیزیکی صفحه را با الگوریتم Adaptive بازسازی می‌کند.
        /// </summary>
        private void Analyze()
        {
            try
            {
                RowDetectionOptions options = new RowDetectionOptions();
                options.MinVerticalOverlapRatio = (double)nudOverlap.Value;
                options.MaxCenterDistanceFactor = (double)nudCenter.Value;

                List<SpatialOcrItem> items = SpatialOcrJsonParser.Parse(txtJson.Text, options);
                List<SpatialRow> rows = new AdaptiveRowDetector(options).Detect(items);

                gridRows.Rows.Clear();
                for (int i = 0; i < rows.Count; i++)
                {
                    SpatialRow row = rows[i];
                    gridRows.Rows.Add(
                        (i + 1).ToString(CultureInfo.InvariantCulture),
                        row.Bounds.Top.ToString("0.0", CultureInfo.InvariantCulture) + " .. " + row.Bounds.Bottom.ToString("0.0", CultureInfo.InvariantCulture),
                        row.MedianHeight.ToString("0.0", CultureInfo.InvariantCulture),
                        row.Items.Count.ToString(CultureInfo.InvariantCulture),
                        row.GetText());
                }
                lblStatus.Text = items.Count + " OCR items  |  " + rows.Count + " rows";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Spatial OCR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Label MakeLabel(string text, int left)
        {
            Label l = new Label(); l.Text = text; l.AutoSize = true; l.Left = left; l.Top = 9; return l;
        }

        private static void SetupNumeric(NumericUpDown n, int left, decimal value)
        {
            n.Left = left; n.Top = 5; n.Width = 75; n.DecimalPlaces = 2;
            n.Increment = 0.05M; n.Minimum = 0; n.Maximum = 2; n.Value = value;
        }
    }
}
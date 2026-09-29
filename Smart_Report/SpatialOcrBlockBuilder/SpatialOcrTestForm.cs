using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Smart_Report.SpatialOcrBlockBuilder
{
    public sealed class SpatialOcrTestForm : Form
    {
        private readonly TextBox txtJson = new TextBox();
        private readonly DataGridView gridRows = new DataGridView();
        private readonly Label lblStatus = new Label();
        private readonly NumericUpDown nudOverlap = new NumericUpDown();
        private readonly NumericUpDown nudCenter = new NumericUpDown();

        public SpatialOcrTestForm()
        {
            Text = "Spatial OCR Block Builder - Row Detector Test";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1250; Height = 780;

            ToolStrip bar = new ToolStrip();
            ToolStripButton run = new ToolStripButton("Analyze Rows");
            ToolStripButton tests = new ToolStripButton("Run Regression Tests");
            ToolStripButton clear = new ToolStripButton("Clear");
            ToolStripButton legacy = new ToolStripButton("Legacy Form");
            run.Click += delegate { Analyze(); };
            // اجرای تست‌های رگرسیون Spatial با همان پارامترهایی که روی فرم تنظیم شده‌اند.
            tests.Click += delegate { RunRegressionTests(); };
            clear.Click += delegate { txtJson.Clear(); gridRows.Rows.Clear(); lblStatus.Text = ""; };
            legacy.Click += delegate { new Smart_Report.Form1().Show(); };
            bar.Items.Add(run); bar.Items.Add(tests); bar.Items.Add(clear); bar.Items.Add(new ToolStripSeparator()); bar.Items.Add(legacy);

            Panel settings = new Panel(); settings.Dock = DockStyle.Top; settings.Height = 34;
            settings.Controls.Add(MakeLabel("Min overlap", 8));
            SetupNumeric(nudOverlap, 90, 0.45M); settings.Controls.Add(nudOverlap);
            settings.Controls.Add(MakeLabel("Max center factor", 180));
            SetupNumeric(nudCenter, 290, 0.55M); settings.Controls.Add(nudCenter);
            lblStatus.AutoSize = true; lblStatus.Left = 405; lblStatus.Top = 9; settings.Controls.Add(lblStatus);

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill; split.Orientation = Orientation.Horizontal; split.SplitterDistance = 300;

            txtJson.Multiline = true; txtJson.ScrollBars = ScrollBars.Both; txtJson.WordWrap = false;
            txtJson.Dock = DockStyle.Fill; txtJson.Font = new Font("Consolas", 9F);
            split.Panel1.Controls.Add(txtJson);

            gridRows.Dock = DockStyle.Fill; gridRows.AllowUserToAddRows = false; gridRows.ReadOnly = true;
            gridRows.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridRows.Columns.Add("Row", "#");
            gridRows.Columns.Add("Y", "Y Range");
            gridRows.Columns.Add("Height", "Median H");
            gridRows.Columns.Add("Items", "Items");
            gridRows.Columns.Add("Text", "Reconstructed Row");
            gridRows.Columns[0].FillWeight = 10; gridRows.Columns[1].FillWeight = 18;
            gridRows.Columns[2].FillWeight = 15; gridRows.Columns[3].FillWeight = 12;
            gridRows.Columns[4].FillWeight = 120;
            split.Panel2.Controls.Add(gridRows);

            Controls.Add(split); Controls.Add(settings); Controls.Add(bar);
            bar.Dock = DockStyle.Top; settings.BringToFront();
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
                        row.Bounds.Top.ToString("0.0") + " .. " + row.Bounds.Bottom.ToString("0.0"),
                        row.MedianHeight.ToString("0.0"),
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
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
        private Image _img;


        public SpatialOcrTestForm()
            : this(null, String.Empty)
        {
        }


        /// <summary>
        /// تصویر اصلی و JSON همان OCR را کنار هم نگه می‌دارد تا تست Spatial همیشه
        /// به منبع تصویری واقعی خود متصل باشد و در مراحل بعد بتوان Overlay را روی همان تصویر رسم کرد.
        /// </summary>

        public SpatialOcrTestForm(Image img, string json)
        {
            InitializeComponent();

            _img = img;
            json = json == null ? String.Empty : json;
            txtJson.Text = json;

            // اگر فرم از مسیر اصلی برنامه همراه تصویر باز شده باشد، همان لحظه تصویر
            // در کنار JSON نمایش داده می‌شود تا بررسی Spatial بدون مراجعه به پنجره دیگری ممکن باشد.
            picSource.Image = _img;
        }



        /// <summary>
        /// Sample ذخیره‌شده را دوباره وارد محیط تست می‌کند.
        /// OCR دقیقاً از ocr.json خوانده می‌شود و تصویر نیز به صورت Clone بارگذاری می‌شود
        /// تا فایل image.png بعد از Load قفل نماند و Sample در ادامه قابل مدیریت باشد.
        /// </summary>
        private void LoadSample()
        {
            try
            {
                SpatialOcrSampleRepository repository = new SpatialOcrSampleRepository();
                string folder;
                string imagePath;
                string ocrPath;
                string expectedPath;

                if (!repository.TryGetSample(txtSampleName.Text, out folder, out imagePath,
                    out ocrPath, out expectedPath))
                    throw new InvalidOperationException(
                        "Sample not found or incomplete: " + txtSampleName.Text);

                string json = File.ReadAllText(ocrPath, Encoding.UTF8);
                Image loadedImage;
                using (Image source = Image.FromFile(imagePath))
                    loadedImage = new Bitmap(source);

                if (_img != null)
                    _img.Dispose();

                _img = loadedImage;
                picSource.Image = _img;
                txtJson.Text = json;
                gridRows.Rows.Clear();
                txtRegionText.Clear();
                txtSampleName.Text = Path.GetFileName(folder);
                lblStatus.Text = "Sample loaded: " + Path.GetFileName(folder);

                // بعد از Load همان مسیر واقعی Engine اجرا می‌شود؛ Expected ورودی Analyze نیست.
                Analyze();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Load Sample",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Sample جاری را شامل تصویر اصلی، JSON خام OCR و Expected Rowها در بانک دائمی پروژه ذخیره می‌کند.
        /// Expected از نتیجه‌ای ساخته می‌شود که اکنون در Test Form تحلیل شده و بعداً می‌تواند دستی بازبینی شود.
        /// </summary>
        private void SaveSample()
        {
            try
            {
                RowDetectionOptions options = new RowDetectionOptions();
                options.MinVerticalOverlapRatio = (double)nudOverlap.Value;
                options.MaxCenterDistanceFactor = (double)nudCenter.Value;

                List<SpatialOcrItem> items = SpatialOcrJsonParser.Parse(txtJson.Text, options);
                List<SpatialRow> rows = new AdaptiveRowDetector(options).Detect(items);

                SpatialOcrSampleRepository repository = new SpatialOcrSampleRepository();
                string folder = repository.Save(txtSampleName.Text, _img, txtJson.Text, rows);

                lblStatus.Text = "Sample saved: " + Path.GetFileName(folder);
                MessageBox.Show(this,
                    "Sample saved successfully." + Environment.NewLine + folder,
                    "Spatial OCR Sample",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Save Sample",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            b.AppendLine("Cluster Gap Height Factor=6.00");
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

                // Clusterها را جداگانه گزارش می‌کنیم تا Row فیزیکی دست‌نخورده بماند
                // و بتوانیم نتیجه لایه دوم Spatial را مستقل بررسی کنیم.
                List<SpatialCluster> clusters = new HorizontalClusterDetector(
                    new HorizontalClusteringOptions()).Detect(row);
                for (int j = 0; j < clusters.Count; j++)
                {
                    b.Append("    Cluster ");
                    b.Append((j + 1).ToString(CultureInfo.InvariantCulture));
                    b.Append("  GapBefore=");
                    b.Append(clusters[j].GapBefore.ToString("0.0", CultureInfo.InvariantCulture));
                    b.Append("  : ");
                    b.AppendLine(clusters[j].GetText());
                }
            }

            HorizontalClusterDetector regionClusterDetector = new HorizontalClusterDetector(
                new HorizontalClusteringOptions());
            List<SpatialRegion> regions = new SpatialRegionDetector(
                new SpatialRegionOptions()).Detect(rows, regionClusterDetector);

            b.AppendLine();
            b.AppendLine("[DETECTED REGIONS]");
            for (int i = 0; i < regions.Count; i++)
            {
                b.Append("Region ");
                b.Append((i + 1).ToString(CultureInfo.InvariantCulture));
                b.Append("  X=");
                b.Append(regions[i].Bounds.Left.ToString("0.0", CultureInfo.InvariantCulture));
                b.Append("..");
                b.Append(regions[i].Bounds.Right.ToString("0.0", CultureInfo.InvariantCulture));
                b.Append("  : ");
                b.AppendLine(regions[i].GetText());
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
                HorizontalClusterDetector clusterDetector = new HorizontalClusterDetector(
                    new HorizontalClusteringOptions());

                gridRows.Rows.Clear();
                for (int i = 0; i < rows.Count; i++)
                {
                    SpatialRow row = rows[i];
                    List<SpatialCluster> clusters = clusterDetector.Detect(row);
                    StringBuilder clusterText = new StringBuilder();
                    for (int j = 0; j < clusters.Count; j++)
                    {
                        if (j > 0) clusterText.Append(" || ");
                        clusterText.Append("[");
                        clusterText.Append(clusters[j].GetText());
                        clusterText.Append("]");
                    }

                    gridRows.Rows.Add(
                        (i + 1).ToString(CultureInfo.InvariantCulture),
                        row.Bounds.Top.ToString("0.0", CultureInfo.InvariantCulture) + " .. " + row.Bounds.Bottom.ToString("0.0", CultureInfo.InvariantCulture),
                        row.MedianHeight.ToString("0.0", CultureInfo.InvariantCulture),
                        row.Items.Count.ToString(CultureInfo.InvariantCulture),
                        row.GetText(),
                        clusters.Count.ToString(CultureInfo.InvariantCulture),
                        clusterText.ToString());
                }
                int clusterCount = 0;
                for (int i = 0; i < rows.Count; i++)
                    clusterCount += clusterDetector.Detect(rows[i]).Count;

                List<AlignmentAnchor> anchors = new AlignmentAnchorDetector(
                    new AlignmentAnchorOptions()).Detect(rows);

                AlignmentPatternDetector patternDetector = new AlignmentPatternDetector(
                    new AlignmentPatternOptions());
                List<AlignmentPattern> patterns = patternDetector.BuildPatterns(rows);
                List<AlignmentPatternTransition> transitions = patternDetector.DetectTransitions(rows);

                List<SpatialRegion> regions = new SpatialRegionDetector(
                    new SpatialRegionOptions()).Detect(rows, clusterDetector);

                // Anchorها قبل از Region نمایش داده می‌شوند تا بتوانیم ابتدا شواهد Alignment
                // را با تصویر مقایسه کنیم و Region را بر پایه حدس یا متن پزشکی تغییر ندهیم.
                StringBuilder anchorOutput = new StringBuilder();
                anchorOutput.AppendLine("===== ALIGNMENT ANCHORS =====");
                for (int i = 0; i < anchors.Count; i++)
                {
                    AlignmentAnchor anchor = anchors[i];
                    anchorOutput.Append("A");
                    anchorOutput.Append((i + 1).ToString(CultureInfo.InvariantCulture));
                    anchorOutput.Append("  X=");
                    anchorOutput.Append(anchor.X.ToString("0.0", CultureInfo.InvariantCulture));
                    anchorOutput.Append("  Rows=");
                    anchorOutput.Append((anchor.StartRow + 1).ToString(CultureInfo.InvariantCulture));
                    anchorOutput.Append("..");
                    anchorOutput.Append((anchor.EndRow + 1).ToString(CultureInfo.InvariantCulture));
                    anchorOutput.Append("  Support=");
                    anchorOutput.Append(anchor.RowCount.ToString(CultureInfo.InvariantCulture));
                    anchorOutput.Append("  : ");

                    for (int j = 0; j < anchor.Items.Count; j++)
                    {
                        if (j > 0) anchorOutput.Append(" / ");
                        anchorOutput.Append(anchor.Items[j].Text);
                    }
                    anchorOutput.AppendLine();
                }
                anchorOutput.AppendLine();

                // Pattern خام هر Row و تغییر آن نسبت به Row قبلی را جداگانه چاپ می‌کنیم
                // تا قبل از استفاده در RegionDetector بتوانیم Threshold و رفتار Topology را با تصویر کنترل کنیم.
                anchorOutput.AppendLine("===== ALIGNMENT PATTERNS =====");
                for (int i = 0; i < patterns.Count; i++)
                {
                    anchorOutput.Append("Row ");
                    anchorOutput.Append((patterns[i].RowIndex + 1).ToString(CultureInfo.InvariantCulture));
                    anchorOutput.Append("  X=");
                    for (int j = 0; j < patterns[i].Positions.Count; j++)
                    {
                        if (j > 0) anchorOutput.Append(", ");
                        anchorOutput.Append(patterns[i].Positions[j].ToString("0.0", CultureInfo.InvariantCulture));
                    }
                    anchorOutput.AppendLine();
                }

                anchorOutput.AppendLine();
                anchorOutput.AppendLine("===== TOPOLOGY TRANSITIONS =====");
                for (int i = 0; i < transitions.Count; i++)
                {
                    AlignmentPatternTransition transition = transitions[i];
                    anchorOutput.Append("Row ");
                    anchorOutput.Append((transition.PreviousRowIndex + 1).ToString(CultureInfo.InvariantCulture));
                    anchorOutput.Append(" -> ");
                    anchorOutput.Append((transition.CurrentRowIndex + 1).ToString(CultureInfo.InvariantCulture));
                    anchorOutput.Append("  Similarity=");
                    anchorOutput.Append(transition.Similarity.ToString("0.00", CultureInfo.InvariantCulture));
                    anchorOutput.Append("  Change=");
                    anchorOutput.Append(transition.ChangeScore.ToString("0.00", CultureInfo.InvariantCulture));
                    if (transition.IsBoundary) anchorOutput.Append("  *** BOUNDARY ***");
                    anchorOutput.AppendLine();
                }
                anchorOutput.AppendLine();

                // خروجی Region مستقیماً در TextBox چندخطی نمایش داده می‌شود؛
                // چون هدف این بخش بررسی سریع ساختار نهایی است و Grid اطلاعات اضافی ایجاد می‌کرد.
                StringBuilder regionOutput = new StringBuilder();
                for (int i = 0; i < regions.Count; i++)
                {
                    SpatialRegion region = regions[i];

                    regionOutput.Append("===== Region ");
                    regionOutput.Append((i + 1).ToString(CultureInfo.InvariantCulture));
                    regionOutput.Append("  X=");
                    regionOutput.Append(region.Bounds.Left.ToString("0.0", CultureInfo.InvariantCulture));
                    regionOutput.Append("..");
                    regionOutput.Append(region.Bounds.Right.ToString("0.0", CultureInfo.InvariantCulture));
                    regionOutput.Append("  Y=");
                    regionOutput.Append(region.Bounds.Top.ToString("0.0", CultureInfo.InvariantCulture));
                    regionOutput.Append("..");
                    regionOutput.Append(region.Bounds.Bottom.ToString("0.0", CultureInfo.InvariantCulture));
                    regionOutput.AppendLine(" =====");

                    for (int j = 0; j < region.Clusters.Count; j++)
                        regionOutput.AppendLine(region.Clusters[j].GetText());

                    if (i < regions.Count - 1)
                        regionOutput.AppendLine();
                }
                txtRegionText.Text = anchorOutput.ToString() + regionOutput.ToString();

                lblStatus.Text = items.Count + " OCR items  |  " + rows.Count +
                    " rows  |  " + clusterCount + " clusters  |  " +
                    anchors.Count + " anchors  |  " + transitions.Count + " transitions  |  " +
                    regions.Count + " regions";
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
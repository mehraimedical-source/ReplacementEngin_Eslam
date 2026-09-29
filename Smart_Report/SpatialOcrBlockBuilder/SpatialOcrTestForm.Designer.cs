namespace Smart_Report.SpatialOcrBlockBuilder
{
    partial class SpatialOcrTestForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlCommands;
        private System.Windows.Forms.Button btnAnalyzeRows;
        private System.Windows.Forms.Button btnRunRegressionTests;
        private System.Windows.Forms.Button btnCopyTestData;
        private System.Windows.Forms.Button btnSaveTestData;
        private System.Windows.Forms.Label lblSampleName;
        private System.Windows.Forms.TextBox txtSampleName;
        private System.Windows.Forms.Button btnSaveSample;
        private System.Windows.Forms.Button btnLoadSample;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnLegacyForm;
        private System.Windows.Forms.Panel pnlSettings;
        private System.Windows.Forms.Label lblOverlap;
        private System.Windows.Forms.NumericUpDown nudOverlap;
        private System.Windows.Forms.Label lblCenterFactor;
        private System.Windows.Forms.NumericUpDown nudCenter;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TextBox txtJson;
        private System.Windows.Forms.DataGridView gridRows;

        /// <summary>
        /// منابع استفاده شده توسط فرم را آزاد می‌کند.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// تمام کنترل‌ها و چیدمان فرم در این متد توسط Windows Forms Designer مدیریت می‌شوند.
        /// برای تغییر ظاهری فرم می‌توان مستقیماً از Visual Studio Designer استفاده کرد.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlCommands = new System.Windows.Forms.Panel();
            this.btnAnalyzeRows = new System.Windows.Forms.Button();
            this.btnRunRegressionTests = new System.Windows.Forms.Button();
            this.btnCopyTestData = new System.Windows.Forms.Button();
            this.btnSaveTestData = new System.Windows.Forms.Button();
            this.lblSampleName = new System.Windows.Forms.Label();
            this.txtSampleName = new System.Windows.Forms.TextBox();
            this.btnSaveSample = new System.Windows.Forms.Button();
            this.btnLoadSample = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnLegacyForm = new System.Windows.Forms.Button();
            this.pnlSettings = new System.Windows.Forms.Panel();
            this.lblOverlap = new System.Windows.Forms.Label();
            this.nudOverlap = new System.Windows.Forms.NumericUpDown();
            this.lblCenterFactor = new System.Windows.Forms.Label();
            this.nudCenter = new System.Windows.Forms.NumericUpDown();
            this.lblStatus = new System.Windows.Forms.Label();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.txtJson = new System.Windows.Forms.TextBox();
            this.gridRows = new System.Windows.Forms.DataGridView();
            this.pnlCommands.SuspendLayout();
            this.pnlSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudOverlap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCenter)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridRows)).BeginInit();
            this.SuspendLayout();

            // پنل فرمان‌ها جای ToolStrip را گرفته تا همه کنترل‌ها Button/TextBox استاندارد
            // و مستقیماً در Visual Studio Designer قابل ویرایش باشند.
            this.pnlCommands.Controls.Add(this.btnAnalyzeRows);
            this.pnlCommands.Controls.Add(this.btnRunRegressionTests);
            this.pnlCommands.Controls.Add(this.btnCopyTestData);
            this.pnlCommands.Controls.Add(this.btnSaveTestData);
            this.pnlCommands.Controls.Add(this.lblSampleName);
            this.pnlCommands.Controls.Add(this.txtSampleName);
            this.pnlCommands.Controls.Add(this.btnSaveSample);
            this.pnlCommands.Controls.Add(this.btnLoadSample);
            this.pnlCommands.Controls.Add(this.btnClear);
            this.pnlCommands.Controls.Add(this.btnLegacyForm);
            this.pnlCommands.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCommands.Location = new System.Drawing.Point(0, 0);
            this.pnlCommands.Name = "pnlCommands";
            this.pnlCommands.Size = new System.Drawing.Size(1234, 42);

            this.btnAnalyzeRows.Location = new System.Drawing.Point(8, 8);
            this.btnAnalyzeRows.Name = "btnAnalyzeRows";
            this.btnAnalyzeRows.Size = new System.Drawing.Size(95, 26);
            this.btnAnalyzeRows.Text = "Analyze Rows";
            this.btnAnalyzeRows.Click += new System.EventHandler(this.btnAnalyzeRows_Click);

            this.btnRunRegressionTests.Location = new System.Drawing.Point(109, 8);
            this.btnRunRegressionTests.Name = "btnRunRegressionTests";
            this.btnRunRegressionTests.Size = new System.Drawing.Size(135, 26);
            this.btnRunRegressionTests.Text = "Run Regression Tests";
            this.btnRunRegressionTests.Click += new System.EventHandler(this.btnRunRegressionTests_Click);

            this.btnCopyTestData.Location = new System.Drawing.Point(250, 8);
            this.btnCopyTestData.Name = "btnCopyTestData";
            this.btnCopyTestData.Size = new System.Drawing.Size(105, 26);
            this.btnCopyTestData.Text = "Copy Test Data";
            this.btnCopyTestData.Click += new System.EventHandler(this.btnCopyTestData_Click);

            this.btnSaveTestData.Location = new System.Drawing.Point(361, 8);
            this.btnSaveTestData.Name = "btnSaveTestData";
            this.btnSaveTestData.Size = new System.Drawing.Size(105, 26);
            this.btnSaveTestData.Text = "Save Test Data";
            this.btnSaveTestData.Click += new System.EventHandler(this.btnSaveTestData_Click);

            this.lblSampleName.AutoSize = true;
            this.lblSampleName.Location = new System.Drawing.Point(480, 14);
            this.lblSampleName.Name = "lblSampleName";
            this.lblSampleName.Text = "Sample:";

            this.txtSampleName.Location = new System.Drawing.Point(530, 11);
            this.txtSampleName.Name = "txtSampleName";
            this.txtSampleName.Size = new System.Drawing.Size(110, 20);

            this.btnSaveSample.Location = new System.Drawing.Point(646, 8);
            this.btnSaveSample.Name = "btnSaveSample";
            this.btnSaveSample.Size = new System.Drawing.Size(90, 26);
            this.btnSaveSample.Text = "Save Sample";
            this.btnSaveSample.Click += new System.EventHandler(this.btnSaveSample_Click);

            this.btnLoadSample.Location = new System.Drawing.Point(742, 8);
            this.btnLoadSample.Name = "btnLoadSample";
            this.btnLoadSample.Size = new System.Drawing.Size(90, 26);
            this.btnLoadSample.Text = "Load Sample";
            this.btnLoadSample.Click += new System.EventHandler(this.btnLoadSample_Click);

            this.btnClear.Location = new System.Drawing.Point(838, 8);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(70, 26);
            this.btnClear.Text = "Clear";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);

            this.btnLegacyForm.Location = new System.Drawing.Point(914, 8);
            this.btnLegacyForm.Name = "btnLegacyForm";
            this.btnLegacyForm.Size = new System.Drawing.Size(90, 26);
            this.btnLegacyForm.Text = "Legacy Form";
            this.btnLegacyForm.Click += new System.EventHandler(this.btnLegacyForm_Click);

            this.pnlSettings.Controls.Add(this.lblOverlap);
            this.pnlSettings.Controls.Add(this.nudOverlap);
            this.pnlSettings.Controls.Add(this.lblCenterFactor);
            this.pnlSettings.Controls.Add(this.nudCenter);
            this.pnlSettings.Controls.Add(this.lblStatus);
            this.pnlSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSettings.Location = new System.Drawing.Point(0, 42);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.Size = new System.Drawing.Size(1234, 34);

            this.lblOverlap.AutoSize = true;
            this.lblOverlap.Location = new System.Drawing.Point(8, 9);
            this.lblOverlap.Text = "Min overlap";

            this.nudOverlap.DecimalPlaces = 2;
            this.nudOverlap.Increment = new decimal(new int[] {5, 0, 0, 131072});
            this.nudOverlap.Location = new System.Drawing.Point(90, 5);
            this.nudOverlap.Maximum = new decimal(new int[] {2, 0, 0, 0});
            this.nudOverlap.Size = new System.Drawing.Size(75, 20);
            this.nudOverlap.Value = new decimal(new int[] {45, 0, 0, 131072});

            this.lblCenterFactor.AutoSize = true;
            this.lblCenterFactor.Location = new System.Drawing.Point(180, 9);
            this.lblCenterFactor.Text = "Max center factor";

            this.nudCenter.DecimalPlaces = 2;
            this.nudCenter.Increment = new decimal(new int[] {5, 0, 0, 131072});
            this.nudCenter.Location = new System.Drawing.Point(290, 5);
            this.nudCenter.Maximum = new decimal(new int[] {2, 0, 0, 0});
            this.nudCenter.Size = new System.Drawing.Size(75, 20);
            this.nudCenter.Value = new decimal(new int[] {55, 0, 0, 131072});

            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(405, 9);
            this.lblStatus.Name = "lblStatus";

            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 76);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitMain.SplitterDistance = 300;

            this.txtJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtJson.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtJson.Multiline = true;
            this.txtJson.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtJson.WordWrap = false;
            this.splitMain.Panel1.Controls.Add(this.txtJson);

            this.gridRows.AllowUserToAddRows = false;
            this.gridRows.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridRows.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridRows.ReadOnly = true;
            this.gridRows.Columns.Add("Row", "#");
            this.gridRows.Columns.Add("Y", "Y Range");
            this.gridRows.Columns.Add("Height", "Median H");
            this.gridRows.Columns.Add("Items", "Items");
            this.gridRows.Columns.Add("Text", "Reconstructed Row");
            this.gridRows.Columns.Add("Clusters", "Clusters");
            this.gridRows.Columns.Add("ClusterText", "Horizontal Clusters");
            this.gridRows.Columns[0].FillWeight = 10F;
            this.gridRows.Columns[1].FillWeight = 18F;
            this.gridRows.Columns[2].FillWeight = 15F;
            this.gridRows.Columns[3].FillWeight = 12F;
            this.gridRows.Columns[4].FillWeight = 80F;
            this.gridRows.Columns[5].FillWeight = 12F;
            this.gridRows.Columns[6].FillWeight = 100F;
            this.splitMain.Panel2.Controls.Add(this.gridRows);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1234, 741);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlSettings);
            this.Controls.Add(this.pnlCommands);
            this.MinimumSize = new System.Drawing.Size(1050, 600);
            this.Name = "SpatialOcrTestForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Spatial OCR Block Builder - Test";
            this.pnlCommands.ResumeLayout(false);
            this.pnlCommands.PerformLayout();
            this.pnlSettings.ResumeLayout(false);
            this.pnlSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudOverlap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCenter)).EndInit();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel1.PerformLayout();
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridRows)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        /// <summary>
        /// رویداد دکمه تحلیل Rowها؛ منطق اصلی در فایل کد فرم نگهداری می‌شود.
        /// </summary>
        private void btnAnalyzeRows_Click(object sender, System.EventArgs e) { Analyze(); }

        /// <summary>
        /// رویداد اجرای تست‌های رگرسیون Spatial.
        /// </summary>
        private void btnRunRegressionTests_Click(object sender, System.EventArgs e) { RunRegressionTests(); }

        /// <summary>
        /// رویداد کپی اطلاعات Sample برای ارسال و بررسی.
        /// </summary>
        private void btnCopyTestData_Click(object sender, System.EventArgs e) { CopyTestData(); }

        /// <summary>
        /// رویداد ذخیره Sample کامل در بانک دائمی پروژه.
        /// </summary>
        private void btnSaveSample_Click(object sender, System.EventArgs e) { SaveSample(); }

        /// <summary>
        /// رویداد بارگذاری مجدد Sample ذخیره‌شده برای اجرای همان Pipeline روی داده واقعی.
        /// </summary>
        private void btnLoadSample_Click(object sender, System.EventArgs e) { LoadSample(); }

        /// <summary>
        /// رویداد ذخیره اطلاعات Sample در فایل متنی.
        /// </summary>
        private void btnSaveTestData_Click(object sender, System.EventArgs e) { SaveTestData(); }

        /// <summary>
        /// فرم تست را برای Sample بعدی پاک می‌کند.
        /// </summary>
        private void btnClear_Click(object sender, System.EventArgs e)
        {
            txtJson.Clear();
            gridRows.Rows.Clear();
            lblStatus.Text = "";
        }

        /// <summary>
        /// فرم قدیمی OCR Block Builder را برای مقایسه باز می‌کند.
        /// </summary>
        private void btnLegacyForm_Click(object sender, System.EventArgs e)
        {
            new Smart_Report.Form1().Show();
        }
    }
}

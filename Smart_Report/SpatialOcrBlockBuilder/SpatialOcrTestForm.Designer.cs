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
        private System.Windows.Forms.SplitContainer splitSource;
        private System.Windows.Forms.PictureBox picSource;
        private System.Windows.Forms.DataGridView gridRows;
        private System.Windows.Forms.SplitContainer splitResults;
        private System.Windows.Forms.TextBox txtRegionText;

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
            this.splitSource = new System.Windows.Forms.SplitContainer();
            this.picSource = new System.Windows.Forms.PictureBox();
            this.txtJson = new System.Windows.Forms.TextBox();
            this.splitResults = new System.Windows.Forms.SplitContainer();
            this.gridRows = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtRegionText = new System.Windows.Forms.TextBox();
            this.pnlCommands.SuspendLayout();
            this.pnlSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudOverlap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCenter)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.splitSource.Panel1.SuspendLayout();
            this.splitSource.Panel2.SuspendLayout();
            this.splitSource.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSource)).BeginInit();
            this.splitResults.Panel1.SuspendLayout();
            this.splitResults.Panel2.SuspendLayout();
            this.splitResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridRows)).BeginInit();

            this.SuspendLayout();
            // 
            // pnlCommands
            // 
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
            this.pnlCommands.TabIndex = 2;
            // 
            // btnAnalyzeRows
            // 
            this.btnAnalyzeRows.Location = new System.Drawing.Point(8, 8);
            this.btnAnalyzeRows.Name = "btnAnalyzeRows";
            this.btnAnalyzeRows.Size = new System.Drawing.Size(95, 26);
            this.btnAnalyzeRows.TabIndex = 0;
            this.btnAnalyzeRows.Text = "Analyze Rows";
            this.btnAnalyzeRows.Click += new System.EventHandler(this.btnAnalyzeRows_Click);
            // 
            // btnRunRegressionTests
            // 
            this.btnRunRegressionTests.Location = new System.Drawing.Point(109, 8);
            this.btnRunRegressionTests.Name = "btnRunRegressionTests";
            this.btnRunRegressionTests.Size = new System.Drawing.Size(135, 26);
            this.btnRunRegressionTests.TabIndex = 1;
            this.btnRunRegressionTests.Text = "Run Regression Tests";
            this.btnRunRegressionTests.Click += new System.EventHandler(this.btnRunRegressionTests_Click);
            // 
            // btnCopyTestData
            // 
            this.btnCopyTestData.Location = new System.Drawing.Point(250, 8);
            this.btnCopyTestData.Name = "btnCopyTestData";
            this.btnCopyTestData.Size = new System.Drawing.Size(105, 26);
            this.btnCopyTestData.TabIndex = 2;
            this.btnCopyTestData.Text = "Copy Test Data";
            this.btnCopyTestData.Click += new System.EventHandler(this.btnCopyTestData_Click);
            // 
            // btnSaveTestData
            // 
            this.btnSaveTestData.Location = new System.Drawing.Point(361, 8);
            this.btnSaveTestData.Name = "btnSaveTestData";
            this.btnSaveTestData.Size = new System.Drawing.Size(105, 26);
            this.btnSaveTestData.TabIndex = 3;
            this.btnSaveTestData.Text = "Save Test Data";
            this.btnSaveTestData.Click += new System.EventHandler(this.btnSaveTestData_Click);
            // 
            // lblSampleName
            // 
            this.lblSampleName.AutoSize = true;
            this.lblSampleName.Location = new System.Drawing.Point(480, 14);
            this.lblSampleName.Name = "lblSampleName";
            this.lblSampleName.Size = new System.Drawing.Size(45, 13);
            this.lblSampleName.TabIndex = 4;
            this.lblSampleName.Text = "Sample:";
            // 
            // txtSampleName
            // 
            this.txtSampleName.Location = new System.Drawing.Point(530, 11);
            this.txtSampleName.Name = "txtSampleName";
            this.txtSampleName.Size = new System.Drawing.Size(110, 20);
            this.txtSampleName.TabIndex = 5;
            // 
            // btnSaveSample
            // 
            this.btnSaveSample.Location = new System.Drawing.Point(646, 8);
            this.btnSaveSample.Name = "btnSaveSample";
            this.btnSaveSample.Size = new System.Drawing.Size(90, 26);
            this.btnSaveSample.TabIndex = 6;
            this.btnSaveSample.Text = "Save Sample";
            this.btnSaveSample.Click += new System.EventHandler(this.btnSaveSample_Click);
            // 
            // btnLoadSample
            // 
            this.btnLoadSample.Location = new System.Drawing.Point(742, 8);
            this.btnLoadSample.Name = "btnLoadSample";
            this.btnLoadSample.Size = new System.Drawing.Size(90, 26);
            this.btnLoadSample.TabIndex = 7;
            this.btnLoadSample.Text = "Load Sample";
            this.btnLoadSample.Click += new System.EventHandler(this.btnLoadSample_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(838, 8);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(70, 26);
            this.btnClear.TabIndex = 8;
            this.btnClear.Text = "Clear";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnLegacyForm
            // 
            this.btnLegacyForm.Location = new System.Drawing.Point(914, 8);
            this.btnLegacyForm.Name = "btnLegacyForm";
            this.btnLegacyForm.Size = new System.Drawing.Size(90, 26);
            this.btnLegacyForm.TabIndex = 9;
            this.btnLegacyForm.Text = "Legacy Form";
            this.btnLegacyForm.Click += new System.EventHandler(this.btnLegacyForm_Click);
            // 
            // pnlSettings
            // 
            this.pnlSettings.Controls.Add(this.lblOverlap);
            this.pnlSettings.Controls.Add(this.nudOverlap);
            this.pnlSettings.Controls.Add(this.lblCenterFactor);
            this.pnlSettings.Controls.Add(this.nudCenter);
            this.pnlSettings.Controls.Add(this.lblStatus);
            this.pnlSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSettings.Location = new System.Drawing.Point(0, 42);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.Size = new System.Drawing.Size(1234, 34);
            this.pnlSettings.TabIndex = 1;
            // 
            // lblOverlap
            // 
            this.lblOverlap.AutoSize = true;
            this.lblOverlap.Location = new System.Drawing.Point(8, 9);
            this.lblOverlap.Name = "lblOverlap";
            this.lblOverlap.Size = new System.Drawing.Size(62, 13);
            this.lblOverlap.TabIndex = 0;
            this.lblOverlap.Text = "Min overlap";
            // 
            // nudOverlap
            // 
            this.nudOverlap.DecimalPlaces = 2;
            this.nudOverlap.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.nudOverlap.Location = new System.Drawing.Point(90, 5);
            this.nudOverlap.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudOverlap.Name = "nudOverlap";
            this.nudOverlap.Size = new System.Drawing.Size(75, 20);
            this.nudOverlap.TabIndex = 1;
            this.nudOverlap.Value = new decimal(new int[] {
            45,
            0,
            0,
            131072});
            // 
            // lblCenterFactor
            // 
            this.lblCenterFactor.AutoSize = true;
            this.lblCenterFactor.Location = new System.Drawing.Point(180, 9);
            this.lblCenterFactor.Name = "lblCenterFactor";
            this.lblCenterFactor.Size = new System.Drawing.Size(90, 13);
            this.lblCenterFactor.TabIndex = 2;
            this.lblCenterFactor.Text = "Max center factor";
            // 
            // nudCenter
            // 
            this.nudCenter.DecimalPlaces = 2;
            this.nudCenter.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.nudCenter.Location = new System.Drawing.Point(290, 5);
            this.nudCenter.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudCenter.Name = "nudCenter";
            this.nudCenter.Size = new System.Drawing.Size(75, 20);
            this.nudCenter.TabIndex = 3;
            this.nudCenter.Value = new decimal(new int[] {
            55,
            0,
            0,
            131072});
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(405, 9);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 4;
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 76);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.splitSource);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.splitResults);
            this.splitMain.Size = new System.Drawing.Size(1234, 665);
            this.splitMain.SplitterDistance = 310;
            this.splitMain.TabIndex = 0;
            // 
            // splitSource
            // 
            this.splitSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitSource.Location = new System.Drawing.Point(0, 0);
            this.splitSource.Name = "splitSource";
            // 
            // splitSource.Panel1
            // 
            this.splitSource.Panel1.Controls.Add(this.picSource);
            // 
            // splitSource.Panel2
            // 
            this.splitSource.Panel2.Controls.Add(this.txtJson);
            this.splitSource.Size = new System.Drawing.Size(1234, 310);
            this.splitSource.SplitterDistance = 710;
            this.splitSource.TabIndex = 0;
            // 
            // picSource
            // 
            this.picSource.BackColor = System.Drawing.Color.Black;
            this.picSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picSource.Location = new System.Drawing.Point(0, 0);
            this.picSource.Name = "picSource";
            this.picSource.Size = new System.Drawing.Size(710, 310);
            this.picSource.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picSource.TabIndex = 0;
            this.picSource.TabStop = false;
            // 
            // txtJson
            // 
            this.txtJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtJson.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtJson.Location = new System.Drawing.Point(0, 0);
            this.txtJson.Multiline = true;
            this.txtJson.Name = "txtJson";
            this.txtJson.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtJson.Size = new System.Drawing.Size(520, 310);
            this.txtJson.TabIndex = 0;
            this.txtJson.WordWrap = false;
            // 
            // splitResults
            // 
            this.splitResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitResults.Location = new System.Drawing.Point(0, 0);
            this.splitResults.Name = "splitResults";
            this.splitResults.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitResults.Panel1
            // 
            this.splitResults.Panel1.Controls.Add(this.gridRows);
            // 
            // splitResults.Panel2
            // 
            this.splitResults.Panel2.Controls.Add(this.txtRegionText);
            this.splitResults.Size = new System.Drawing.Size(1234, 351);
            this.splitResults.SplitterDistance = 178;
            this.splitResults.TabIndex = 0;
            // 
            // gridRows
            // 
            this.gridRows.AllowUserToAddRows = false;
            this.gridRows.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridRows.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7});
            this.gridRows.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridRows.Location = new System.Drawing.Point(0, 0);
            this.gridRows.Name = "gridRows";
            this.gridRows.ReadOnly = true;
            this.gridRows.Size = new System.Drawing.Size(1234, 178);
            this.gridRows.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.HeaderText = "#";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Y Range";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.HeaderText = "Median H";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "Items";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "Reconstructed Row";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.HeaderText = "Clusters";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.HeaderText = "Horizontal Clusters";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.ReadOnly = true;
            // 
            // txtRegionText
            // 
            this.txtRegionText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRegionText.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtRegionText.Multiline = true;
            this.txtRegionText.Name = "txtRegionText";
            this.txtRegionText.ReadOnly = true;
            this.txtRegionText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRegionText.WordWrap = false;
            this.txtRegionText.Size = new System.Drawing.Size(1234, 169);
            // 
            // 
            // SpatialOcrTestForm
            // 
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
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.ResumeLayout(false);
            this.splitSource.Panel1.ResumeLayout(false);
            this.splitSource.Panel2.ResumeLayout(false);
            this.splitSource.Panel2.PerformLayout();
            this.splitSource.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picSource)).EndInit();
            this.splitResults.Panel1.ResumeLayout(false);
            this.splitResults.Panel2.ResumeLayout(false);
            this.splitResults.ResumeLayout(false);
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
            txtRegionText.Clear();
            lblStatus.Text = "";
        }

        /// <summary>
        /// فرم قدیمی OCR Block Builder را برای مقایسه باز می‌کند.
        /// </summary>
        private void btnLegacyForm_Click(object sender, System.EventArgs e)
        {
            new Smart_Report.Form1().Show();
        }

        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
    }
}

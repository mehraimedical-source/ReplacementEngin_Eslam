namespace DicomViewer_ChatGPT
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.Panel toolbarPanel;
        private System.Windows.Forms.Button btnResetAxes;
        private System.Windows.Forms.Button btnReset3D;
        private System.Windows.Forms.Label lblAxisWidth;
        private System.Windows.Forms.NumericUpDown numAxisWidth;
        private System.Windows.Forms.Button btnWindowLevel;
        private System.Windows.Forms.Button btnDefaultWindowLevel;
        private System.Windows.Forms.Label lblWindowLevel;
        private MedicalDicomViewerControl dicomViewer;

        protected override void Dispose(bool disposing) { if (disposing && (components != null)) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.topPanel = new System.Windows.Forms.Panel();
            this.lblState = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.toolbarPanel = new System.Windows.Forms.Panel();
            this.lblWindowLevel = new System.Windows.Forms.Label();
            this.btnDefaultWindowLevel = new System.Windows.Forms.Button();
            this.btnWindowLevel = new System.Windows.Forms.Button();
            this.numAxisWidth = new System.Windows.Forms.NumericUpDown();
            this.lblAxisWidth = new System.Windows.Forms.Label();
            this.btnReset3D = new System.Windows.Forms.Button();
            this.btnResetAxes = new System.Windows.Forms.Button();
            this.dicomViewer = new DicomViewer_ChatGPT.MedicalDicomViewerControl();
            this.topPanel.SuspendLayout();
            this.toolbarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAxisWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // topPanel
            // 
            this.topPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.topPanel.Controls.Add(this.lblState);
            this.topPanel.Controls.Add(this.btnLoad);
            this.topPanel.Controls.Add(this.btnBrowse);
            this.topPanel.Controls.Add(this.txtFolder);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Padding = new System.Windows.Forms.Padding(6);
            this.topPanel.Size = new System.Drawing.Size(1280, 42);
            this.topPanel.TabIndex = 1;
            // 
            // lblState
            // 
            this.lblState.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblState.ForeColor = System.Drawing.Color.White;
            this.lblState.Location = new System.Drawing.Point(994, 6);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(90, 30);
            this.lblState.TabIndex = 0;
            this.lblState.Text = "Ready";
            this.lblState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLoad
            // 
            this.btnLoad.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnLoad.Location = new System.Drawing.Point(1084, 6);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(100, 30);
            this.btnLoad.TabIndex = 1;
            this.btnLoad.Text = "Load DICOM";
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnBrowse.Location = new System.Drawing.Point(1184, 6);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(90, 30);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFolder
            // 
            this.txtFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFolder.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtFolder.Location = new System.Drawing.Point(6, 6);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(1268, 25);
            this.txtFolder.TabIndex = 3;
            // 
            // toolbarPanel
            // 
            this.toolbarPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(38)))));
            this.toolbarPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolbarPanel.Controls.Add(this.lblWindowLevel);
            this.toolbarPanel.Controls.Add(this.btnDefaultWindowLevel);
            this.toolbarPanel.Controls.Add(this.btnWindowLevel);
            this.toolbarPanel.Controls.Add(this.numAxisWidth);
            this.toolbarPanel.Controls.Add(this.lblAxisWidth);
            this.toolbarPanel.Controls.Add(this.btnReset3D);
            this.toolbarPanel.Controls.Add(this.btnResetAxes);
            this.toolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarPanel.Location = new System.Drawing.Point(0, 42);
            this.toolbarPanel.Name = "toolbarPanel";
            this.toolbarPanel.Size = new System.Drawing.Size(1280, 48);
            this.toolbarPanel.TabIndex = 2;
            // 
            // lblWindowLevel
            // 
            this.lblWindowLevel.AutoSize = true;
            this.lblWindowLevel.ForeColor = System.Drawing.Color.Gainsboro;
            this.lblWindowLevel.Location = new System.Drawing.Point(642, 16);
            this.lblWindowLevel.Name = "lblWindowLevel";
            this.lblWindowLevel.Size = new System.Drawing.Size(73, 13);
            this.lblWindowLevel.TabIndex = 5;
            this.lblWindowLevel.Text = "WL: -   WW: -";
            // 
            // btnDefaultWindowLevel
            // 
            this.btnDefaultWindowLevel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.btnDefaultWindowLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDefaultWindowLevel.ForeColor = System.Drawing.Color.White;
            this.btnDefaultWindowLevel.Location = new System.Drawing.Point(540, 8);
            this.btnDefaultWindowLevel.Name = "btnDefaultWindowLevel";
            this.btnDefaultWindowLevel.Size = new System.Drawing.Size(92, 30);
            this.btnDefaultWindowLevel.TabIndex = 5;
            this.btnDefaultWindowLevel.Text = "Default W/L";
            this.btnDefaultWindowLevel.UseVisualStyleBackColor = false;
            this.btnDefaultWindowLevel.Click += new System.EventHandler(this.btnDefaultWindowLevel_Click);
            // 
            // btnWindowLevel
            // 
            this.btnWindowLevel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.btnWindowLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWindowLevel.ForeColor = System.Drawing.Color.White;
            this.btnWindowLevel.Location = new System.Drawing.Point(425, 8);
            this.btnWindowLevel.Name = "btnWindowLevel";
            this.btnWindowLevel.Size = new System.Drawing.Size(105, 30);
            this.btnWindowLevel.TabIndex = 4;
            this.btnWindowLevel.Text = "Window / Level";
            this.btnWindowLevel.UseVisualStyleBackColor = false;
            this.btnWindowLevel.Click += new System.EventHandler(this.btnWindowLevel_Click);
            // 
            // numAxisWidth
            // 
            this.numAxisWidth.DecimalPlaces = 1;
            this.numAxisWidth.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numAxisWidth.Location = new System.Drawing.Point(345, 13);
            this.numAxisWidth.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numAxisWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numAxisWidth.Name = "numAxisWidth";
            this.numAxisWidth.Size = new System.Drawing.Size(58, 20);
            this.numAxisWidth.TabIndex = 3;
            this.numAxisWidth.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.numAxisWidth.ValueChanged += new System.EventHandler(this.numAxisWidth_ValueChanged);
            // 
            // lblAxisWidth
            // 
            this.lblAxisWidth.AutoSize = true;
            this.lblAxisWidth.ForeColor = System.Drawing.Color.White;
            this.lblAxisWidth.Location = new System.Drawing.Point(250, 16);
            this.lblAxisWidth.Name = "lblAxisWidth";
            this.lblAxisWidth.Size = new System.Drawing.Size(77, 13);
            this.lblAxisWidth.TabIndex = 2;
            this.lblAxisWidth.Text = "Axis thickness:";
            // 
            // btnReset3D
            // 
            this.btnReset3D.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.btnReset3D.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReset3D.ForeColor = System.Drawing.Color.White;
            this.btnReset3D.Location = new System.Drawing.Point(124, 8);
            this.btnReset3D.Name = "btnReset3D";
            this.btnReset3D.Size = new System.Drawing.Size(110, 30);
            this.btnReset3D.TabIndex = 1;
            this.btnReset3D.Text = "Reset 3D";
            this.btnReset3D.UseVisualStyleBackColor = false;
            this.btnReset3D.Click += new System.EventHandler(this.btnReset3D_Click);
            // 
            // btnResetAxes
            // 
            this.btnResetAxes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.btnResetAxes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResetAxes.ForeColor = System.Drawing.Color.White;
            this.btnResetAxes.Location = new System.Drawing.Point(8, 8);
            this.btnResetAxes.Name = "btnResetAxes";
            this.btnResetAxes.Size = new System.Drawing.Size(110, 30);
            this.btnResetAxes.TabIndex = 0;
            this.btnResetAxes.Text = "Reset Axes";
            this.btnResetAxes.UseVisualStyleBackColor = false;
            this.btnResetAxes.Click += new System.EventHandler(this.btnResetAxes_Click);
            // 
            // dicomViewer
            // 
            this.dicomViewer.AxisLineWidth = 2F;
            this.dicomViewer.BackColor = System.Drawing.Color.Black;
            this.dicomViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dicomViewer.Location = new System.Drawing.Point(0, 90);
            this.dicomViewer.Name = "dicomViewer";
            this.dicomViewer.Size = new System.Drawing.Size(1280, 730);
            this.dicomViewer.TabIndex = 0;
            this.dicomViewer.WindowLevelMode = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 820);
            this.Controls.Add(this.dicomViewer);
            this.Controls.Add(this.toolbarPanel);
            this.Controls.Add(this.topPanel);
            this.ForeColor = System.Drawing.Color.White;
            this.MinimumSize = new System.Drawing.Size(900, 650);
            this.Name = "Form1";
            this.Text = "DicomViewer_ChatGPT - MPR / 3D MVP";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.toolbarPanel.ResumeLayout(false);
            this.toolbarPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAxisWidth)).EndInit();
            this.ResumeLayout(false);

        }
    }
}

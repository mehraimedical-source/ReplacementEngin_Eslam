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
        private MedicalDicomViewerControl dicomViewer;

        protected override void Dispose(bool disposing) { if (disposing && (components != null)) components.Dispose(); base.Dispose(disposing); }

        private void InitializeComponent()
        {
            this.topPanel = new System.Windows.Forms.Panel();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.lblState = new System.Windows.Forms.Label();
            this.dicomViewer = new MedicalDicomViewerControl();
            this.topPanel.SuspendLayout();
            this.SuspendLayout();

            this.topPanel.Controls.Add(this.lblState);
            this.topPanel.Controls.Add(this.btnLoad);
            this.topPanel.Controls.Add(this.btnBrowse);
            this.topPanel.Controls.Add(this.txtFolder);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Height = 42;
            this.topPanel.Padding = new System.Windows.Forms.Padding(6);
            this.topPanel.BackColor = System.Drawing.Color.FromArgb(45,45,48);

            this.txtFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFolder.Font = new System.Drawing.Font("Segoe UI", 10F);

            this.btnBrowse.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnBrowse.Width = 90;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            this.btnLoad.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnLoad.Width = 100;
            this.btnLoad.Text = "Load DICOM";
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);

            this.lblState.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblState.Width = 90;
            this.lblState.Text = "Ready";
            this.lblState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblState.ForeColor = System.Drawing.Color.White;

            this.dicomViewer.Dock = System.Windows.Forms.DockStyle.Fill;

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 820);
            this.Controls.Add(this.dicomViewer);
            this.Controls.Add(this.topPanel);
            this.MinimumSize = new System.Drawing.Size(900, 650);
            this.Text = "DicomViewer_ChatGPT - MPR / 3D MVP";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}

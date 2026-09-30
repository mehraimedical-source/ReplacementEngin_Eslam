namespace Worlist_SCP
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel pnlConfig;
        private System.Windows.Forms.Label lblAeCaption;
        private System.Windows.Forms.TextBox txtAETitle;
        private System.Windows.Forms.Label lblPortCaption;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblServerStatus;
        private System.Windows.Forms.Panel pnlStats;
        private System.Windows.Forms.Label lblAssocCaption;
        private System.Windows.Forms.Label lblAssocCount;
        private System.Windows.Forms.Label lblRequestCaption;
        private System.Windows.Forms.Label lblRequestCount;
        private System.Windows.Forms.Label lblResponseCaption;
        private System.Windows.Forms.Label lblResponseCount;
        private System.Windows.Forms.DataGridView dgvLog;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Label lblHint;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle headerStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle rowStyle = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlConfig = new System.Windows.Forms.Panel();
            this.lblServerStatus = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.lblPortCaption = new System.Windows.Forms.Label();
            this.txtAETitle = new System.Windows.Forms.TextBox();
            this.lblAeCaption = new System.Windows.Forms.Label();
            this.pnlStats = new System.Windows.Forms.Panel();
            this.lblResponseCount = new System.Windows.Forms.Label();
            this.lblResponseCaption = new System.Windows.Forms.Label();
            this.lblRequestCount = new System.Windows.Forms.Label();
            this.lblRequestCaption = new System.Windows.Forms.Label();
            this.lblAssocCount = new System.Windows.Forms.Label();
            this.lblAssocCaption = new System.Windows.Forms.Label();
            this.dgvLog = new System.Windows.Forms.DataGridView();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.lblHint = new System.Windows.Forms.Label();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.pnlConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.pnlStats.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(31, 41, 55);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 78;

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(22, 12);
            this.lblTitle.Text = "DICOM Modality Worklist SCP";

            // lblSubtitle
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.lblSubtitle.Location = new System.Drawing.Point(25, 49);
            this.lblSubtitle.Text = "fo-dicom 5.2.6  •  .NET Framework 4.8  •  Accept any Called AE Title";

            // pnlConfig
            this.pnlConfig.BackColor = System.Drawing.Color.White;
            this.pnlConfig.Controls.Add(this.lblServerStatus);
            this.pnlConfig.Controls.Add(this.btnStop);
            this.pnlConfig.Controls.Add(this.btnStart);
            this.pnlConfig.Controls.Add(this.numPort);
            this.pnlConfig.Controls.Add(this.lblPortCaption);
            this.pnlConfig.Controls.Add(this.txtAETitle);
            this.pnlConfig.Controls.Add(this.lblAeCaption);
            this.pnlConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlConfig.Height = 92;
            this.pnlConfig.Padding = new System.Windows.Forms.Padding(20, 12, 20, 12);

            // lblAeCaption
            this.lblAeCaption.AutoSize = true;
            this.lblAeCaption.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAeCaption.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblAeCaption.Location = new System.Drawing.Point(22, 14);
            this.lblAeCaption.Text = "Local AE Title";

            // txtAETitle
            this.txtAETitle.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.txtAETitle.Location = new System.Drawing.Point(24, 35);
            this.txtAETitle.MaxLength = 16;
            this.txtAETitle.Size = new System.Drawing.Size(190, 25);
            this.txtAETitle.Text = "WORKLIST_SCP";

            // lblPortCaption
            this.lblPortCaption.AutoSize = true;
            this.lblPortCaption.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPortCaption.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblPortCaption.Location = new System.Drawing.Point(235, 14);
            this.lblPortCaption.Text = "Port";

            // numPort
            this.numPort.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.numPort.Location = new System.Drawing.Point(238, 35);
            this.numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numPort.Size = new System.Drawing.Size(110, 25);
            this.numPort.Value = new decimal(new int[] { 11112, 0, 0, 0 });

            // btnStart
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.FlatAppearance.BorderSize = 0;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.Location = new System.Drawing.Point(380, 27);
            this.btnStart.Size = new System.Drawing.Size(120, 40);
            this.btnStart.Text = "Start Server";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);

            // btnStop
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(239, 68, 68);
            this.btnStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStop.Enabled = false;
            this.btnStop.FlatAppearance.BorderSize = 0;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Location = new System.Drawing.Point(510, 27);
            this.btnStop.Size = new System.Drawing.Size(120, 40);
            this.btnStop.Text = "Stop Server";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);

            // lblServerStatus
            this.lblServerStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblServerStatus.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.lblServerStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.lblServerStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblServerStatus.Location = new System.Drawing.Point(805, 27);
            this.lblServerStatus.Size = new System.Drawing.Size(250, 40);
            this.lblServerStatus.Text = "●  STOPPED";
            this.lblServerStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // pnlStats
            this.pnlStats.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.pnlStats.Controls.Add(this.lblResponseCount);
            this.pnlStats.Controls.Add(this.lblResponseCaption);
            this.pnlStats.Controls.Add(this.lblRequestCount);
            this.pnlStats.Controls.Add(this.lblRequestCaption);
            this.pnlStats.Controls.Add(this.lblAssocCount);
            this.pnlStats.Controls.Add(this.lblAssocCaption);
            this.pnlStats.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlStats.Height = 64;

            this.lblAssocCaption.AutoSize = true;
            this.lblAssocCaption.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblAssocCaption.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblAssocCaption.Location = new System.Drawing.Point(24, 10);
            this.lblAssocCaption.Text = "Associations";
            this.lblAssocCount.AutoSize = true;
            this.lblAssocCount.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblAssocCount.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblAssocCount.Location = new System.Drawing.Point(23, 28);
            this.lblAssocCount.Text = "0";

            this.lblRequestCaption.AutoSize = true;
            this.lblRequestCaption.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblRequestCaption.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblRequestCaption.Location = new System.Drawing.Point(170, 10);
            this.lblRequestCaption.Text = "MWL Requests";
            this.lblRequestCount.AutoSize = true;
            this.lblRequestCount.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblRequestCount.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblRequestCount.Location = new System.Drawing.Point(169, 28);
            this.lblRequestCount.Text = "0";

            this.lblResponseCaption.AutoSize = true;
            this.lblResponseCaption.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblResponseCaption.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblResponseCaption.Location = new System.Drawing.Point(326, 10);
            this.lblResponseCaption.Text = "Items Sent";
            this.lblResponseCount.AutoSize = true;
            this.lblResponseCount.Font = new System.Drawing.Font("Segoe UI Semibold", 15F, System.Drawing.FontStyle.Bold);
            this.lblResponseCount.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblResponseCount.Location = new System.Drawing.Point(325, 28);
            this.lblResponseCount.Text = "0";

            // dgvLog
            this.dgvLog.AllowUserToAddRows = false;
            this.dgvLog.AllowUserToDeleteRows = false;
            this.dgvLog.AllowUserToResizeRows = false;
            this.dgvLog.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
            this.dgvLog.BackgroundColor = System.Drawing.Color.White;
            this.dgvLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvLog.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvLog.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            headerStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            headerStyle.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            headerStyle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            headerStyle.ForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            headerStyle.SelectionBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            headerStyle.SelectionForeColor = System.Drawing.Color.FromArgb(51, 65, 85);
            headerStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvLog.ColumnHeadersDefaultCellStyle = headerStyle;
            this.dgvLog.ColumnHeadersHeight = 36;
            this.dgvLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvLog.EnableHeadersVisualStyles = false;
            this.dgvLog.GridColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.dgvLog.ReadOnly = true;
            rowStyle.BackColor = System.Drawing.Color.White;
            rowStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
            rowStyle.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            rowStyle.SelectionBackColor = System.Drawing.Color.FromArgb(219, 234, 254);
            rowStyle.SelectionForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.dgvLog.RowsDefaultCellStyle = rowStyle;
            this.dgvLog.RowHeadersVisible = false;
            this.dgvLog.RowTemplate.Height = 30;
            this.dgvLog.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            // pnlBottom
            this.pnlBottom.BackColor = System.Drawing.Color.White;
            this.pnlBottom.Controls.Add(this.lblHint);
            this.pnlBottom.Controls.Add(this.btnClearLog);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Height = 48;

            // btnClearLog
            this.btnClearLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearLog.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClearLog.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnClearLog.Location = new System.Drawing.Point(955, 9);
            this.btnClearLog.Size = new System.Drawing.Size(100, 30);
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);

            // lblHint
            this.lblHint.AutoSize = true;
            this.lblHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblHint.Location = new System.Drawing.Point(20, 16);
            this.lblHint.Text = "Called AE is logged for diagnostics but is intentionally not validated.";

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.ClientSize = new System.Drawing.Size(1080, 690);
            this.Controls.Add(this.dgvLog);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlStats);
            this.Controls.Add(this.pnlConfig);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(980, 600);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mehrad Medical - DICOM Worklist SCP";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlConfig.ResumeLayout(false);
            this.pnlConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.pnlStats.ResumeLayout(false);
            this.pnlStats.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}

namespace Print_SCP
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubTitle;
        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.Label lblAe;
        private System.Windows.Forms.TextBox txtAeTitle;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Panel pnlState;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.Label lblStateDot;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabJobs;
        private System.Windows.Forms.TabPage tabReceived;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.DataGridView dgvJobs;
        private System.Windows.Forms.DataGridView dgvReceived;
        private System.Windows.Forms.DataGridView dgvLog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblActiveJobsText;
        private System.Windows.Forms.ToolStripStatusLabel lblActiveJobs;
        private System.Windows.Forms.ToolStripStatusLabel lblSeparator1;
        private System.Windows.Forms.ToolStripStatusLabel lblReceivedText;
        private System.Windows.Forms.ToolStripStatusLabel lblReceivedCount;
        private System.Windows.Forms.ToolStripStatusLabel lblSpring;
        private System.Windows.Forms.ToolStripStatusLabel lblLastEvent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubTitle = new System.Windows.Forms.Label();
            this.pnlControls = new System.Windows.Forms.Panel();
            this.lblAe = new System.Windows.Forms.Label();
            this.txtAeTitle = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.pnlState = new System.Windows.Forms.Panel();
            this.lblState = new System.Windows.Forms.Label();
            this.lblStateDot = new System.Windows.Forms.Label();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabJobs = new System.Windows.Forms.TabPage();
            this.tabReceived = new System.Windows.Forms.TabPage();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.dgvJobs = new System.Windows.Forms.DataGridView();
            this.dgvReceived = new System.Windows.Forms.DataGridView();
            this.dgvLog = new System.Windows.Forms.DataGridView();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblActiveJobsText = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblActiveJobs = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblSeparator1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblReceivedText = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblReceivedCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblSpring = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLastEvent = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReceived)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).BeginInit();
            this.SuspendLayout();

            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 86;
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);

            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(24, 16);
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 18F);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Text = "DICOM Print Gateway";

            this.lblSubTitle.AutoSize = true;
            this.lblSubTitle.Location = new System.Drawing.Point(27, 53);
            this.lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubTitle.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184);
            this.lblSubTitle.Text = "Print SCP • Windows Print • DICOM Forward • Job Monitor";

            this.pnlState.Location = new System.Drawing.Point(760, 20);
            this.pnlState.Size = new System.Drawing.Size(190, 44);
            this.pnlState.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.pnlState.BackColor = System.Drawing.Color.FromArgb(100, 116, 139);

            this.lblStateDot.AutoSize = true;
            this.lblStateDot.Location = new System.Drawing.Point(14, 10);
            this.lblStateDot.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblStateDot.Text = "○";
            this.lblStateDot.ForeColor = System.Drawing.Color.White;

            this.lblState.AutoSize = true;
            this.lblState.Location = new System.Drawing.Point(44, 13);
            this.lblState.Font = new System.Drawing.Font("Segoe UI Semibold", 10F);
            this.lblState.ForeColor = System.Drawing.Color.White;
            this.lblState.Text = "STOPPED";

            this.pnlState.Controls.Add(this.lblStateDot);
            this.pnlState.Controls.Add(this.lblState);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSubTitle);
            this.pnlHeader.Controls.Add(this.pnlState);

            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlControls.Height = 78;
            this.pnlControls.Padding = new System.Windows.Forms.Padding(20, 16, 20, 12);
            this.pnlControls.BackColor = System.Drawing.Color.White;

            this.lblAe.AutoSize = true;
            this.lblAe.Location = new System.Drawing.Point(22, 12);
            this.lblAe.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblAe.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblAe.Text = "Local AE Title";

            this.txtAeTitle.Location = new System.Drawing.Point(24, 34);
            this.txtAeTitle.Size = new System.Drawing.Size(190, 27);
            this.txtAeTitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtAeTitle.Text = "PRINT_SCP";

            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(232, 12);
            this.lblPort.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblPort.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.lblPort.Text = "Port";

            this.numPort.Location = new System.Drawing.Point(234, 34);
            this.numPort.Size = new System.Drawing.Size(100, 27);
            this.numPort.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.numPort.Maximum = 65535;
            this.numPort.Minimum = 1;
            this.numPort.Value = 105;

            this.btnStart.Location = new System.Drawing.Point(360, 30);
            this.btnStart.Size = new System.Drawing.Size(118, 34);
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.FlatAppearance.BorderSize = 0;
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnStart.Text = "▶  START";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);

            this.btnStop.Location = new System.Drawing.Point(488, 30);
            this.btnStop.Size = new System.Drawing.Size(118, 34);
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.FlatAppearance.BorderSize = 0;
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(239, 68, 68);
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnStop.Text = "■  STOP";
            this.btnStop.Enabled = false;
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);

            this.pnlControls.Controls.Add(this.lblAe);
            this.pnlControls.Controls.Add(this.txtAeTitle);
            this.pnlControls.Controls.Add(this.lblPort);
            this.pnlControls.Controls.Add(this.numPort);
            this.pnlControls.Controls.Add(this.btnStart);
            this.pnlControls.Controls.Add(this.btnStop);

            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.tabs.Padding = new System.Drawing.Point(18, 7);

            this.tabJobs.Text = "Active Jobs";
            this.tabJobs.BackColor = System.Drawing.Color.White;
            this.tabJobs.Padding = new System.Windows.Forms.Padding(10);

            this.tabReceived.Text = "Received Prints";
            this.tabReceived.BackColor = System.Drawing.Color.White;
            this.tabReceived.Padding = new System.Windows.Forms.Padding(10);

            this.tabLog.Text = "Live DICOM Log";
            this.tabLog.BackColor = System.Drawing.Color.White;
            this.tabLog.Padding = new System.Windows.Forms.Padding(10);

            ConfigureGrid(this.dgvJobs);
            ConfigureGrid(this.dgvReceived);
            ConfigureGrid(this.dgvLog);

            AddTextColumn(this.dgvJobs, "colJobTime", "Time", 85);
            AddTextColumn(this.dgvJobs, "colCallingAe", "Calling AE", 125);
            AddTextColumn(this.dgvJobs, "colCalledAe", "Called AE", 125);
            AddTextColumn(this.dgvJobs, "colRemoteIp", "Remote IP", 125);
            AddTextColumn(this.dgvJobs, "colFilm", "Film", 90);
            AddTextColumn(this.dgvJobs, "colLayout", "Layout", 135);
            AddTextColumn(this.dgvJobs, "colImages", "Images", 80);
            AddTextColumn(this.dgvJobs, "colJobStatus", "Status", 145);
            AddTextColumn(this.dgvJobs, "colJobMessage", "Message", 260);
            this.dgvJobs.Columns["colJobMessage"].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            AddTextColumn(this.dgvReceived, "colReceivedTime", "Received", 155);
            AddTextColumn(this.dgvReceived, "colReceivedCallingAe", "Calling AE", 125);
            AddTextColumn(this.dgvReceived, "colReceivedCalledAe", "Called AE", 125);
            AddTextColumn(this.dgvReceived, "colReceivedIp", "Remote IP", 125);
            AddTextColumn(this.dgvReceived, "colReceivedFilm", "Film", 90);
            AddTextColumn(this.dgvReceived, "colReceivedOrientation", "Orientation", 100);
            AddTextColumn(this.dgvReceived, "colReceivedLayout", "Layout", 140);
            AddTextColumn(this.dgvReceived, "colReceivedImages", "Images", 75);
            AddTextColumn(this.dgvReceived, "colReceivedStatus", "Status", 120);
            this.dgvReceived.Columns["colReceivedStatus"].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            AddTextColumn(this.dgvLog, "colLogTime", "Time", 100);
            AddTextColumn(this.dgvLog, "colLogIp", "Remote IP", 125);
            AddTextColumn(this.dgvLog, "colLogCalling", "Calling AE", 125);
            AddTextColumn(this.dgvLog, "colLogCalled", "Called AE", 125);
            AddTextColumn(this.dgvLog, "colLogCommand", "Command", 105);
            AddTextColumn(this.dgvLog, "colLogStatus", "Status", 105);
            AddTextColumn(this.dgvLog, "colLogMessage", "Message", 350);
            this.dgvLog.Columns["colLogMessage"].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            this.tabJobs.Controls.Add(this.dgvJobs);
            this.tabReceived.Controls.Add(this.dgvReceived);
            this.tabLog.Controls.Add(this.dgvLog);
            this.tabs.Controls.Add(this.tabJobs);
            this.tabs.Controls.Add(this.tabReceived);
            this.tabs.Controls.Add(this.tabLog);

            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.lblActiveJobsText, this.lblActiveJobs, this.lblSeparator1,
                this.lblReceivedText, this.lblReceivedCount, this.lblSpring, this.lblLastEvent });
            this.lblActiveJobsText.Text = "Jobs:";
            this.lblActiveJobs.Text = "0";
            this.lblSeparator1.Text = "   |   ";
            this.lblReceivedText.Text = "Received:";
            this.lblReceivedCount.Text = "0";
            this.lblSpring.Spring = true;
            this.lblLastEvent.Text = "Ready";

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.ClientSize = new System.Drawing.Size(1180, 720);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.pnlControls);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(980, 620);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DICOM Print Gateway";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReceived)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private static void ConfigureGrid(System.Windows.Forms.DataGridView grid)
        {
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = System.Drawing.Color.White;
            grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            grid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = System.Drawing.Color.FromArgb(226, 232, 240);
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(30, 41, 59);
            grid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            grid.ColumnHeadersHeight = 38;
            grid.RowTemplate.Height = 34;
            grid.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        }

        private static void AddTextColumn(System.Windows.Forms.DataGridView grid, string name, string header, int width)
        {
            grid.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                Width = width,
                SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
            });
        }
    }
}
namespace Worklist_Client
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.GroupBox grpConnection;
        private System.Windows.Forms.GroupBox grpQuery;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.TextBox txtCallingAE;
        private System.Windows.Forms.TextBox txtCalledAE;
        private System.Windows.Forms.Button btnEcho;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox txtPatientID;
        private System.Windows.Forms.TextBox txtPatientName;
        private System.Windows.Forms.TextBox txtAccession;
        private System.Windows.Forms.ComboBox cboModality;
        private System.Windows.Forms.TextBox txtStationAE;
        private System.Windows.Forms.TextBox txtStationName;
        private System.Windows.Forms.CheckBox chkUseDate;
        private System.Windows.Forms.DateTimePicker dtFrom;
        private System.Windows.Forms.DateTimePicker dtTo;
        private System.Windows.Forms.TextBox txtRequestedProcedureID;
        private System.Windows.Forms.TextBox txtSPSID;
        private System.Windows.Forms.TextBox txtRequestingPhysician;
        private System.Windows.Forms.TextBox txtReferringPhysician;
        private System.Windows.Forms.TextBox txtPerformingPhysician;
        private System.Windows.Forms.TextBox txtLocation;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabResults;
        private System.Windows.Forms.TabPage tabDebug;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.TextBox txtDebug;
        private System.Windows.Forms.Button btnClear;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private System.Windows.Forms.Label L(string text, int x, int y)
        {
            return new System.Windows.Forms.Label
            {
                AutoSize = true,
                Text = text,
                Location = new System.Drawing.Point(x, y),
                ForeColor = System.Drawing.Color.FromArgb(71, 85, 105),
                Font = new System.Drawing.Font("Segoe UI", 8.5F)
            };
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.root = new System.Windows.Forms.TableLayoutPanel();
            this.grpConnection = new System.Windows.Forms.GroupBox();
            this.grpQuery = new System.Windows.Forms.GroupBox();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.txtCallingAE = new System.Windows.Forms.TextBox();
            this.txtCalledAE = new System.Windows.Forms.TextBox();
            this.btnEcho = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtPatientID = new System.Windows.Forms.TextBox();
            this.txtPatientName = new System.Windows.Forms.TextBox();
            this.txtAccession = new System.Windows.Forms.TextBox();
            this.cboModality = new System.Windows.Forms.ComboBox();
            this.txtStationAE = new System.Windows.Forms.TextBox();
            this.txtStationName = new System.Windows.Forms.TextBox();
            this.chkUseDate = new System.Windows.Forms.CheckBox();
            this.dtFrom = new System.Windows.Forms.DateTimePicker();
            this.dtTo = new System.Windows.Forms.DateTimePicker();
            this.txtRequestedProcedureID = new System.Windows.Forms.TextBox();
            this.txtSPSID = new System.Windows.Forms.TextBox();
            this.txtRequestingPhysician = new System.Windows.Forms.TextBox();
            this.txtReferringPhysician = new System.Windows.Forms.TextBox();
            this.txtPerformingPhysician = new System.Windows.Forms.TextBox();
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabResults = new System.Windows.Forms.TabPage();
            this.tabDebug = new System.Windows.Forms.TabPage();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.txtDebug = new System.Windows.Forms.TextBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.root.SuspendLayout();
            this.grpConnection.SuspendLayout();
            this.grpQuery.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.tabs.SuspendLayout();
            this.tabResults.SuspendLayout();
            this.tabDebug.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.SuspendLayout();

            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 76;
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSubtitle);

            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(22, 10);
            this.lblTitle.Text = "DICOM Modality Worklist Test Client";

            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.lblSubtitle.Location = new System.Drawing.Point(25, 48);
            this.lblSubtitle.Text = "MWL C-FIND SCU • C-ECHO • fo-dicom 5.2.6";

            this.root.Dock = System.Windows.Forms.DockStyle.Fill;
            this.root.ColumnCount = 1;
            this.root.RowCount = 3;
            this.root.Padding = new System.Windows.Forms.Padding(12);
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 245F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.root.Controls.Add(this.grpConnection, 0, 0);
            this.root.Controls.Add(this.grpQuery, 0, 1);
            this.root.Controls.Add(this.tabs, 0, 2);

            this.grpConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpConnection.Text = "DICOM Connection";
            this.grpConnection.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.grpConnection.Controls.Add(this.L("Host", 18, 27));
            this.grpConnection.Controls.Add(this.L("Port", 205, 27));
            this.grpConnection.Controls.Add(this.L("Calling AE", 315, 27));
            this.grpConnection.Controls.Add(this.L("Called AE", 475, 27));
            this.grpConnection.Controls.Add(this.txtHost);
            this.grpConnection.Controls.Add(this.numPort);
            this.grpConnection.Controls.Add(this.txtCallingAE);
            this.grpConnection.Controls.Add(this.txtCalledAE);
            this.grpConnection.Controls.Add(this.btnEcho);
            this.grpConnection.Controls.Add(this.btnSend);
            this.grpConnection.Controls.Add(this.lblStatus);

            this.txtHost.Location = new System.Drawing.Point(20, 48);
            this.txtHost.Size = new System.Drawing.Size(165, 23);
            this.txtHost.Text = "127.0.0.1";
            this.numPort.Location = new System.Drawing.Point(207, 48);
            this.numPort.Minimum = 1;
            this.numPort.Maximum = 65535;
            this.numPort.Value = 11112;
            this.numPort.Size = new System.Drawing.Size(90, 23);
            this.txtCallingAE.Location = new System.Drawing.Point(317, 48);
            this.txtCallingAE.MaxLength = 16;
            this.txtCallingAE.Size = new System.Drawing.Size(140, 23);
            this.txtCallingAE.Text = "TEST_SCU";
            this.txtCalledAE.Location = new System.Drawing.Point(477, 48);
            this.txtCalledAE.MaxLength = 16;
            this.txtCalledAE.Size = new System.Drawing.Size(140, 23);
            this.txtCalledAE.Text = "WORKLIST_SCP";

            this.btnEcho.Location = new System.Drawing.Point(645, 39);
            this.btnEcho.Size = new System.Drawing.Size(105, 38);
            this.btnEcho.Text = "C-ECHO";
            this.btnEcho.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEcho.Click += new System.EventHandler(this.btnEcho_Click);

            this.btnSend.Location = new System.Drawing.Point(760, 39);
            this.btnSend.Size = new System.Drawing.Size(135, 38);
            this.btnSend.Text = "Send MWL Query";
            this.btnSend.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnSend.ForeColor = System.Drawing.Color.White;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.FlatAppearance.BorderSize = 0;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);

            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(915, 51);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblStatus.Text = "● READY";

            this.grpQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpQuery.Text = "Typical Modality Worklist Query";
            this.grpQuery.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);

            int y1=45, y2=105, y3=165;
            this.grpQuery.Controls.Add(this.L("Patient ID", 18, 24));
            this.grpQuery.Controls.Add(this.L("Patient Name", 180, 24));
            this.grpQuery.Controls.Add(this.L("Accession Number", 360, 24));
            this.grpQuery.Controls.Add(this.L("Modality", 545, 24));
            this.grpQuery.Controls.Add(this.L("Station AE", 675, 24));
            this.grpQuery.Controls.Add(this.L("Station Name", 835, 24));

            this.txtPatientID.Location = new System.Drawing.Point(20,y1); this.txtPatientID.Size=new System.Drawing.Size(140,23);
            this.txtPatientName.Location = new System.Drawing.Point(182,y1); this.txtPatientName.Size=new System.Drawing.Size(155,23);
            this.txtAccession.Location = new System.Drawing.Point(362,y1); this.txtAccession.Size=new System.Drawing.Size(160,23);
            this.cboModality.Location = new System.Drawing.Point(547,y1); this.cboModality.Size=new System.Drawing.Size(105,23); this.cboModality.DropDownStyle=System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboModality.Items.AddRange(new object[] { "(Any / Empty)", "US", "CT", "MR", "CR", "DX", "MG", "XA", "NM", "PT", "RF", "OT" });
            this.txtStationAE.Location = new System.Drawing.Point(677,y1); this.txtStationAE.Size=new System.Drawing.Size(135,23);
            this.txtStationName.Location = new System.Drawing.Point(837,y1); this.txtStationName.Size=new System.Drawing.Size(150,23);

            this.grpQuery.Controls.Add(this.L("Requested Procedure ID",18,84));
            this.grpQuery.Controls.Add(this.L("SPS ID",205,84));
            this.grpQuery.Controls.Add(this.L("Requesting Physician",340,84));
            this.grpQuery.Controls.Add(this.L("Referring Physician",540,84));
            this.grpQuery.Controls.Add(this.L("Performing Physician",735,84));
            this.grpQuery.Controls.Add(this.L("Location",930,84));

            this.txtRequestedProcedureID.Location=new System.Drawing.Point(20,y2); this.txtRequestedProcedureID.Size=new System.Drawing.Size(165,23);
            this.txtSPSID.Location=new System.Drawing.Point(207,y2); this.txtSPSID.Size=new System.Drawing.Size(110,23);
            this.txtRequestingPhysician.Location=new System.Drawing.Point(342,y2); this.txtRequestingPhysician.Size=new System.Drawing.Size(175,23);
            this.txtReferringPhysician.Location=new System.Drawing.Point(542,y2); this.txtReferringPhysician.Size=new System.Drawing.Size(170,23);
            this.txtPerformingPhysician.Location=new System.Drawing.Point(737,y2); this.txtPerformingPhysician.Size=new System.Drawing.Size(170,23);
            this.txtLocation.Location=new System.Drawing.Point(932,y2); this.txtLocation.Size=new System.Drawing.Size(115,23);

            this.chkUseDate.Location=new System.Drawing.Point(20,y3-3);
            this.chkUseDate.Size=new System.Drawing.Size(150,25);
            this.chkUseDate.Text="Use Scheduled Date";
            this.chkUseDate.Checked=true;
            this.grpQuery.Controls.Add(this.chkUseDate);
            this.grpQuery.Controls.Add(this.L("From",180,144));
            this.grpQuery.Controls.Add(this.L("To",390,144));
            this.dtFrom.Location=new System.Drawing.Point(182,y3); this.dtFrom.Size=new System.Drawing.Size(190,23); this.dtFrom.Format=System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtTo.Location=new System.Drawing.Point(392,y3); this.dtTo.Size=new System.Drawing.Size(190,23); this.dtTo.Format=System.Windows.Forms.DateTimePickerFormat.Short;

            this.grpQuery.Controls.Add(this.txtPatientID); this.grpQuery.Controls.Add(this.txtPatientName);
            this.grpQuery.Controls.Add(this.txtAccession); this.grpQuery.Controls.Add(this.cboModality);
            this.grpQuery.Controls.Add(this.txtStationAE); this.grpQuery.Controls.Add(this.txtStationName);
            this.grpQuery.Controls.Add(this.txtRequestedProcedureID); this.grpQuery.Controls.Add(this.txtSPSID);
            this.grpQuery.Controls.Add(this.txtRequestingPhysician); this.grpQuery.Controls.Add(this.txtReferringPhysician);
            this.grpQuery.Controls.Add(this.txtPerformingPhysician); this.grpQuery.Controls.Add(this.txtLocation);
            this.grpQuery.Controls.Add(this.dtFrom); this.grpQuery.Controls.Add(this.dtTo);

            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Controls.Add(this.tabResults);
            this.tabs.Controls.Add(this.tabDebug);

            this.tabResults.Text = "Worklist Results";
            this.tabResults.Controls.Add(this.dgvResults);
            this.tabResults.Controls.Add(this.btnClear);

            this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResults.AllowUserToAddRows=false;
            this.dgvResults.AllowUserToDeleteRows=false;
            this.dgvResults.ReadOnly=true;
            this.dgvResults.RowHeadersVisible=false;
            this.dgvResults.SelectionMode=System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvResults.BackgroundColor=System.Drawing.Color.White;
            this.dgvResults.BorderStyle=System.Windows.Forms.BorderStyle.None;
            this.dgvResults.AutoSizeColumnsMode=System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;

            this.btnClear.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnClear.Height = 32;
            this.btnClear.Text = "Clear Results";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);

            this.tabDebug.Text = "DICOM Debug";
            this.tabDebug.Controls.Add(this.txtDebug);
            this.txtDebug.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDebug.Multiline = true;
            this.txtDebug.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDebug.WordWrap = false;
            this.txtDebug.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtDebug.BackColor = System.Drawing.Color.FromArgb(15,23,42);
            this.txtDebug.ForeColor = System.Drawing.Color.FromArgb(226,232,240);

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1120, 760);
            this.MinimumSize = new System.Drawing.Size(1000, 680);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DICOM Worklist Test Client";
            this.Font = new System.Drawing.Font("Segoe UI",9F);
            this.Controls.Add(this.root);
            this.Controls.Add(this.pnlHeader);
            this.BackColor = System.Drawing.Color.FromArgb(248,250,252);

            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.root.ResumeLayout(false);
            this.grpConnection.ResumeLayout(false);
            this.grpConnection.PerformLayout();
            this.grpQuery.ResumeLayout(false);
            this.grpQuery.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.tabs.ResumeLayout(false);
            this.tabResults.ResumeLayout(false);
            this.tabDebug.ResumeLayout(false);
            this.tabDebug.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

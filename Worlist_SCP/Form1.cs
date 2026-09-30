using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FellowOakDicom.Network;

namespace Worlist_SCP
{
    public partial class Form1 : Form
    {
        private IDicomServer _server;
        private int _associationCount;
        private int _requestCount;
        private int _responseCount;

        public Form1()
        {
            InitializeComponent();
            ConfigureGrid();

            WorklistEvents.Log += OnWorklistLog;
            WorklistEvents.WorklistRequested += OnWorklistRequestedAsync;

            AddLocalLog("DEBUG", "STARTED", "Debug file: " + DebugTrace.CurrentFile);
        }

        private void ConfigureGrid()
        {
            dgvLog.Columns.Clear();
            dgvLog.Columns.Add(MakeColumn("Time", "Time", 82));
            dgvLog.Columns.Add(MakeColumn("Association", "Assoc", 78));
            dgvLog.Columns.Add(MakeColumn("RemoteIP", "Remote IP", 115));
            dgvLog.Columns.Add(MakeColumn("CallingAE", "Calling AE", 110));
            dgvLog.Columns.Add(MakeColumn("CalledAE", "Called AE", 110));
            dgvLog.Columns.Add(MakeColumn("Command", "Command", 105));
            dgvLog.Columns.Add(MakeColumn("Status", "Status", 85));
            dgvLog.Columns.Add(MakeColumn("Query", "Query", 235));
            dgvLog.Columns.Add(MakeColumn("Results", "Results", 65));
            dgvLog.Columns.Add(MakeColumn("Duration", "Duration", 80));
            dgvLog.Columns.Add(MakeColumn("Message", "Message", 260));

            dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvLog.Columns["Message"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private static DataGridViewTextBoxColumn MakeColumn(string name, string header, int width)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                Width = width,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_server != null)
                return;

            string aeTitle = (txtAETitle.Text ?? string.Empty).Trim().ToUpperInvariant();
            int port = (int)numPort.Value;

            if (string.IsNullOrWhiteSpace(aeTitle))
            {
                MessageBox.Show(this, "Please enter a local AE Title.", "Worklist SCP",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAETitle.Focus();
                return;
            }

            try
            {
                SetServerState("STARTING", Color.FromArgb(217, 119, 6), true);
                _server = DicomServerFactory.Create<WorklistScp>(port);

                txtAETitle.Enabled = false;
                numPort.Enabled = false;
                btnStart.Enabled = false;
                btnStop.Enabled = true;

                SetServerState("LISTENING  " + aeTitle + ":" + port,
                    Color.FromArgb(5, 150, 105), true);

                AddLocalLog("SERVER", "STARTED",
                    "Listening on port " + port + ". Called AE validation is disabled.");
            }
            catch (Exception ex)
            {
                _server?.Dispose();
                _server = null;
                SetServerState("FAILED", Color.FromArgb(220, 38, 38), false);
                AddLocalLog("SERVER", "FAILED", ex.Message);
                MessageBox.Show(this, ex.Message, "Unable to start DICOM server",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void StopServer()
        {
            if (_server == null)
                return;

            try
            {
                _server.Stop();
                _server.Dispose();
                _server = null;
                AddLocalLog("SERVER", "FINISHED", "Server stopped.");
            }
            catch (Exception ex)
            {
                AddLocalLog("SERVER", "FAILED", ex.Message);
            }
            finally
            {
                txtAETitle.Enabled = true;
                numPort.Enabled = true;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                SetServerState("STOPPED", Color.FromArgb(100, 116, 139), false);
            }
        }

        private Task<List<WorklistItem>> OnWorklistRequestedAsync(WorklistRequestEventArgs e)
        {
            string modality = e.Modality ?? string.Empty;
            string stationAe = e.ScheduledStationAETitle ?? string.Empty;
            DateTime now = DateTime.Now;

            var patients = new List<WorklistItem>
            {
                new WorklistItem
                {
                    PatientID = "TEST001",
                    PatientName = "AHMADI^ALI",
                    BirthDate = new DateTime(1984, 5, 14),
                    Sex = "M",
                    AccessionNumber = "ACC-TEST-001",
                    Modality = modality,
                    ScheduledStationAETitle = stationAe,
                    ScheduledDateTime = now,
                    ProcedureDescription = "TEST PROCEDURE 1",
                    RequestedProcedureID = "RP001",
                    ScheduledProcedureStepID = "SPS001"
                },
                new WorklistItem
                {
                    PatientID = "TEST002",
                    PatientName = "MOHAMMADI^SARA",
                    BirthDate = new DateTime(1991, 11, 3),
                    Sex = "F",
                    AccessionNumber = "ACC-TEST-002",
                    Modality = modality,
                    ScheduledStationAETitle = stationAe,
                    ScheduledDateTime = now.AddMinutes(10),
                    ProcedureDescription = "TEST PROCEDURE 2",
                    RequestedProcedureID = "RP002",
                    ScheduledProcedureStepID = "SPS002"
                },
                new WorklistItem
                {
                    PatientID = "TEST003",
                    PatientName = "REZAEI^REZA",
                    BirthDate = new DateTime(1978, 2, 22),
                    Sex = "M",
                    AccessionNumber = "ACC-TEST-003",
                    Modality = modality,
                    ScheduledStationAETitle = stationAe,
                    ScheduledDateTime = now.AddMinutes(20),
                    ProcedureDescription = "TEST PROCEDURE 3",
                    RequestedProcedureID = "RP003",
                    ScheduledProcedureStepID = "SPS003"
                }
            };

            return Task.FromResult(patients);
        }

        private void OnWorklistLog(WorklistLogEntry entry)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<WorklistLogEntry>(OnWorklistLog), entry);
                return;
            }

            if (entry.Command == "ASSOCIATE" && entry.Status == "ACCEPTED")
                _associationCount++;

            if (entry.Command == "C-FIND MWL" && entry.Status == "STARTED")
                _requestCount++;

            if (entry.Command == "C-FIND MWL" && entry.Status == "PENDING")
                _responseCount++;

            lblAssocCount.Text = _associationCount.ToString();
            lblRequestCount.Text = _requestCount.ToString();
            lblResponseCount.Text = _responseCount.ToString();

            int index = dgvLog.Rows.Add(
                entry.Time.ToString("HH:mm:ss.fff"),
                entry.AssociationId,
                entry.RemoteIP,
                entry.CallingAE,
                entry.CalledAE,
                entry.Command,
                entry.Status,
                entry.Query,
                entry.Results.HasValue ? entry.Results.Value.ToString() : "",
                entry.DurationMs.HasValue ? entry.DurationMs.Value + " ms" : "",
                entry.Message);

            PaintStatus(dgvLog.Rows[index], entry.Status);

            if (dgvLog.Rows.Count > 1000)
                dgvLog.Rows.RemoveAt(0);

            if (index >= 0)
                dgvLog.FirstDisplayedScrollingRowIndex = index;
        }

        private void AddLocalLog(string command, string status, string message)
        {
            OnWorklistLog(new WorklistLogEntry
            {
                AssociationId = "-",
                RemoteIP = "-",
                CallingAE = "-",
                CalledAE = "-",
                Command = command,
                Status = status,
                Message = message
            });
        }

        private static void PaintStatus(DataGridViewRow row, string status)
        {
            if (row == null) return;

            DataGridViewCell cell = row.Cells["Status"];
            switch ((status ?? string.Empty).ToUpperInvariant())
            {
                case "ACCEPTED":
                case "FINISHED":
                    cell.Style.ForeColor = Color.FromArgb(5, 150, 105);
                    cell.Style.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
                    break;
                case "PENDING":
                case "STARTED":
                    cell.Style.ForeColor = Color.FromArgb(37, 99, 235);
                    cell.Style.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
                    break;
                case "FAILED":
                case "REJECTED":
                    cell.Style.ForeColor = Color.FromArgb(220, 38, 38);
                    cell.Style.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
                    break;
                case "DISCONNECTED":
                case "CANCELLED":
                    cell.Style.ForeColor = Color.FromArgb(100, 116, 139);
                    break;
            }
        }

        private void SetServerState(string text, Color color, bool running)
        {
            lblServerStatus.Text = "●  " + text;
            lblServerStatus.ForeColor = color;
            lblServerStatus.BackColor = running
                ? Color.FromArgb(236, 253, 245)
                : Color.FromArgb(241, 245, 249);
        }

        private void btnExportDebug_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Title = "Export DICOM Worklist Debug Report";
                    dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    dialog.FileName = "WorklistDebug_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        DebugTrace.ExportTo(dialog.FileName);
                        MessageBox.Show(this,
                            "Debug report exported successfully.\r\n\r\n" + dialog.FileName,
                            "Debug Export",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Debug Export Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            dgvLog.Rows.Clear();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            WorklistEvents.Log -= OnWorklistLog;
            WorklistEvents.WorklistRequested -= OnWorklistRequestedAsync;
            StopServer();
        }
    }
}

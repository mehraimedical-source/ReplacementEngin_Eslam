using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using Print_SCP.Dicom;
using Print_SCP.Models;
using Print_SCP.Services;

namespace Print_SCP
{
    public partial class Form1 : Form
    {
        private readonly PrintServerController _server = new PrintServerController();
        private readonly Dictionary<Guid, DataGridViewRow> _jobRows =
            new Dictionary<Guid, DataGridViewRow>();

        private TabPage _tabDestinations;
        private CheckBox _chkWindowsPrint;
        private ComboBox _cmbWindowsPrinter;
        private CheckBox _chkDicomForward;
        private TextBox _txtDicomHost;
        private NumericUpDown _numDicomPort;
        private TextBox _txtDicomCallingAe;
        private TextBox _txtDicomCalledAe;
        private NumericUpDown _numDpi;

        public Form1()
        {
            InitializeComponent();
            BuildDestinationsTab();
            ConfigureGrids();

            PrintEvents.Log += OnLog;
            PrintEvents.JobChanged += OnJobChanged;
            PrintEvents.PrintCompleted += OnPrintCompleted;

            try
            {
                PrintRuntime.Archive.Initialize();
                LoadArchive();
            }
            catch (Exception ex)
            {
                AddLocalLog("SQLITE", "FAILED", ex.Message);
            }

            LoadWindowsPrinters();
            SetServerState(false, "STOPPED");
            AddLocalLog("APPLICATION", "READY", "Print SCP is ready.");
        }

        private void ConfigureGrids()
        {
            dgvJobs.AutoGenerateColumns = false;
            dgvReceived.AutoGenerateColumns = false;
            dgvLog.AutoGenerateColumns = false;

            if (!dgvReceived.Columns.Contains("colThumbnail"))
            {
                var thumbnailColumn = new DataGridViewImageColumn
                {
                    Name = "colThumbnail",
                    HeaderText = "Preview",
                    Width = 92,
                    ImageLayout = DataGridViewImageCellLayout.Zoom
                };
                dgvReceived.Columns.Insert(0, thumbnailColumn);
                dgvReceived.RowTemplate.Height = 74;
            }

            dgvReceived.CellDoubleClick += DgvReceived_CellDoubleClick;
        }

        private void BuildDestinationsTab()
        {
            _tabDestinations = new TabPage("Destinations")
            {
                BackColor = Color.White,
                Padding = new Padding(18)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 7,
                Padding = new Padding(6)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));

            _chkWindowsPrint = new CheckBox
            {
                Text = "Enable Windows Print",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10F)
            };

            _cmbWindowsPrinter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 260
            };

            _chkDicomForward = new CheckBox
            {
                Text = "Enable DICOM Forward",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10F)
            };

            _txtDicomHost = new TextBox { Text = "127.0.0.1", Width = 220 };
            _numDicomPort = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 65535,
                Value = 104,
                Width = 100
            };
            _txtDicomCallingAe = new TextBox { Text = "PRINT_GATEWAY", Width = 180 };
            _txtDicomCalledAe = new TextBox { Text = "DICOM_PRINTER", Width = 180 };
            _numDpi = new NumericUpDown
            {
                Minimum = 72,
                Maximum = 1200,
                Value = 300,
                Increment = 50,
                Width = 100
            };

            layout.Controls.Add(_chkWindowsPrint, 0, 0);
            layout.SetColumnSpan(_chkWindowsPrint, 2);
            layout.Controls.Add(_chkDicomForward, 2, 0);
            layout.SetColumnSpan(_chkDicomForward, 2);

            layout.Controls.Add(MakeSettingsLabel("Windows Printer"), 0, 1);
            layout.Controls.Add(_cmbWindowsPrinter, 1, 1);
            layout.Controls.Add(MakeSettingsLabel("DICOM Printer Host"), 2, 1);
            layout.Controls.Add(_txtDicomHost, 3, 1);

            layout.Controls.Add(MakeSettingsLabel("Render DPI"), 0, 2);
            layout.Controls.Add(_numDpi, 1, 2);
            layout.Controls.Add(MakeSettingsLabel("DICOM Printer Port"), 2, 2);
            layout.Controls.Add(_numDicomPort, 3, 2);

            layout.Controls.Add(new Label(), 0, 3);
            layout.Controls.Add(new Label(), 1, 3);
            layout.Controls.Add(MakeSettingsLabel("Calling AE"), 2, 3);
            layout.Controls.Add(_txtDicomCallingAe, 3, 3);

            layout.Controls.Add(new Label(), 0, 4);
            layout.Controls.Add(new Label(), 1, 4);
            layout.Controls.Add(MakeSettingsLabel("Called AE"), 2, 4);
            layout.Controls.Add(_txtDicomCalledAe, 3, 4);

            var note = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(900, 0),
                ForeColor = Color.FromArgb(71, 85, 105),
                Text = "A received DICOM Print can be rendered once and routed to Windows Print, " +
                       "another DICOM printer, both destinations, or archive only."
            };
            layout.Controls.Add(note, 0, 6);
            layout.SetColumnSpan(note, 4);

            _tabDestinations.Controls.Add(layout);
            tabs.Controls.Add(_tabDestinations);
        }

        private static Label MakeSettingsLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(3, 7, 3, 3),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
        }

        private void LoadWindowsPrinters()
        {
            _cmbWindowsPrinter.Items.Clear();

            foreach (string printer in PrinterSettings.InstalledPrinters)
                _cmbWindowsPrinter.Items.Add(printer);

            if (_cmbWindowsPrinter.Items.Count > 0)
                _cmbWindowsPrinter.SelectedIndex = 0;
        }

        private void LoadArchive()
        {
            foreach (var item in PrintRuntime.Archive.GetRecent(250))
                AddReceivedRow(item, false);

            lblReceivedCount.Text = dgvReceived.Rows.Count.ToString();
        }

        private void ApplyRuntimeSettings()
        {
            PrintRuntime.Settings = new PrintRuntimeSettings
            {
                RenderDpi = (int)_numDpi.Value,
                WindowsPrintEnabled = _chkWindowsPrint.Checked,
                WindowsPrinterName = Convert.ToString(_cmbWindowsPrinter.SelectedItem) ?? "",
                DicomForwardEnabled = _chkDicomForward.Checked,
                DicomRemoteHost = (_txtDicomHost.Text ?? "").Trim(),
                DicomRemotePort = (int)_numDicomPort.Value,
                DicomCallingAe = (_txtDicomCallingAe.Text ?? "").Trim().ToUpperInvariant(),
                DicomCalledAe = (_txtDicomCalledAe.Text ?? "").Trim().ToUpperInvariant()
            };
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_server.IsRunning) return;

            try
            {
                ApplyRuntimeSettings();

                int port = (int)numPort.Value;
                _server.Start(port);

                txtAeTitle.Enabled = false;
                numPort.Enabled = false;
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                SetDestinationControlsEnabled(false);

                SetServerState(true, "LISTENING  *:" + port);
                AddLocalLog("SERVER", "STARTED",
                    "Accepting any Calling AE and any Called AE on port " + port + ".");
            }
            catch (Exception ex)
            {
                SetServerState(false, "FAILED");
                AddLocalLog("SERVER", "FAILED", ex.Message);
                MessageBox.Show(this, ex.Message, "Print SCP",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void StopServer()
        {
            try
            {
                _server.Stop();
                AddLocalLog("SERVER", "STOPPED", "Listener stopped.");
            }
            catch (Exception ex)
            {
                AddLocalLog("SERVER", "FAILED", ex.Message);
            }
            finally
            {
                txtAeTitle.Enabled = true;
                numPort.Enabled = true;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                SetDestinationControlsEnabled(true);
                SetServerState(false, "STOPPED");
            }
        }

        private void SetDestinationControlsEnabled(bool enabled)
        {
            _chkWindowsPrint.Enabled = enabled;
            _cmbWindowsPrinter.Enabled = enabled;
            _chkDicomForward.Enabled = enabled;
            _txtDicomHost.Enabled = enabled;
            _numDicomPort.Enabled = enabled;
            _txtDicomCallingAe.Enabled = enabled;
            _txtDicomCalledAe.Enabled = enabled;
            _numDpi.Enabled = enabled;
        }

        private void SetServerState(bool running, string text)
        {
            lblState.Text = text;
            pnlState.BackColor = running
                ? Color.FromArgb(16, 185, 129)
                : Color.FromArgb(100, 116, 139);
            lblStateDot.Text = running ? "●" : "○";
            lblStateDot.ForeColor = Color.White;
        }

        private void OnJobChanged(PrintJobInfo job)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<PrintJobInfo>(OnJobChanged), job);
                return;
            }

            DataGridViewRow row;
            if (!_jobRows.TryGetValue(job.JobId, out row))
            {
                int index = dgvJobs.Rows.Add();
                row = dgvJobs.Rows[index];
                row.Tag = job.JobId;
                _jobRows[job.JobId] = row;
            }

            row.Cells["colJobTime"].Value = job.CreatedAt.ToString("HH:mm:ss");
            row.Cells["colCallingAe"].Value = job.CallingAeTitle;
            row.Cells["colCalledAe"].Value = job.CalledAeTitle;
            row.Cells["colRemoteIp"].Value = job.RemoteIp;
            row.Cells["colFilm"].Value = job.FilmSizeId;
            row.Cells["colLayout"].Value = job.Layout;
            row.Cells["colImages"].Value = job.TotalImages > 0
                ? job.ReceivedImages + "/" + job.TotalImages
                : job.ReceivedImages.ToString();
            row.Cells["colJobStatus"].Value = job.Status;
            row.Cells["colJobMessage"].Value = job.Message;

            lblActiveJobs.Text = _jobRows.Count.ToString();
        }

        private void OnPrintCompleted(RenderedPrintInfo item)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<RenderedPrintInfo>(OnPrintCompleted), item);
                return;
            }

            AddReceivedRow(item, true);

            DataGridViewRow activeRow;
            if (_jobRows.TryGetValue(item.JobId, out activeRow))
            {
                dgvJobs.Rows.Remove(activeRow);
                _jobRows.Remove(item.JobId);
            }

            lblActiveJobs.Text = _jobRows.Count.ToString();
            lblReceivedCount.Text = dgvReceived.Rows.Count.ToString();
            tabs.SelectedTab = tabReceived;
        }

        private void AddReceivedRow(RenderedPrintInfo item, bool insertAtTop)
        {
            int index;

            if (insertAtTop)
            {
                dgvReceived.Rows.Insert(0, 1);
                index = 0;
            }
            else
            {
                index = dgvReceived.Rows.Add();
            }

            DataGridViewRow row = dgvReceived.Rows[index];
            row.Tag = item;
            row.Cells["colThumbnail"].Value = item.Thumbnail ?? LoadImageCopy(item.ThumbnailPath);
            row.Cells["colReceivedTime"].Value = item.ReceivedAt.ToString("yyyy-MM-dd HH:mm:ss");
            row.Cells["colReceivedCallingAe"].Value = item.CallingAeTitle;
            row.Cells["colReceivedCalledAe"].Value = item.CalledAeTitle;
            row.Cells["colReceivedIp"].Value = item.RemoteIp;
            row.Cells["colReceivedFilm"].Value = item.FilmSizeId;
            row.Cells["colReceivedOrientation"].Value = item.Orientation;
            row.Cells["colReceivedLayout"].Value = item.Layout;
            row.Cells["colReceivedImages"].Value = item.ImageCount;
            row.Cells["colReceivedStatus"].Value =
                "WIN: " + item.WindowsPrintStatus + " | DICOM: " + item.DicomForwardStatus;
        }

        private static Image LoadImageCopy(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return null;

                using (var source = Image.FromFile(path))
                    return new Bitmap(source);
            }
            catch
            {
                return null;
            }
        }

        private void DgvReceived_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var item = dgvReceived.Rows[e.RowIndex].Tag as RenderedPrintInfo;
            if (item == null || string.IsNullOrWhiteSpace(item.RenderedImagePath) ||
                !File.Exists(item.RenderedImagePath))
                return;

            using (var image = LoadImageCopy(item.RenderedImagePath))
            {
                if (image == null) return;

                var preview = new Form
                {
                    Text = item.FilmSizeId + " - " + item.Layout,
                    WindowState = FormWindowState.Maximized,
                    BackColor = Color.Black
                };

                var box = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = new Bitmap(image)
                };

                preview.Controls.Add(box);
                preview.FormClosed += (s, args) => box.Image?.Dispose();
                preview.ShowDialog(this);
            }
        }

        private void OnLog(PrintLogEntry entry)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<PrintLogEntry>(OnLog), entry);
                return;
            }

            dgvLog.Rows.Add(
                entry.Time.ToString("HH:mm:ss.fff"),
                entry.RemoteIp,
                entry.CallingAe,
                entry.CalledAe,
                entry.Command,
                entry.Status,
                entry.Message);

            if (dgvLog.Rows.Count > 1500)
                dgvLog.Rows.RemoveAt(0);

            if (dgvLog.Rows.Count > 0)
                dgvLog.FirstDisplayedScrollingRowIndex = dgvLog.Rows.Count - 1;

            lblLastEvent.Text = entry.Command + "  " + entry.Status;
        }

        private void AddLocalLog(string command, string status, string message)
        {
            OnLog(new PrintLogEntry
            {
                Command = command,
                Status = status,
                Message = message
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            PrintEvents.Log -= OnLog;
            PrintEvents.JobChanged -= OnJobChanged;
            PrintEvents.PrintCompleted -= OnPrintCompleted;

            _server.Dispose();

            foreach (DataGridViewRow row in dgvReceived.Rows)
            {
                var image = row.Cells["colThumbnail"].Value as Image;
                image?.Dispose();
            }

            base.OnFormClosing(e);
        }
    }
}
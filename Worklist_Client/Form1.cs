using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;

namespace Worklist_Client
{
    public partial class Form1 : Form
    {
        private readonly List<DicomDataset> _responses = new List<DicomDataset>();

        public Form1()
        {
            InitializeComponent();
            ConfigureResultsGrid();
            cboModality.SelectedIndex = 0;
            dtFrom.Value = DateTime.Today;
            dtTo.Value = DateTime.Today;
        }

        private void ConfigureResultsGrid()
        {
            dgvResults.Columns.Clear();
            dgvResults.Columns.Add(MakeColumn("PatientID", "Patient ID", 90));
            dgvResults.Columns.Add(MakeColumn("PatientName", "Patient Name", 160));
            dgvResults.Columns.Add(MakeColumn("Sex", "Sex", 50));
            dgvResults.Columns.Add(MakeColumn("BirthDate", "Birth Date", 90));
            dgvResults.Columns.Add(MakeColumn("Accession", "Accession", 110));
            dgvResults.Columns.Add(MakeColumn("Modality", "Modality", 70));
            dgvResults.Columns.Add(MakeColumn("StationAE", "Station AE", 100));
            dgvResults.Columns.Add(MakeColumn("SPSDate", "Scheduled Date", 105));
            dgvResults.Columns.Add(MakeColumn("SPSTime", "Scheduled Time", 95));
            dgvResults.Columns.Add(MakeColumn("Description", "Procedure", 180));
            dgvResults.Columns.Add(MakeColumn("SPSID", "SPS ID", 90));
            dgvResults.Columns.Add(MakeColumn("RequestedProcedureID", "Requested Proc ID", 120));
            dgvResults.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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

        private async void btnSend_Click(object sender, EventArgs e)
        {
            btnSend.Enabled = false;
            dgvResults.Rows.Clear();
            _responses.Clear();
            txtDebug.Clear();
            lblStatus.Text = "● CONNECTING";
            lblStatus.ForeColor = Color.FromArgb(217, 119, 6);

            try
            {
                var request = BuildWorklistRequest();
                DumpDataset("C-FIND REQUEST", request.Dataset);

                request.OnResponseReceived += (req, response) =>
                {
                    if (IsDisposed) return;

                    BeginInvoke(new Action(() =>
                    {
                        AppendDebug("RESPONSE STATUS: " + response.Status);

                        if (response.Dataset != null)
                        {
                            _responses.Add(response.Dataset);
                            AddResponseRow(response.Dataset);
                            DumpDataset("C-FIND RESPONSE #" + _responses.Count, response.Dataset);
                        }
                    }));
                };

                string host = txtHost.Text.Trim();
                int port = (int)numPort.Value;
                string callingAe = txtCallingAE.Text.Trim().ToUpperInvariant();
                string calledAe = txtCalledAE.Text.Trim().ToUpperInvariant();

                AppendDebug("TARGET       : " + host + ":" + port);
                AppendDebug("CALLING AE   : " + callingAe);
                AppendDebug("CALLED AE    : " + calledAe);

                var client = DicomClientFactory.Create(host, port, false, callingAe, calledAe);
                await client.AddRequestAsync(request);
                await client.SendAsync();

                lblStatus.Text = "● FINISHED - " + _responses.Count + " item(s)";
                lblStatus.ForeColor = Color.FromArgb(5, 150, 105);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "● FAILED";
                lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
                AppendDebug("ERROR:");
                AppendDebug(ex.ToString());
                MessageBox.Show(this, ex.Message, "MWL Client Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSend.Enabled = true;
            }
        }

        private DicomCFindRequest BuildWorklistRequest()
        {
            string patientId = ValueOrNull(txtPatientID.Text);
            string patientName = ValueOrNull(txtPatientName.Text);
            string stationAe = ValueOrNull(txtStationAE.Text);
            string stationName = ValueOrNull(txtStationName.Text);
            string modality = cboModality.SelectedIndex <= 0 ? null : cboModality.Text;

            DicomDateRange range = null;
            if (chkUseDate.Checked)
            {
                DateTime from = dtFrom.Value.Date;
                DateTime to = dtTo.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                range = new DicomDateRange(from, to);
            }

            var request = DicomCFindRequest.CreateWorklistQuery(
                patientId: patientId,
                patientName: patientName,
                stationAE: stationAe,
                stationName: stationName,
                modality: modality,
                scheduledDateTime: range);

            request.Dataset.AddOrUpdate(DicomTag.AccessionNumber, ValueOrEmpty(txtAccession.Text));
            request.Dataset.AddOrUpdate(DicomTag.RequestedProcedureID, ValueOrEmpty(txtRequestedProcedureID.Text));
            request.Dataset.AddOrUpdate(DicomTag.RequestingPhysician, ValueOrEmpty(txtRequestingPhysician.Text));
            request.Dataset.AddOrUpdate(DicomTag.ReferringPhysicianName, ValueOrEmpty(txtReferringPhysician.Text));

            if (request.Dataset.TryGetSequence(DicomTag.ScheduledProcedureStepSequence, out var sps) &&
                sps.Items.Count > 0)
            {
                var item = sps.Items[0];
                item.AddOrUpdate(DicomTag.ScheduledProcedureStepID, ValueOrEmpty(txtSPSID.Text));
                item.AddOrUpdate(DicomTag.ScheduledPerformingPhysicianName, ValueOrEmpty(txtPerformingPhysician.Text));
                item.AddOrUpdate(DicomTag.ScheduledProcedureStepLocation, ValueOrEmpty(txtLocation.Text));
            }

            return request;
        }

        private void AddResponseRow(DicomDataset ds)
        {
            var sps = GetFirstSequenceItem(ds, DicomTag.ScheduledProcedureStepSequence);

            dgvResults.Rows.Add(
                Get(ds, DicomTag.PatientID),
                Get(ds, DicomTag.PatientName),
                Get(ds, DicomTag.PatientSex),
                Get(ds, DicomTag.PatientBirthDate),
                Get(ds, DicomTag.AccessionNumber),
                Get(sps, DicomTag.Modality),
                Get(sps, DicomTag.ScheduledStationAETitle),
                Get(sps, DicomTag.ScheduledProcedureStepStartDate),
                Get(sps, DicomTag.ScheduledProcedureStepStartTime),
                Get(sps, DicomTag.ScheduledProcedureStepDescription),
                Get(sps, DicomTag.ScheduledProcedureStepID),
                Get(ds, DicomTag.RequestedProcedureID));
        }

        private static DicomDataset GetFirstSequenceItem(DicomDataset ds, DicomTag tag)
        {
            if (ds != null && ds.TryGetSequence(tag, out var seq) && seq.Items.Count > 0)
                return seq.Items[0];
            return null;
        }

        private static string Get(DicomDataset ds, DicomTag tag)
        {
            if (ds == null) return "";
            return ds.TryGetString(tag, out var value) ? value ?? "" : "";
        }

        private static string ValueOrNull(string text)
        {
            string value = (text ?? "").Trim();
            return value.Length == 0 ? null : value;
        }

        private static string ValueOrEmpty(string text)
        {
            return (text ?? "").Trim();
        }

        private void btnEcho_Click(object sender, EventArgs e)
        {
            SendEchoAsync();
        }

        private async void SendEchoAsync()
        {
            btnEcho.Enabled = false;
            try
            {
                bool success = false;
                var echo = new DicomCEchoRequest();
                echo.OnResponseReceived += (req, response) =>
                {
                    success = response.Status == DicomStatus.Success;
                    BeginInvoke(new Action(() => AppendDebug("C-ECHO: " + response.Status)));
                };

                var client = DicomClientFactory.Create(
                    txtHost.Text.Trim(),
                    (int)numPort.Value,
                    false,
                    txtCallingAE.Text.Trim().ToUpperInvariant(),
                    txtCalledAE.Text.Trim().ToUpperInvariant());

                await client.AddRequestAsync(echo);
                await client.SendAsync();

                lblStatus.Text = success ? "● ECHO SUCCESS" : "● ECHO FAILED";
                lblStatus.ForeColor = success
                    ? Color.FromArgb(5, 150, 105)
                    : Color.FromArgb(220, 38, 38);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "● ECHO FAILED";
                lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
                AppendDebug(ex.ToString());
            }
            finally
            {
                btnEcho.Enabled = true;
            }
        }

        private void DumpDataset(string title, DicomDataset ds)
        {
            AppendDebug("");
            AppendDebug("========== " + title + " ==========");
            if (ds == null)
            {
                AppendDebug("<null>");
                return;
            }

            foreach (var item in ds)
            {
                if (item is DicomSequence seq)
                {
                    AppendDebug(item.Tag + " SQ " + item.Tag.DictionaryEntry.Keyword);
                    foreach (var child in seq.Items)
                    {
                        foreach (var childItem in child)
                            AppendDebug("  " + FormatItem(child, childItem));
                    }
                }
                else
                {
                    AppendDebug(FormatItem(ds, item));
                }
            }
        }

        private static string FormatItem(DicomDataset ds, DicomItem item)
        {
            string value = "";
            try
            {
                if (item is DicomElement)
                    ds.TryGetString(item.Tag, out value);
            }
            catch { }

            return item.Tag + " " + item.ValueRepresentation.Code +
                   " [" + (value ?? "") + "] " + item.Tag.DictionaryEntry.Keyword;
        }

        private void AppendDebug(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendDebug), text);
                return;
            }

            txtDebug.AppendText(text + Environment.NewLine);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            dgvResults.Rows.Clear();
            txtDebug.Clear();
            _responses.Clear();
            lblStatus.Text = "● READY";
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
        }
    }
}

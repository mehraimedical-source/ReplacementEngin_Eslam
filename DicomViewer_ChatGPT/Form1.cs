using System;
using System.IO;
using System.Windows.Forms;

namespace DicomViewer_ChatGPT
{
    public partial class Form1 : Form
    {
        public Form1() { InitializeComponent(); }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a folder containing a DICOM study/series";
                if (Directory.Exists(txtFolder.Text)) dialog.SelectedPath = txtFolder.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK) txtFolder.Text = dialog.SelectedPath;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                UseWaitCursor = true;
                lblState.Text = "Scanning and loading DICOM...";
                Application.DoEvents();
                dicomViewer.ActiveFolder(txtFolder.Text);
                lblState.Text = "Loaded";
            }
            catch (Exception ex)
            {
                lblState.Text = "Failed";
                MessageBox.Show(this, ex.ToString(), "DICOM load error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { UseWaitCursor = false; }
        }
    }
}

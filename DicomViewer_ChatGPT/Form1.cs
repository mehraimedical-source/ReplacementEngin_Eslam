using System;
using System.IO;
using System.Windows.Forms;

namespace DicomViewer_ChatGPT
{
    public partial class Form1 : Form
    {
        public Form1() { InitializeComponent(); }

        private void btnResetAxes_Click(object sender, EventArgs e)
        {
            dicomViewer.ResetAxes();
        }

        private void btnReset3D_Click(object sender, EventArgs e)
        {
            dicomViewer.Reset3D();
        }

        private void numAxisWidth_ValueChanged(object sender, EventArgs e)
        {
            // ضخامت خطوط محورهای MPR مستقیماً از Toolbar قابل تنظیم است.
            dicomViewer.AxisLineWidth = (float)numAxisWidth.Value;
        }

        private void btnWindowLevel_Click(object sender, EventArgs e)
        {
            // ابزار Window/Level به صورت Toggle فعال می‌شود.
            dicomViewer.WindowLevelMode=!dicomViewer.WindowLevelMode;
            btnWindowLevel.BackColor=dicomViewer.WindowLevelMode
                ? System.Drawing.Color.DimGray
                : System.Drawing.Color.FromArgb(55,55,55);
            lblWindowLevel.Text=String.Format("WL: {0:0}   WW: {1:0}",dicomViewer.WindowCenter,dicomViewer.WindowWidth);
        }

        private void btnDefaultWindowLevel_Click(object sender, EventArgs e)
        {
            // W/L را به مقدار اولیه همان Series که هنگام Load محاسبه شده بود برمی‌گرداند.
            dicomViewer.ResetWindowLevel();
            lblWindowLevel.Text=String.Format("WL: {0:0}   WW: {1:0}",dicomViewer.WindowCenter,dicomViewer.WindowWidth);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            txtFolder.Text = @"SR002";
            return;
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

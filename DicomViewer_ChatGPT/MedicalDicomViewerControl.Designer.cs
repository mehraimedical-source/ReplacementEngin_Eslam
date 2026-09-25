namespace DicomViewer_ChatGPT
{
    partial class MedicalDicomViewerControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel grid;
        private System.Windows.Forms.Panel panelAxial;
        private System.Windows.Forms.Panel panelSagittal;
        private System.Windows.Forms.Panel panelCoronal;
        private System.Windows.Forms.Panel panel3D;
        private System.Windows.Forms.Label lblAxial;
        private System.Windows.Forms.Label lblSagittal;
        private System.Windows.Forms.Label lblCoronal;
        private System.Windows.Forms.Label lbl3D;
        private System.Windows.Forms.PictureBox axial;
        private System.Windows.Forms.PictureBox sagittal;
        private System.Windows.Forms.PictureBox coronal;
        private System.Windows.Forms.PictureBox volume3D;
        private System.Windows.Forms.Label status;

        private void InitializeComponent()
        {
            this.grid = new System.Windows.Forms.TableLayoutPanel();
            this.panelAxial = new System.Windows.Forms.Panel();
            this.panelSagittal = new System.Windows.Forms.Panel();
            this.panelCoronal = new System.Windows.Forms.Panel();
            this.panel3D = new System.Windows.Forms.Panel();
            this.lblAxial = new System.Windows.Forms.Label();
            this.lblSagittal = new System.Windows.Forms.Label();
            this.lblCoronal = new System.Windows.Forms.Label();
            this.lbl3D = new System.Windows.Forms.Label();
            this.axial = new System.Windows.Forms.PictureBox();
            this.sagittal = new System.Windows.Forms.PictureBox();
            this.coronal = new System.Windows.Forms.PictureBox();
            this.volume3D = new System.Windows.Forms.PictureBox();
            this.status = new System.Windows.Forms.Label();
            this.grid.SuspendLayout();
            this.panelAxial.SuspendLayout();
            this.panelSagittal.SuspendLayout();
            this.panelCoronal.SuspendLayout();
            this.panel3D.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axial)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sagittal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.coronal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.volume3D)).BeginInit();
            this.SuspendLayout();

            this.grid.BackColor = System.Drawing.Color.Black;
            this.grid.ColumnCount = 2;
            this.grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.grid.Controls.Add(this.panelAxial, 0, 0);
            this.grid.Controls.Add(this.panelSagittal, 1, 0);
            this.grid.Controls.Add(this.panelCoronal, 0, 1);
            this.grid.Controls.Add(this.panel3D, 1, 1);
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.Margin = new System.Windows.Forms.Padding(0);
            this.grid.Name = "grid";
            this.grid.RowCount = 2;
            this.grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.grid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.grid.Size = new System.Drawing.Size(1000, 676);

            SetupViewport(this.panelAxial, this.axial, this.lblAxial, "AXIAL");
            SetupViewport(this.panelSagittal, this.sagittal, this.lblSagittal, "SAGITTAL");
            SetupViewport(this.panelCoronal, this.coronal, this.lblCoronal, "CORONAL");
            SetupViewport(this.panel3D, this.volume3D, this.lbl3D, "3D VOLUME - BONE");

            // Match each viewport title to the MPR plane color used by the cross-reference lines.
            // Keep the title bar black; color only the compact area occupied by each word.
            // Compact colored titles must stay above the Dock=Fill PictureBox.
            // BringToFront prevents the image control from covering AutoSize labels.
            this.lblAxial.Dock = System.Windows.Forms.DockStyle.None;
            this.lblAxial.AutoSize = true;
            this.lblAxial.Location = new System.Drawing.Point(4, 2);
            this.lblAxial.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.lblAxial.BackColor = System.Drawing.Color.Yellow;
            this.lblAxial.ForeColor = System.Drawing.Color.Black;
            this.lblSagittal.Dock = System.Windows.Forms.DockStyle.None;
            this.lblSagittal.AutoSize = true;
            this.lblSagittal.Location = new System.Drawing.Point(4, 2);
            this.lblSagittal.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.lblSagittal.BackColor = System.Drawing.Color.Cyan;
            this.lblSagittal.ForeColor = System.Drawing.Color.Black;
            this.lblCoronal.Dock = System.Windows.Forms.DockStyle.None;
            this.lblCoronal.AutoSize = true;
            this.lblCoronal.Location = new System.Drawing.Point(4, 2);
            this.lblCoronal.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.lblCoronal.BackColor = System.Drawing.Color.Magenta;
            this.lblCoronal.ForeColor = System.Drawing.Color.White;
            this.lblAxial.BringToFront();
            this.lblSagittal.BringToFront();
            this.lblCoronal.BringToFront();

            this.status.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.status.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.status.ForeColor = System.Drawing.Color.Gainsboro;
            this.status.Height = 24;
            this.status.Name = "status";
            this.status.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.status.Text = "Ready";
            this.status.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.grid);
            this.Controls.Add(this.status);
            this.Name = "MedicalDicomViewerControl";
            this.Size = new System.Drawing.Size(1000, 700);

            this.grid.ResumeLayout(false);
            this.panelAxial.ResumeLayout(false);
            this.panelSagittal.ResumeLayout(false);
            this.panelCoronal.ResumeLayout(false);
            this.panel3D.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.axial)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sagittal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.coronal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.volume3D)).EndInit();
            this.ResumeLayout(false);
        }

        private static void SetupViewport(System.Windows.Forms.Panel panel, System.Windows.Forms.PictureBox picture, System.Windows.Forms.Label title, string text)
        {
            // یک حاشیه باریک دور هر سلول، محدوده هر Viewport را مشخص می‌کند.
            // Padding باعث می‌شود رنگ Panel مثل Border دیده شود و روی خود تصویر اثری ندارد.
            panel.BackColor = System.Drawing.Color.DimGray;
            panel.Dock = System.Windows.Forms.DockStyle.Fill;
            panel.Margin = new System.Windows.Forms.Padding(1);
            panel.Padding = new System.Windows.Forms.Padding(1);
            picture.BackColor = System.Drawing.Color.Black;
            picture.Dock = System.Windows.Forms.DockStyle.Fill;
            picture.Name = text.Replace(" ", "") + "Picture";
            picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picture.TabStop = true;
            title.BackColor = System.Drawing.Color.Black;
            title.Dock = System.Windows.Forms.DockStyle.Top;
            title.ForeColor = System.Drawing.Color.LimeGreen;
            title.Height = 20;
            title.Padding = new System.Windows.Forms.Padding(4, 2, 0, 0);
            title.Text = text;
            panel.Controls.Add(picture);
            panel.Controls.Add(title);
        }
    }
}

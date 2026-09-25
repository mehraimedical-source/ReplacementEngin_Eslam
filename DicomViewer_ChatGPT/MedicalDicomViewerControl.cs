using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DicomViewer_ChatGPT
{
    public sealed class MedicalDicomViewerControl : UserControl
    {
        private readonly TableLayoutPanel grid;
        private readonly PictureBox axial;
        private readonly PictureBox sagittal;
        private readonly PictureBox coronal;
        private readonly PictureBox volume3D;
        private readonly Label status;

        private ProcessedDicomImage[] volume;
        private int width, height, depth;
        private int xIndex, yIndex, zIndex;

        public MedicalDicomViewerControl()
        {
            BackColor = Color.Black;
            grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, BackColor = Color.Black };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            axial = MakeView("AXIAL");
            sagittal = MakeView("SAGITTAL");
            coronal = MakeView("CORONAL");
            volume3D = MakeView("3D MIP PREVIEW");
            grid.Controls.Add(axial, 0, 0);
            grid.Controls.Add(sagittal, 1, 0);
            grid.Controls.Add(coronal, 0, 1);
            grid.Controls.Add(volume3D, 1, 1);

            status = new Label { Dock = DockStyle.Bottom, Height = 24, ForeColor = Color.Gainsboro, BackColor = Color.FromArgb(30,30,30), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8,0,0,0) };
            Controls.Add(grid);
            Controls.Add(status);

            axial.MouseWheel += delegate(object s, MouseEventArgs e) { if (depth > 0) { zIndex = Clamp(zIndex + Math.Sign(e.Delta), 0, depth - 1); RefreshViews(); } };
            sagittal.MouseWheel += delegate(object s, MouseEventArgs e) { if (width > 0) { xIndex = Clamp(xIndex + Math.Sign(e.Delta), 0, width - 1); RefreshViews(); } };
            coronal.MouseWheel += delegate(object s, MouseEventArgs e) { if (height > 0) { yIndex = Clamp(yIndex + Math.Sign(e.Delta), 0, height - 1); RefreshViews(); } };
        }

        public void Active(string[] dicomFiles)
        {
            Active(DicomSeriesLoader.Load(dicomFiles));
        }

        public void Active(ProcessedDicomImage[] images)
        {
            if (images == null || images.Length == 0) throw new ArgumentException("No images supplied.");
            foreach (var image in images) image.Validate();

            width = images[0].Width;
            height = images[0].Height;
            if (images.Any(x => x.Width != width || x.Height != height))
                throw new ArgumentException("All slices must have the same dimensions.");

            volume = images;
            depth = images.Length;
            xIndex = width / 2;
            yIndex = height / 2;
            zIndex = depth / 2;
            RefreshViews();
        }

        public void ActiveFolder(string folder)
        {
            string[] files = DicomSeriesLoader.FindLargestImageSeries(folder);
            if (files.Length == 0) throw new InvalidOperationException("No image DICOM series was found in this folder.");
            Active(files);
        }

        private PictureBox MakeView(string name)
        {
            var host = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black, Margin = new Padding(1) };
            var title = new Label { Text = name, Dock = DockStyle.Top, Height = 20, ForeColor = Color.LimeGreen, BackColor = Color.Black, Padding = new Padding(4,2,0,0) };
            var box = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom, TabStop = true };
            host.Controls.Add(box);
            host.Controls.Add(title);
            grid.Controls.Add(host);
            return box;
        }

        private void RefreshViews()
        {
            SetImage(axial, BuildAxial());
            SetImage(sagittal, BuildSagittal());
            SetImage(coronal, BuildCoronal());
            SetImage(volume3D, BuildMip());
            status.Text = String.Format("Volume {0} x {1} x {2}    Crosshair X:{3} Y:{4} Z:{5}    Mouse wheel: change plane", width, height, depth, xIndex, yIndex, zIndex);
        }

        private Bitmap BuildAxial()
        {
            byte[] pixels = (byte[])volume[zIndex].Gray8.Clone();
            Bitmap b = GrayBitmap(pixels, width, height);
            DrawCrosshair(b, xIndex, yIndex, Color.Cyan, Color.Magenta);
            return b;
        }

        private Bitmap BuildCoronal()
        {
            byte[] p = new byte[width * depth];
            for (int z = 0; z < depth; z++)
                Buffer.BlockCopy(volume[z].Gray8, yIndex * width, p, (depth - 1 - z) * width, width);
            Bitmap b = GrayBitmap(p, width, depth);
            DrawCrosshair(b, xIndex, depth - 1 - zIndex, Color.Cyan, Color.Yellow);
            return b;
        }

        private Bitmap BuildSagittal()
        {
            byte[] p = new byte[height * depth];
            for (int z = 0; z < depth; z++)
                for (int y = 0; y < height; y++)
                    p[(depth - 1 - z) * height + y] = volume[z].Gray8[y * width + xIndex];
            Bitmap b = GrayBitmap(p, height, depth);
            DrawCrosshair(b, yIndex, depth - 1 - zIndex, Color.Magenta, Color.Yellow);
            return b;
        }

        private Bitmap BuildMip()
        {
            byte[] p = new byte[width * height];
            for (int z = 0; z < depth; z++)
            {
                byte[] s = volume[z].Gray8;
                for (int i = 0; i < p.Length; i++) if (s[i] > p[i]) p[i] = s[i];
            }
            Bitmap b = GrayBitmap(p, width, height);
            using (Graphics g = Graphics.FromImage(b))
            using (var pen = new Pen(Color.FromArgb(160, Color.DeepSkyBlue), Math.Max(1, width / 256f)))
            {
                int inset = Math.Max(6, width / 20);
                g.DrawRectangle(pen, inset, inset, Math.Max(1, width - inset * 2), Math.Max(1, height - inset * 2));
            }
            return b;
        }

        private static Bitmap GrayBitmap(byte[] pixels, int w, int h)
        {
            var b = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            var data = b.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte[] row = new byte[Math.Abs(data.Stride)];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        byte v = pixels[y * w + x];
                        int i = x * 3;
                        row[i] = row[i + 1] = row[i + 2] = v;
                    }
                    Marshal.Copy(row, 0, IntPtr.Add(data.Scan0, y * data.Stride), row.Length);
                }
            }
            finally { b.UnlockBits(data); }
            return b;
        }

        private static void DrawCrosshair(Bitmap b, int x, int y, Color vertical, Color horizontal)
        {
            using (Graphics g = Graphics.FromImage(b))
            using (var pv = new Pen(vertical, Math.Max(1, b.Width / 512f)))
            using (var ph = new Pen(horizontal, Math.Max(1, b.Width / 512f)))
            {
                g.DrawLine(pv, x, 0, x, b.Height - 1);
                g.DrawLine(ph, 0, y, b.Width - 1, y);
            }
        }

        private static void SetImage(PictureBox box, Image image)
        {
            Image old = box.Image;
            box.Image = image;
            if (old != null) old.Dispose();
        }

        private static int Clamp(int v, int min, int max) { return Math.Max(min, Math.Min(max, v)); }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (axial.Image != null) axial.Image.Dispose();
                if (sagittal.Image != null) sagittal.Image.Dispose();
                if (coronal.Image != null) coronal.Image.Dispose();
                if (volume3D.Image != null) volume3D.Image.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

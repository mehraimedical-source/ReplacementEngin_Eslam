using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DicomViewer_ChatGPT
{
    public partial class MedicalDicomViewerControl : UserControl
    {
   
        private ProcessedDicomImage[] volume;
        private int width, height, depth, xIndex, yIndex, zIndex;
        private double spacingX = 1.0, spacingY = 1.0, spacingZ = 1.0;

        public MedicalDicomViewerControl()
        {
            InitializeComponent();

            axial.MouseEnter += delegate { axial.Focus(); };
            sagittal.MouseEnter += delegate { sagittal.Focus(); };
            coronal.MouseEnter += delegate { coronal.Focus(); };

            axial.MouseWheel += delegate(object s, MouseEventArgs e) { if (depth > 0) { zIndex = Clamp(zIndex + Math.Sign(e.Delta), 0, depth - 1); RefreshViews(); } };
            sagittal.MouseWheel += delegate(object s, MouseEventArgs e) { if (width > 0) { xIndex = Clamp(xIndex + Math.Sign(e.Delta), 0, width - 1); RefreshViews(); } };
            coronal.MouseWheel += delegate(object s, MouseEventArgs e) { if (height > 0) { yIndex = Clamp(yIndex + Math.Sign(e.Delta), 0, height - 1); RefreshViews(); } };
        }

        public void Active(string[] dicomFiles) { Active(DicomSeriesLoader.Load(dicomFiles)); }

        public void Active(ProcessedDicomImage[] images)
        {
            if (images == null || images.Length == 0) throw new ArgumentException("No images supplied.");
            foreach (var image in images) image.Validate();
            width = images[0].Width; height = images[0].Height;
            if (images.Any(x => x.Width != width || x.Height != height)) throw new ArgumentException("All slices must have the same dimensions.");
            volume = images; depth = images.Length;
            CalculateVoxelSpacing();
            xIndex = width / 2; yIndex = height / 2; zIndex = depth / 2;
            RefreshViews();
        }

        public void ActiveFolder(string folder)
        {
            string[] files = DicomSeriesLoader.FindLargestImageSeries(folder);
            if (files.Length == 0) throw new InvalidOperationException("No image DICOM series was found in this folder.");
            Active(files);
        }

        private void RefreshViews()
        {
            SetImage(axial, BuildAxial()); SetImage(sagittal, BuildSagittal());
            SetImage(coronal, BuildCoronal()); SetImage(volume3D, BuildMip());
            status.Text = String.Format("Volume {0} x {1} x {2}    Spacing {3:0.###} x {4:0.###} x {5:0.###} mm    X:{6} Y:{7} Z:{8}", width, height, depth, spacingX, spacingY, spacingZ, xIndex, yIndex, zIndex);
        }

        private Bitmap BuildAxial()
        {
            Bitmap b = GrayBitmap((byte[])volume[zIndex].Gray8.Clone(), width, height);
            DrawCrosshair(b, xIndex, yIndex, Color.Cyan, Color.Magenta); return b;
        }

        private Bitmap BuildCoronal()
        {
            byte[] p = new byte[width * depth];
            for (int z = 0; z < depth; z++) Buffer.BlockCopy(volume[z].Gray8, yIndex * width, p, (depth - 1 - z) * width, width);
            Bitmap b = GrayBitmap(p, width, depth);
            b.SetResolution(96f, (float)(96.0 * spacingZ / spacingX));
            DrawCrosshair(b, xIndex, depth - 1 - zIndex, Color.Cyan, Color.Yellow); return b;
        }

        private Bitmap BuildSagittal()
        {
            byte[] p = new byte[height * depth];
            for (int z = 0; z < depth; z++) for (int y = 0; y < height; y++) p[(depth - 1 - z) * height + y] = volume[z].Gray8[y * width + xIndex];
            Bitmap b = GrayBitmap(p, height, depth);
            b.SetResolution(96f, (float)(96.0 * spacingZ / spacingY));
            DrawCrosshair(b, yIndex, depth - 1 - zIndex, Color.Magenta, Color.Yellow); return b;
        }

        private Bitmap BuildMip()
        {
            byte[] p = new byte[width * height];
            for (int z = 0; z < depth; z++) { byte[] s = volume[z].Gray8; for (int i = 0; i < p.Length; i++) if (s[i] > p[i]) p[i] = s[i]; }
            return GrayBitmap(p, width, height);
        }

        private void CalculateVoxelSpacing()
        {
            if (volume[0].PixelSpacing != null && volume[0].PixelSpacing.Length >= 2)
            {
                // DICOM PixelSpacing is Row spacing, Column spacing.
                spacingY = Math.Abs(volume[0].PixelSpacing[0]);
                spacingX = Math.Abs(volume[0].PixelSpacing[1]);
            }
            if (spacingX <= 0) spacingX = 1.0;
            if (spacingY <= 0) spacingY = 1.0;

            if (depth > 1 && volume[0].ImagePositionPatient != null && volume[0].ImageOrientationPatient != null)
            {
                double[] o = volume[0].ImageOrientationPatient;
                double nx = o[1] * o[5] - o[2] * o[4];
                double ny = o[2] * o[3] - o[0] * o[5];
                double nz = o[0] * o[4] - o[1] * o[3];
                double sum = 0; int count = 0;
                for (int i = 1; i < depth; i++)
                {
                    double[] a = volume[i - 1].ImagePositionPatient;
                    double[] b = volume[i].ImagePositionPatient;
                    if (a == null || b == null) continue;
                    double d = Math.Abs((b[0]-a[0])*nx + (b[1]-a[1])*ny + (b[2]-a[2])*nz);
                    if (d > 0.0001) { sum += d; count++; }
                }
                if (count > 0) spacingZ = sum / count;
            }
            if (spacingZ <= 0 && volume[0].SpacingBetweenSlices > 0) spacingZ = Math.Abs(volume[0].SpacingBetweenSlices);
            if (spacingZ <= 0 && volume[0].SliceThickness > 0) spacingZ = Math.Abs(volume[0].SliceThickness);
            if (spacingZ <= 0) spacingZ = 1.0;
        }

        private static Bitmap GrayBitmap(byte[] pixels, int w, int h)
        {
            var b = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            var data = b.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try {
                byte[] row = new byte[Math.Abs(data.Stride)];
                for (int y = 0; y < h; y++) {
                    Array.Clear(row, 0, row.Length);
                    for (int x = 0; x < w; x++) { byte v = pixels[y*w+x]; int i=x*3; row[i]=row[i+1]=row[i+2]=v; }
                    Marshal.Copy(row, 0, IntPtr.Add(data.Scan0, y*data.Stride), row.Length);
                }
            } finally { b.UnlockBits(data); }
            return b;
        }

        private static void DrawCrosshair(Bitmap b, int x, int y, Color vertical, Color horizontal)
        {
            using (Graphics g=Graphics.FromImage(b))
            using (var pv=new Pen(vertical, Math.Max(1,b.Width/512f)))
            using (var ph=new Pen(horizontal, Math.Max(1,b.Width/512f))) { g.DrawLine(pv,x,0,x,b.Height-1); g.DrawLine(ph,0,y,b.Width-1,y); }
        }

        private static void SetImage(PictureBox box, Image image) { Image old=box.Image; box.Image=image; if(old!=null) old.Dispose(); }
        private static int Clamp(int v,int min,int max) { return Math.Max(min,Math.Min(max,v)); }

        protected override void Dispose(bool disposing)
        {
            if(disposing) { if(axial.Image!=null) axial.Image.Dispose(); if(sagittal.Image!=null) sagittal.Image.Dispose(); if(coronal.Image!=null) coronal.Image.Dispose(); if(volume3D.Image!=null) volume3D.Image.Dispose(); }
            base.Dispose(disposing);
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DicomViewer_ChatGPT
{
    // Dependency-free CPU volume renderer. The input is a contiguous x/y/z volume.
    // Interactive renders use a coarser ray step; final renders use the native minimum spacing.
    internal sealed class CpuVolumeRenderer
    {
        private byte[] voxels;
        private short[] modalityVoxels;
        private bool useHu;
        private int width, height, depth, sliceStride;
        private double sx = 1, sy = 1, sz = 1;
        private double yaw = -0.55, pitch = -0.25, zoom = 1.0;

        public bool Ready { get { return (useHu ? modalityVoxels != null : voxels != null) && width > 0 && height > 0 && depth > 0; } }

        public void SetVolume(byte[] data, int w, int h, int d, double spacingX, double spacingY, double spacingZ)
        {
            voxels = data; modalityVoxels = null; useHu = false;
            SetGeometry(w,h,d,spacingX,spacingY,spacingZ);
        }

        public void SetCtVolume(short[] huData, int w, int h, int d, double spacingX, double spacingY, double spacingZ)
        {
            modalityVoxels = huData; voxels = null; useHu = true;
            SetGeometry(w,h,d,spacingX,spacingY,spacingZ);
        }

        private void SetGeometry(int w,int h,int d,double spacingX,double spacingY,double spacingZ)
        {
            width=w;height=h;depth=d;sliceStride=w*h;
            sx=Math.Max(.0001,spacingX);sy=Math.Max(.0001,spacingY);sz=Math.Max(.0001,spacingZ);
            ResetCamera();
        }

        public void ResetCamera() { yaw = -0.55; pitch = -0.25; zoom = 1.0; }
        public void Rotate(double dx, double dy)
        {
            yaw += dx * .012; pitch += dy * .012;
            pitch = Math.Max(-1.45, Math.Min(1.45, pitch));
        }
        public void Zoom(int wheelDelta)
        {
            zoom *= wheelDelta > 0 ? 1.12 : 1.0 / 1.12;
            zoom = Math.Max(.35, Math.Min(4.0, zoom));
        }

        public Bitmap Render(int targetWidth, int targetHeight, bool interactive)
        {
            if (!Ready) return null;
            int maxSide = interactive ? 300 : 520;
            double aspect = Math.Max(1.0, targetWidth) / Math.Max(1.0, targetHeight);
            int outW, outH;
            if (aspect >= 1) { outW = Math.Min(maxSide, Math.Max(96, targetWidth)); outH = Math.Max(64, (int)Math.Round(outW / aspect)); }
            else { outH = Math.Min(maxSide, Math.Max(96, targetHeight)); outW = Math.Max(64, (int)Math.Round(outH * aspect)); }

            byte[] rgb = new byte[outW * outH * 3];
            double physX = Math.Max(sx, (width - 1) * sx);
            double physY = Math.Max(sy, (height - 1) * sy);
            double physZ = Math.Max(sz, (depth - 1) * sz);
            double diagonal = Math.Sqrt(physX * physX + physY * physY + physZ * physZ);
            double viewSize = Math.Max(physX, Math.Max(physY, physZ)) / zoom;
            double pixel = viewSize / Math.Max(1, Math.Min(outW, outH));
            double step = Math.Min(sx, Math.Min(sy, sz)) * (interactive ? 2.2 : 1.0);

            double cy = Math.Cos(yaw), syaw = Math.Sin(yaw), cp = Math.Cos(pitch), sp = Math.Sin(pitch);
            Vec right = new Vec(cy, 0, -syaw);
            Vec up = new Vec(syaw * sp, cp, cy * sp);
            Vec forward = Normalize(Cross(right, up));
            Vec center = new Vec(physX * .5, physY * .5, physZ * .5);
            double halfW = (outW - 1) * pixel * .5, halfH = (outH - 1) * pixel * .5;

            Parallel.For(0, outH, py =>
            {
                double v = py * pixel - halfH;
                for (int px = 0; px < outW; px++)
                {
                    double u = px * pixel - halfW;
                    Vec basePoint = Add(center, Add(Scale(right, u), Scale(up, v)));
                    Vec start = Add(basePoint, Scale(forward, -diagonal * .55));
                    double ar = 0, ag = 0, ab = 0, alpha = 0;

                    for (double t = 0; t <= diagonal * 1.1 && alpha < .985; t += step)
                    {
                        Vec p = Add(start, Scale(forward, t));
                        double fx = p.X / sx, fy = p.Y / sy, fz = p.Z / sz;
                        double value = Sample(fx, fy, fz);
                        if (useHu) { if (value < 280) continue; }
                        else if (value < 105) continue;

                        double a, r, g, b;
                        if(useHu) BoneTransferHu(value, step, out a, out r, out g, out b);
                        else BoneTransfer((byte)value, out a, out r, out g, out b);
                        if(useHu)
                        {
                            // Add only local surface shading. Geometry, ray traversal and
                            // compositing remain unchanged from the stable baseline.
                            Vec normal=GradientNormal(fx,fy,fz);
                            double diffuse=Math.Max(0,normal.X*(-forward.X)+normal.Y*(-forward.Y)+normal.Z*(-forward.Z));
                            double light=.38+.62*diffuse;
                            r*=light;g*=light;b*=light;
                        }
                        a *= (1.0 - alpha);
                        ar += r * a; ag += g * a; ab += b * a; alpha += a;
                    }

                    int i = (py * outW + px) * 3;
                    rgb[i] = ToByte(ab); rgb[i + 1] = ToByte(ag); rgb[i + 2] = ToByte(ar);
                }
            });
            return BitmapFromBgr(rgb, outW, outH);
        }

        private double Sample(double x, double y, double z)
        {
            if (x < 0 || y < 0 || z < 0 || x > width - 1 || y > height - 1 || z > depth - 1) return useHu ? -1024 : 0;
            int x0=(int)x,y0=(int)y,z0=(int)z;
            int x1=x0<width-1?x0+1:x0,y1=y0<height-1?y0+1:y0,z1=z0<depth-1?z0+1:z0;
            double tx=x-x0,ty=y-y0,tz=z-z0;
            int b0=z0*sliceStride,b1=z1*sliceStride;
            double a,b,c,d;
            if(useHu)
            {
                a=Lerp(modalityVoxels[b0+y0*width+x0],modalityVoxels[b0+y0*width+x1],tx);
                b=Lerp(modalityVoxels[b0+y1*width+x0],modalityVoxels[b0+y1*width+x1],tx);
                c=Lerp(modalityVoxels[b1+y0*width+x0],modalityVoxels[b1+y0*width+x1],tx);
                d=Lerp(modalityVoxels[b1+y1*width+x0],modalityVoxels[b1+y1*width+x1],tx);
            }
            else
            {
                a=Lerp(voxels[b0+y0*width+x0],voxels[b0+y0*width+x1],tx);
                b=Lerp(voxels[b0+y1*width+x0],voxels[b0+y1*width+x1],tx);
                c=Lerp(voxels[b1+y0*width+x0],voxels[b1+y0*width+x1],tx);
                d=Lerp(voxels[b1+y1*width+x0],voxels[b1+y1*width+x1],tx);
            }
            return Lerp(Lerp(a,b,ty),Lerp(c,d,ty),tz);
        }

        private Vec GradientNormal(double x,double y,double z)
        {
            // Central differences in physical space. Only used for CT bone samples
            // that already passed the transfer-function threshold.
            double gx=(Sample(x+1,y,z)-Sample(x-1,y,z))/(2.0*sx);
            double gy=(Sample(x,y+1,z)-Sample(x,y-1,z))/(2.0*sy);
            double gz=(Sample(x,y,z+1)-Sample(x,y,z-1))/(2.0*sz);
            return Normalize(new Vec(gx,gy,gz));
        }

        private static void BoneTransferHu(double hu,double stepMm,out double a,out double r,out double g,out double b)
        {
            // CT bone preset in true modality (HU) space. Keep soft tissue and most
            // partial-volume voxels transparent, then ramp cancellous -> cortical bone.
            if(hu<280){a=r=g=b=0;return;}
            double t=Math.Max(0,Math.Min(1,(hu-280.0)/1200.0));
            double baseAlpha=.012+.34*t*t;
            // Normalize opacity to physical ray length. This keeps the final appearance
            // stable when interactive rendering changes its sampling step.
            a=1.0-Math.Pow(1.0-baseAlpha,Math.Max(.15,stepMm));
            double warm=Math.Max(0,Math.Min(1,(hu-280.0)/700.0));
            r=.90+.10*warm;g=.78+.19*warm;b=.62+.32*warm;
        }

        private static void BoneTransfer(byte v, out double a, out double r, out double g, out double b)
        {
            if (v < 105) { a = r = g = b = 0; return; }
            double t = (v - 105) / 150.0; t = Math.Max(0, Math.Min(1, t));
            // Low-opacity amber transition into dense ivory bone.
            a = .018 + .16 * t * t;
            r = 0.72 + .28 * t; g = 0.48 + .45 * t; b = 0.28 + .62 * t;
        }

        private static Bitmap BitmapFromBgr(byte[] rgb, int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                int rowBytes = w * 3;
                for (int y = 0; y < h; y++) Marshal.Copy(rgb, y * rowBytes, IntPtr.Add(data.Scan0, y * data.Stride), rowBytes);
            }
            finally { bmp.UnlockBits(data); }
            return bmp;
        }

        private static byte ToByte(double v) { return (byte)Math.Max(0, Math.Min(255, Math.Round(v * 255.0))); }
        private static double Lerp(double a, double b, double t) { return a + (b - a) * t; }
        private struct Vec { public double X, Y, Z; public Vec(double x, double y, double z) { X = x; Y = y; Z = z; } }
        private static Vec Add(Vec a, Vec b) { return new Vec(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
        private static Vec Scale(Vec a, double s) { return new Vec(a.X * s, a.Y * s, a.Z * s); }
        private static Vec Cross(Vec a, Vec b) { return new Vec(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X); }
        private static Vec Normalize(Vec a) { double l = Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z); return l < 1e-9 ? new Vec(0, 0, 1) : Scale(a, 1.0 / l); }
    }
}
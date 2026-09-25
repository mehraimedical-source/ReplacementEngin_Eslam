using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DicomViewer_ChatGPT
{
    internal static class DicomSeriesLoader
    {
        private sealed class Item
        {
            public string Path;
            public string SeriesUid;
            public int Instance;
            public double Position;
        }

        public static string[] FindLargestImageSeries(string folder)
        {
            if (String.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                throw new DirectoryNotFoundException(folder);

            var items = new List<Item>();
            foreach (string path in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                try
                {
                    // fo-dicom 4.x does not expose FileReadOption.ReadSmallest.
                    // Open normally here; PixelData is decoded only later when the selected series is rendered.
                    DicomFile file = DicomFile.Open(path);
                    DicomDataset ds = file.Dataset;
                    if (!ds.Contains(DicomTag.PixelData)) continue;

                    string series = ds.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, String.Empty);
                    if (String.IsNullOrEmpty(series)) series = "(no-series)";
                    int instance = ds.GetSingleValueOrDefault(DicomTag.InstanceNumber, 0);
                    double pos = GetSlicePosition(ds, instance);
                    items.Add(new Item { Path = path, SeriesUid = series, Instance = instance, Position = pos });
                }
                catch
                {
                    // A folder may contain reports, thumbnails and arbitrary files. Ignore non-DICOM files.
                }
            }

            var best = items.GroupBy(x => x.SeriesUid).OrderByDescending(g => g.Count()).FirstOrDefault();
            if (best == null) return new string[0];

            return best.OrderBy(x => x.Position).ThenBy(x => x.Instance).Select(x => x.Path).ToArray();
        }

        public static ProcessedDicomImage[] Load(string[] files)
        {
            if (files == null || files.Length == 0) throw new ArgumentException("No DICOM files were supplied.");

            var result = new List<ProcessedDicomImage>(files.Length);
            foreach (string path in files)
            {
                DicomFile file = DicomFile.Open(path);
                DicomDataset ds = file.Dataset;
                var dicomImage = new DicomImage(ds);

                using (var rendered = dicomImage.RenderImage())
                using (var source = rendered.AsClonedBitmap())
                using (var bitmap = To24Bpp(source))
                {
                    result.Add(new ProcessedDicomImage
                    {
                        Width = bitmap.Width,
                        Height = bitmap.Height,
                        Gray8 = ToGray8(bitmap),
                        Modality16 = TryGetModality16(ds, bitmap.Width, bitmap.Height),
                        ImagePositionPatient = GetValues(ds, DicomTag.ImagePositionPatient, 3),
                        ImageOrientationPatient = GetValues(ds, DicomTag.ImageOrientationPatient, 6),
                        PixelSpacing = GetValues(ds, DicomTag.PixelSpacing, 2),
                        SliceThickness = ds.GetSingleValueOrDefault(DicomTag.SliceThickness, 0.0),
                        SpacingBetweenSlices = ds.GetSingleValueOrDefault(DicomTag.SpacingBetweenSlices, 0.0),
                        SopInstanceUid = ds.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, String.Empty),
                        SeriesInstanceUid = ds.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, String.Empty)
                    });
                }
            }
            return result.ToArray();
        }

        private static short[] TryGetModality16(DicomDataset ds, int width, int height)
        {
            try
            {
                var pd=DicomPixelData.Create(ds);
                if(pd.NumberOfFrames!=1)return null;
                IPixelData px=PixelDataFactory.Create(pd,0);
                int count=width*height;
                double slope=ds.GetSingleValueOrDefault(DicomTag.RescaleSlope,1.0);
                double intercept=ds.GetSingleValueOrDefault(DicomTag.RescaleIntercept,0.0);
                short[] dst=new short[count];

                if(px is GrayscalePixelDataU16)
                {
                    ushort[] src=((GrayscalePixelDataU16)px).Data;
                    for(int i=0;i<count;i++)dst[i]=ClampShort(src[i]*slope+intercept);
                    return dst;
                }
                if(px is GrayscalePixelDataS16)
                {
                    short[] src=((GrayscalePixelDataS16)px).Data;
                    for(int i=0;i<count;i++)dst[i]=ClampShort(src[i]*slope+intercept);
                    return dst;
                }
            }
            catch
            {
                // Keep the proven RenderImage/Gray8 path as fallback for unsupported pixel formats/codecs.
            }
            return null;
        }

        private static short ClampShort(double value)
        {
            if(value<short.MinValue)return short.MinValue;
            if(value>short.MaxValue)return short.MaxValue;
            return (short)Math.Round(value);
        }

        private static double GetSlicePosition(DicomDataset ds, int fallback)
        {
            double[] p, o;
            if (ds.TryGetValues(DicomTag.ImagePositionPatient, out p) && p.Length >= 3 &&
                ds.TryGetValues(DicomTag.ImageOrientationPatient, out o) && o.Length >= 6)
            {
                double nx = o[1] * o[5] - o[2] * o[4];
                double ny = o[2] * o[3] - o[0] * o[5];
                double nz = o[0] * o[4] - o[1] * o[3];
                return p[0] * nx + p[1] * ny + p[2] * nz;
            }
            return fallback;
        }

        private static double[] GetValues(DicomDataset ds, DicomTag tag, int expected)
        {
            double[] values;
            if (ds.TryGetValues(tag, out values) && values.Length >= expected) return values;
            return null;
        }

        private static Bitmap To24Bpp(Bitmap source)
        {
            var target = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(target)) g.DrawImageUnscaled(source, 0, 0);
            return target;
        }

        private static byte[] ToGray8(Bitmap bitmap)
        {
            int w = bitmap.Width, h = bitmap.Height;
            byte[] gray = new byte[w * h];
            var rect = new Rectangle(0, 0, w, h);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                int stride = data.Stride;
                byte[] row = new byte[Math.Abs(stride)];
                for (int y = 0; y < h; y++)
                {
                    IntPtr p = IntPtr.Add(data.Scan0, y * stride);
                    Marshal.Copy(p, row, 0, row.Length);
                    int dst = y * w;
                    for (int x = 0; x < w; x++)
                    {
                        int i = x * 3;
                        gray[dst + x] = (byte)((row[i] * 29 + row[i + 1] * 150 + row[i + 2] * 77) >> 8);
                    }
                }
            }
            finally { bitmap.UnlockBits(data); }
            return gray;
        }
    }
}

using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DicomViewer_ChatGPT
{
    internal static class DicomSeriesLoader
    {
        private sealed class Item { public string Path; public string SeriesUid; public int Instance; public double Position; }

        public static string[] FindLargestImageSeries(string folder)
        {
            if (String.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) throw new DirectoryNotFoundException(folder);
            var items = new List<Item>();
            foreach (string path in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                try
                {
                    DicomDataset ds = DicomFile.Open(path).Dataset;
                    if (!ds.Contains(DicomTag.PixelData)) continue;
                    string series = ds.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, String.Empty);
                    if (String.IsNullOrEmpty(series)) series = "(no-series)";
                    int instance = ds.GetSingleValueOrDefault(DicomTag.InstanceNumber, 0);
                    items.Add(new Item { Path=path, SeriesUid=series, Instance=instance, Position=GetSlicePosition(ds,instance) });
                }
                catch { }
            }
            var best=items.GroupBy(x=>x.SeriesUid).OrderByDescending(g=>g.Count()).FirstOrDefault();
            return best==null ? new string[0] : best.OrderBy(x=>x.Position).ThenBy(x=>x.Instance).Select(x=>x.Path).ToArray();
        }

        public static ProcessedDicomImage[] Load(string[] files)
        {
            if (files==null || files.Length==0) throw new ArgumentException("No DICOM files were supplied.");
            var result=new List<ProcessedDicomImage>(files.Length);
            foreach(string path in files)
            {
                DicomDataset ds=DicomFile.Open(path).Dataset;
                DicomPixelData pd=DicomPixelData.Create(ds);
                if (pd.NumberOfFrames < 1) continue;
                IPixelData pixels=PixelDataFactory.Create(pd,0);
                short[] raw=ToSigned16(pixels,pd);
                result.Add(new ProcessedDicomImage {
                    Width=(int)pd.Width, Height=(int)pd.Height, PixelData16=raw,
                    RescaleSlope=ds.GetSingleValueOrDefault(DicomTag.RescaleSlope,1.0),
                    RescaleIntercept=ds.GetSingleValueOrDefault(DicomTag.RescaleIntercept,0.0),
                    WindowCenter=ds.GetSingleValueOrDefault(DicomTag.WindowCenter,40.0),
                    WindowWidth=ds.GetSingleValueOrDefault(DicomTag.WindowWidth,400.0),
                    ImagePositionPatient=GetValues(ds,DicomTag.ImagePositionPatient,3),
                    ImageOrientationPatient=GetValues(ds,DicomTag.ImageOrientationPatient,6),
                    PixelSpacing=GetValues(ds,DicomTag.PixelSpacing,2),
                    SliceThickness=ds.GetSingleValueOrDefault(DicomTag.SliceThickness,0.0),
                    SpacingBetweenSlices=ds.GetSingleValueOrDefault(DicomTag.SpacingBetweenSlices,0.0),
                    SopInstanceUid=ds.GetSingleValueOrDefault(DicomTag.SOPInstanceUID,String.Empty),
                    SeriesInstanceUid=ds.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID,String.Empty)
                });
            }
            return result.ToArray();
        }

        private static short[] ToSigned16(IPixelData pixelData, DicomPixelData pd)
        {
            int count=(int)(pd.Width*pd.Height);
            short[] dst=new short[count];
            if(pixelData is GrayscalePixelDataS16) {
                var src=((GrayscalePixelDataS16)pixelData).Data;
                Array.Copy(src,dst,Math.Min(src.Length,dst.Length)); return dst;
            }
            if(pixelData is GrayscalePixelDataU16) {
                var src=((GrayscalePixelDataU16)pixelData).Data;
                for(int i=0;i<count && i<src.Length;i++) dst[i]=(short)Math.Min(short.MaxValue,src[i]);
                return dst;
            }
            if(pixelData is GrayscalePixelDataU8) {
                var src=((GrayscalePixelDataU8)pixelData).Data;
                for(int i=0;i<count && i<src.Length;i++) dst[i]=src[i];
                return dst;
            }
            throw new NotSupportedException("This MVP currently expects grayscale CT/MR pixel data.");
        }

        private static double GetSlicePosition(DicomDataset ds,int fallback)
        {
            double[] p,o;
            if(ds.TryGetValues(DicomTag.ImagePositionPatient,out p)&&p.Length>=3&&ds.TryGetValues(DicomTag.ImageOrientationPatient,out o)&&o.Length>=6) {
                double nx=o[1]*o[5]-o[2]*o[4], ny=o[2]*o[3]-o[0]*o[5], nz=o[0]*o[4]-o[1]*o[3];
                return p[0]*nx+p[1]*ny+p[2]*nz;
            }
            return fallback;
        }
        private static double[] GetValues(DicomDataset ds,DicomTag tag,int expected) { double[] v; return ds.TryGetValues(tag,out v)&&v.Length>=expected?v:null; }
    }
}

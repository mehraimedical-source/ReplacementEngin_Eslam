using System;

namespace DicomViewer_ChatGPT
{
    public sealed class ProcessedDicomImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int[] StoredPixels { get; set; }
        public byte[] Gray8 { get; set; }
        public double RescaleSlope { get; set; }
        public double RescaleIntercept { get; set; }
        public double WindowCenter { get; set; }
        public double WindowWidth { get; set; }
        public double[] ImagePositionPatient { get; set; }
        public double[] ImageOrientationPatient { get; set; }
        public double[] PixelSpacing { get; set; }
        public double SliceThickness { get; set; }
        public double SpacingBetweenSlices { get; set; }
        public string SopInstanceUid { get; set; }
        public string SeriesInstanceUid { get; set; }

        public void Validate()
        {
            if (Width <= 0 || Height <= 0) throw new ArgumentException("Invalid image dimensions.");
            int n = Width * Height;
            if ((StoredPixels == null || StoredPixels.Length != n) && (Gray8 == null || Gray8.Length != n))
                throw new ArgumentException("A Width*Height pixel buffer is required.");
        }
    }
}

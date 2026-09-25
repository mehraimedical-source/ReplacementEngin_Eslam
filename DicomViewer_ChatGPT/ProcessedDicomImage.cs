using System;

namespace DicomViewer_ChatGPT
{
    public sealed class ProcessedDicomImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Gray8 { get; set; }
        // Optional high precision modality values (HU for CT). Gray8 remains the safe fallback.
        public short[] Modality16 { get; set; }
        public bool HasModality16 { get { return Modality16 != null && Modality16.Length == Width * Height; } }
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
            if (Gray8 == null || Gray8.Length != Width * Height) throw new ArgumentException("Gray8 must contain Width*Height pixels.");
        }
    }
}

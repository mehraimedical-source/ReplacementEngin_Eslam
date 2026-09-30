using System;
using System.Drawing;

namespace Print_SCP.Models
{
    public sealed class RenderedPrintInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobId { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.Now;
        public string CallingAeTitle { get; set; } = "";
        public string CalledAeTitle { get; set; } = "";
        public string RemoteIp { get; set; } = "";
        public string FilmSizeId { get; set; } = "";
        public string Orientation { get; set; } = "";
        public string Layout { get; set; } = "";
        public int ImageCount { get; set; }
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
        public int Dpi { get; set; }
        public string RenderedImagePath { get; set; } = "";
        public string ThumbnailPath { get; set; } = "";
        public string WindowsPrintStatus { get; set; } = "Not requested";
        public string DicomForwardStatus { get; set; } = "Not requested";
        public string ErrorMessage { get; set; } = "";
        public Bitmap Image { get; set; }
        public Bitmap Thumbnail { get; set; }
    }
}
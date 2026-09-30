using System;

namespace Print_SCP.Models
{
    public sealed class PrintJobInfo
    {
        public Guid JobId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CallingAeTitle { get; set; } = "";
        public string CalledAeTitle { get; set; } = "";
        public string RemoteIp { get; set; } = "";
        public string SopClassUid { get; set; } = "";
        public string SopInstanceUid { get; set; } = "";
        public string FilmSizeId { get; set; } = "";
        public string Orientation { get; set; } = "";
        public string Layout { get; set; } = "";
        public int ReceivedImages { get; set; }
        public int TotalImages { get; set; }
        public string Status { get; set; } = "Connected";
        public string Message { get; set; } = "";
    }

    public sealed class PrintLogEntry
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public string RemoteIp { get; set; } = "";
        public string CallingAe { get; set; } = "";
        public string CalledAe { get; set; } = "";
        public string Command { get; set; } = "";
        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
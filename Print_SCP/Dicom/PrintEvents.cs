using System;
using Print_SCP.Models;

namespace Print_SCP.Dicom
{
    internal static class PrintEvents
    {
        public static event Action<PrintLogEntry> Log;
        public static event Action<PrintJobInfo> JobChanged;
        public static event Action<RenderedPrintInfo> PrintCompleted;

        public static void RaiseLog(PrintLogEntry entry) => Log?.Invoke(entry);
        public static void RaiseJob(PrintJobInfo job) => JobChanged?.Invoke(job);
        public static void RaiseCompleted(RenderedPrintInfo item) => PrintCompleted?.Invoke(item);
    }
}
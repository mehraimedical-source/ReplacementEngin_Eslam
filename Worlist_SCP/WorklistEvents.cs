using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worlist_SCP
{
    public static class WorklistEvents
    {
        public static event Func<WorklistRequestEventArgs, Task<List<WorklistItem>>> WorklistRequested;
        public static event Action<WorklistLogEntry> Log;

        internal static async Task<List<WorklistItem>> RequestAsync(WorklistRequestEventArgs args)
        {
            var handler = WorklistRequested;
            if (handler == null) return new List<WorklistItem>();
            return await handler(args).ConfigureAwait(false) ?? new List<WorklistItem>();
        }

        internal static void WriteLog(WorklistLogEntry entry)
        {
            Log?.Invoke(entry);
        }
    }

    public sealed class WorklistLogEntry
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public string AssociationId { get; set; }
        public string RemoteIP { get; set; }
        public string CallingAE { get; set; }
        public string CalledAE { get; set; }
        public string Command { get; set; }
        public string Status { get; set; }
        public string Query { get; set; }
        public int? Results { get; set; }
        public long? DurationMs { get; set; }
        public string Message { get; set; }
    }
}

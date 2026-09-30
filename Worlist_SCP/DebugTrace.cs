using System;
using System.IO;
using System.Text;
using FellowOakDicom;
using FellowOakDicom.Log;

namespace Worlist_SCP
{
    public static class DebugTrace
    {
        private static readonly object Sync = new object();
        private static readonly string SessionFile = CreateSessionFile();

        public static string CurrentFile => SessionFile;

        private static string CreateSessionFile()
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            try
            {
                Directory.CreateDirectory(folder);
            }
            catch
            {
                folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Worlist_SCP", "Logs");
                Directory.CreateDirectory(folder);
            }

            string path = Path.Combine(folder,
                "WorklistDebug_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");

            var sb = new StringBuilder();
            sb.AppendLine("============================================================");
            sb.AppendLine(" DICOM MODALITY WORKLIST SCP - DEBUG SESSION");
            sb.AppendLine("============================================================");
            sb.AppendLine("Started       : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.AppendLine("Machine       : " + Environment.MachineName);
            sb.AppendLine("OS            : " + Environment.OSVersion);
            sb.AppendLine(".NET Runtime  : " + Environment.Version);
            sb.AppendLine("64-bit OS     : " + Environment.Is64BitOperatingSystem);
            sb.AppendLine("64-bit Process: " + Environment.Is64BitProcess);
            sb.AppendLine("App Folder    : " + AppDomain.CurrentDomain.BaseDirectory);
            sb.AppendLine();

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            return path;
        }

        public static void Write(WorklistLogEntry entry)
        {
            if (entry == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("------------------------------------------------------------");
            sb.AppendLine("TIME        : " + entry.Time.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.AppendLine("ASSOCIATION : " + Safe(entry.AssociationId));
            sb.AppendLine("REMOTE IP   : " + Safe(entry.RemoteIP));
            sb.AppendLine("CALLING AE  : " + Safe(entry.CallingAE));
            sb.AppendLine("CALLED AE   : " + Safe(entry.CalledAE));
            sb.AppendLine("COMMAND     : " + Safe(entry.Command));
            sb.AppendLine("STATUS      : " + Safe(entry.Status));
            if (!string.IsNullOrWhiteSpace(entry.Query))
                sb.AppendLine("QUERY       : " + entry.Query);
            if (entry.Results.HasValue)
                sb.AppendLine("RESULTS     : " + entry.Results.Value);
            if (entry.DurationMs.HasValue)
                sb.AppendLine("DURATION    : " + entry.DurationMs.Value + " ms");
            if (!string.IsNullOrWhiteSpace(entry.Message))
                sb.AppendLine("MESSAGE     : " + entry.Message);

            Append(sb.ToString());
        }

        public static void WriteSection(string title, string text)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("==================== " + title + " ====================");
            sb.AppendLine(text ?? string.Empty);
            sb.AppendLine("============================================================");
            Append(sb.ToString());
        }

        public static void WriteDataset(string title, DicomDataset dataset)
        {
            if (dataset == null)
            {
                WriteSection(title, "<null dataset>");
                return;
            }

            try
            {
                var sb = new StringBuilder();
                var dumper = new DicomDatasetDumper(sb, 160, 110);
                new DicomDatasetWalker(dataset).Walk(dumper);
                WriteSection(title, sb.ToString());
            }
            catch (Exception ex)
            {
                WriteSection(title + " [DUMP ERROR]", ex.ToString());
            }
        }

        public static void ExportTo(string destination)
        {
            lock (Sync)
            {
                File.Copy(SessionFile, destination, true);
            }
        }

        private static string Safe(string value)
        {
            return string.IsNullOrEmpty(value) ? "<empty>" : value;
        }

        private static void Append(string text)
        {
            try
            {
                lock (Sync)
                {
                    File.AppendAllText(SessionFile, text, Encoding.UTF8);
                }
            }
            catch
            {
                // Debug logging must never interrupt DICOM networking.
            }
        }
    }
}

using System;
using System.IO;
using Print_SCP.Data;

namespace Print_SCP.Services
{
    internal sealed class PrintRuntimeSettings
    {
        public int RenderDpi { get; set; } = 300;
        public string OutputRoot { get; set; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prints");

        public bool WindowsPrintEnabled { get; set; }
        public string WindowsPrinterName { get; set; } = "";

        public bool DicomForwardEnabled { get; set; }
        public string DicomRemoteHost { get; set; } = "";
        public int DicomRemotePort { get; set; } = 104;
        public string DicomCallingAe { get; set; } = "PRINT_GATEWAY";
        public string DicomCalledAe { get; set; } = "";
    }

    internal static class PrintRuntime
    {
        private static readonly object Sync = new object();
        private static PrintRuntimeSettings _settings = new PrintRuntimeSettings();

        public static PrintArchiveRepository Archive { get; } =
            new PrintArchiveRepository(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "DicomPrint.db"));

        public static PrintRuntimeSettings Settings
        {
            get
            {
                lock (Sync)
                {
                    return new PrintRuntimeSettings
                    {
                        RenderDpi = _settings.RenderDpi,
                        OutputRoot = _settings.OutputRoot,
                        WindowsPrintEnabled = _settings.WindowsPrintEnabled,
                        WindowsPrinterName = _settings.WindowsPrinterName,
                        DicomForwardEnabled = _settings.DicomForwardEnabled,
                        DicomRemoteHost = _settings.DicomRemoteHost,
                        DicomRemotePort = _settings.DicomRemotePort,
                        DicomCallingAe = _settings.DicomCallingAe,
                        DicomCalledAe = _settings.DicomCalledAe
                    };
                }
            }
            set
            {
                lock (Sync)
                    _settings = value ?? new PrintRuntimeSettings();
            }
        }
    }
}
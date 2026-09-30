using System;
using FellowOakDicom.Network;

namespace Print_SCP.Dicom
{
    internal sealed class PrintServerController : IDisposable
    {
        private IDicomServer _server;

        public bool IsRunning => _server != null;

        public void Start(int port)
        {
            if (_server != null) return;
            _server = DicomServerFactory.Create<DicomPrintScp>(port);
        }

        public void Stop()
        {
            if (_server == null) return;

            try { _server.Stop(); }
            finally
            {
                _server.Dispose();
                _server = null;
            }
        }

        public void Dispose() => Stop();
    }
}
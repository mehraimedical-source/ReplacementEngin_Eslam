using System;
using System.Collections.Generic;
using FellowOakDicom;

namespace Worlist_SCP
{
    public sealed class WorklistRequestEventArgs : EventArgs
    {
        public string RemoteIP { get; internal set; }
        public string CallingAETitle { get; internal set; }
        public string CalledAETitle { get; internal set; }
        public DicomDataset Query { get; internal set; }

        public string PatientID { get; internal set; }
        public string PatientName { get; internal set; }
        public string AccessionNumber { get; internal set; }
        public string Modality { get; internal set; }
        public string ScheduledStationAETitle { get; internal set; }

        public List<WorklistItem> Results { get; set; } = new List<WorklistItem>();
    }
}

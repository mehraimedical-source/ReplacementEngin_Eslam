using System;

namespace Worlist_SCP
{
    public class WorklistItem
    {
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Sex { get; set; }
        public string AccessionNumber { get; set; }
        public string Modality { get; set; }
        public string ScheduledStationAETitle { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public string ProcedureDescription { get; set; }
        public string RequestedProcedureID { get; set; }
        public string ScheduledProcedureStepID { get; set; }
        public string ReferringPhysicianName { get; set; }
    }
}

using System;
using System.Drawing;

namespace MEHR_PACSViewer
{
    internal class PacsResult : ResultInfo
    {
        public PacsInfo[] PacsInfos { get; set; }

        internal class PacsInfo
        {
            public PacsInfo()
            {
                serverName=""; nationalID=""; studyTime=""; institutionName=""; modality=""; bodyParts="";
                StudyInstanceUIDS=""; studyDescriptions=""; patientHistory=""; comment=""; studyID="";
                patientName=""; patientID=""; patientOtherName=""; PatientTelephoneNumbers=""; PatientSex="";
                patientAge=""; admissionType=""; patientSize=""; PatientWeight=""; PatientServices="";
                AnswerStatus=""; AssignToUserFullName=""; reporterName=""; DoctorName=""; Diagnosis="";
                RobatFolder=""; userName=""; regTime=""; lastChange=""; View_Base="seri"; SeenTime="";
                ReportedTime=""; Label=""; LabelComment=""; WriteDVDRegtime="";
            }

            public ulong PID { get; set; }
            public ulong pacsID { get; set; }
            public int serverID { get; set; }
            public string serverName { get; set; }
            public string nationalID { get; set; }
            public int studyDate { get; set; }
            public string studyDateNew
            {
                get
                {
                    string s=studyDate.ToString();
                    return s.Length>=8?s.Substring(0,4)+"-"+s.Substring(4,2)+"-"+s.Substring(6,2):"";
                }
            }
            public string studyTime { get; set; }
            public int institutionID { get; set; }
            public string institutionName { get; set; }
            public int modalityID { get; set; }
            public string modality { get; set; }
            public string bodyParts { get; set; }
            public string Title
            {
                get
                {
                    if((bodyParts.Length<2||bodyParts.StartsWith("/"))&&!String.IsNullOrEmpty(studyDescriptions))
                        return studyDescriptions.Replace("\r\n",",");
                    return bodyParts;
                }
            }
            public string StudyInstanceUIDS { get; set; }
            public string studyDescriptions { get; set; }
            public string patientHistory { get; set; }
            public string comment { get; set; }
            public string studyID { get; set; }
            public string patientName { get; set; }
            public string patientID { get; set; }
            public string patientOtherName { get; set; }
            public string PatientFarsiName
            {
                get
                {
                    string n="";
                    if(!String.IsNullOrEmpty(patientOtherName))
                    {
                        if(PatientSex=="F")n="خانم ";
                        else if(PatientSex=="M")n="آقای ";
                        n+=patientOtherName;
                    }
                    return n;
                }
            }
            public string PatientName_ForSMS { get { return String.IsNullOrEmpty(PatientFarsiName)?patientName:PatientFarsiName; } }
            public string PatientTelephoneNumbers { get; set; }
            public string PatientSex { get; set; }
            public string patientAge { get; set; }
            public string admissionType { get; set; }
            public string patientSize { get; set; }
            public string PatientWeight { get; set; }
            public int MarkedCount { get; set; }
            public string PatientServices { get; set; }
            public int AnswerStatusID { get; set; }
            public string AnswerStatus { get; set; }
            public string AssignToUserFullName { get; set; }
            public string reporterName { get; set; }
            public string DoctorName { get; set; }
            public bool recheckReport { get; set; }
            public string Diagnosis { get; set; }
            public int studyCount { get; set; }
            public int seriesCount { get; set; }
            public int ImageCount { get; set; }
            public string RobatFolder { get; set; }
            public int serverStudyCount { get; set; }
            public int serverSeriesCount { get; set; }
            public int serverImageCount { get; set; }
            public int userID { get; set; }
            public string userName { get; set; }
            public string regTime { get; set; }
            public string lastChange { get; set; }
            public int filesSize { get; set; }
            public string View_Base { get; set; }
            public bool Seen { get; set; }
            public string SeenTime { get; set; }
            public bool Reported { get; set; }
            public string ReportedTime { get; set; }
            public int VoiceCount { get; set; }
            public bool pined { get; set; }
            public Image pinedImage { get; set; }
            public int LabelID { get; set; }
            public string Label { get; set; }
            public Image LabelImage { get; set; }
            public string LabelComment { get; set; }
            public string Folder
            {
                get
                {
                    if(serverID>0&&pacsID>0&&studyDate>0)
                        return PacsCommand.GetPatientFolder(serverID,pacsID,studyDate,patientID,patientName);
                    return "";
                }
            }
            public string WriteDVDRegtime { get; set; }
        }
    }
}
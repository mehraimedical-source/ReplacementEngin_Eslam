using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MEHR_PACSViewer
{
    internal enum VOICERECORDERSTATUS { Stop=0, Recordording=1, Pause=2 }

    internal partial class uc_VoiceRecorder : UserControl
    {
        private readonly WaveInRecorder _recorder=new WaveInRecorder();
        private PacsResult.PacsInfo _pacsInfo;
        private string _tempFile;
        public VOICERECORDERSTATUS VoiceRecorderStatus { get; private set; }

        public uc_VoiceRecorder()
        {
            InitializeComponent();
            SetStopped();
        }

        public void Active(PacsResult.PacsInfo pacsInfo){_pacsInfo=pacsInfo;CalculateSumVoice();}
        public void SaveVoice(){if(VoiceRecorderStatus!=VOICERECORDERSTATUS.Stop)SaveCurrent();}

        private void picMicrophone_Click(object sender,EventArgs e)
        {
            if(VoiceRecorderStatus!=VOICERECORDERSTATUS.Stop)return;
            try
            {
                _tempFile=Path.Combine(Path.GetTempPath(),"MEHRVoice_"+Guid.NewGuid().ToString("N")+".wav");
                _recorder.Start(_tempFile);
                VoiceRecorderStatus=VOICERECORDERSTATUS.Recordording;
                picMicrophone.Image=Properties.Resources.RedMicrophone;
                ShowRecording(true);
                tmrVoiceTiming.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"Voice Recorder",MessageBoxButtons.OK,MessageBoxIcon.Error);
                SetStopped();
            }
        }

        private void lbPause_Click(object sender,EventArgs e)
        {
            if(VoiceRecorderStatus==VOICERECORDERSTATUS.Recordording)
            {
                _recorder.Pause();VoiceRecorderStatus=VOICERECORDERSTATUS.Pause;
                picMicrophone.Image=Properties.Resources.LightBlue;lbPause.Text=">";
            }
            else if(VoiceRecorderStatus==VOICERECORDERSTATUS.Pause)
            {
                _recorder.Resume();VoiceRecorderStatus=VOICERECORDERSTATUS.Recordording;
                picMicrophone.Image=Properties.Resources.RedMicrophone;lbPause.Text="II";
            }
        }

        private void picOK_Click(object sender,EventArgs e){SaveCurrent();}
        private void picCancel_Click(object sender,EventArgs e)
        {
            tmrVoiceTiming.Stop();_recorder.Stop(false);_tempFile=null;SetStopped();
        }

        private void SaveCurrent()
        {
            if(VoiceRecorderStatus==VOICERECORDERSTATUS.Stop)return;
            int seconds=Math.Max(1,_recorder.RecordedSeconds);
            tmrVoiceTiming.Stop();_recorder.Stop(true);
            if(_pacsInfo==null)
            {
                MessageBox.Show("PacsInfo is not active. WAV kept at:\r\n"+_tempFile);
                SetStopped();return;
            }
            VoiceResult.VoiceInfo v=new VoiceResult.VoiceInfo();
            v.serverID=_pacsInfo.serverID;v.PID=_pacsInfo.PID;v.userID=1;v.VoiceSeconds=seconds;
            VoiceResult.VoiceInfo saved=VoiceCommand.Add(v,0);
            if(saved!=null&&saved.voiceID>0)
            {
                string dir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data\\server"+_pacsInfo.serverID+"\\ClientVoice");
                Directory.CreateDirectory(dir);
                string dst=Path.Combine(dir,saved.voiceID+".wav");
                if(File.Exists(dst))File.Delete(dst);
                File.Move(_tempFile,dst);_tempFile=null;
            }
            else MessageBox.Show("VoiceCommand.Add is a test stub. WAV kept at:\r\n"+_tempFile);
            CalculateSumVoice();SetStopped();
        }

        private void tmrVoiceTiming_Tick(object sender,EventArgs e)
        {
            levelMeter.Value=VoiceRecorderStatus==VOICERECORDERSTATUS.Recordording?_recorder.Level:0;
            int s=_recorder.RecordedSeconds;
            lbTime.Text=(s/60).ToString("00")+":"+(s%60).ToString("00");
        }

        private void CalculateSumVoice(){if(_pacsInfo!=null)VoiceCommand.Count(_pacsInfo.PID);}
        private void ShowRecording(bool visible)
        {
            levelMeter.Visible=visible;lbPause.Visible=visible;lbTime.Visible=visible;
            picOK.Visible=visible;picCancel.Visible=visible;
        }
        private void SetStopped()
        {
            VoiceRecorderStatus=VOICERECORDERSTATUS.Stop;
            picMicrophone.Image=Properties.Resources.Blue;levelMeter.Value=0;
            ShowRecording(false);lbPause.Text="II";lbTime.Text="00:00";
        }
    }
}
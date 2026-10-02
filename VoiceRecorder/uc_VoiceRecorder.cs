using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MEHR_PACSViewer
{
    internal enum VOICERECORDERSTATUS { Stop=0, Recordording=1, Pause=2 }

    internal sealed class uc_VoiceRecorder : UserControl
    {
        private readonly WaveInRecorder _recorder=new WaveInRecorder();
        private readonly Timer _timer=new Timer();
        private readonly Label _mic=new Label(),_pause=new Label(),_time=new Label(),_ok=new Label(),_cancel=new Label();
        private readonly VoiceLevelMeter _meter=new VoiceLevelMeter();
        private PacsResult.PacsInfo _pacsInfo;
        private string _tempFile;
        public VOICERECORDERSTATUS VoiceRecorderStatus { get; private set; }

        public uc_VoiceRecorder()
        {
            BackColor=Color.Black; Size=new Size(144,49);
            Setup(_mic,"●",Color.DodgerBlue,2,8,24,34,20);
            _meter.SetBounds(27,6,8,37);
            Setup(_pause,"II",Color.White,39,2,18,18,9);
            Setup(_time,"00:00",Color.White,57,15,43,20,9);
            Setup(_ok,"✓",Color.LimeGreen,101,8,22,30,17);
            Setup(_cancel,"×",Color.OrangeRed,123,6,20,32,20);
            Controls.Add(_mic);Controls.Add(_meter);Controls.Add(_pause);Controls.Add(_time);Controls.Add(_ok);Controls.Add(_cancel);
            _mic.Click+=StartClick;_pause.Click+=PauseClick;_ok.Click+=OkClick;_cancel.Click+=CancelClick;
            _timer.Interval=80;_timer.Tick+=TimerTick;SetStopped();
        }

        private static void Setup(Label c,string text,Color color,int x,int y,int w,int h,float font)
        { c.Text=text;c.ForeColor=color;c.Font=new Font("Arial",font,FontStyle.Bold);c.SetBounds(x,y,w,h);c.TextAlign=ContentAlignment.MiddleCenter;c.Cursor=Cursors.Hand; }

        public void Active(PacsResult.PacsInfo pacsInfo){_pacsInfo=pacsInfo;CalculateSumVoice();}
        public void SaveVoice(){if(VoiceRecorderStatus!=VOICERECORDERSTATUS.Stop)SaveCurrent();}

        private void StartClick(object sender,EventArgs e)
        {
            if(VoiceRecorderStatus!=VOICERECORDERSTATUS.Stop)return;
            try{
                _tempFile=Path.Combine(Path.GetTempPath(),"MEHRVoice_"+Guid.NewGuid().ToString("N")+".wav");
                _recorder.Start(_tempFile);VoiceRecorderStatus=VOICERECORDERSTATUS.Recordording;
                _mic.ForeColor=Color.Red;ShowRecording(true);_timer.Start();
            }catch(Exception ex){MessageBox.Show(ex.Message,"Voice Recorder",MessageBoxButtons.OK,MessageBoxIcon.Error);SetStopped();}
        }

        private void PauseClick(object sender,EventArgs e)
        {
            if(VoiceRecorderStatus==VOICERECORDERSTATUS.Recordording){_recorder.Pause();VoiceRecorderStatus=VOICERECORDERSTATUS.Pause;_mic.ForeColor=Color.LightSkyBlue;_pause.Text=">";}
            else if(VoiceRecorderStatus==VOICERECORDERSTATUS.Pause){_recorder.Resume();VoiceRecorderStatus=VOICERECORDERSTATUS.Recordording;_mic.ForeColor=Color.Red;_pause.Text="II";}
        }

        private void OkClick(object sender,EventArgs e){SaveCurrent();}
        private void CancelClick(object sender,EventArgs e){_timer.Stop();_recorder.Stop(false);_tempFile=null;SetStopped();}

        private void SaveCurrent()
        {
            if(VoiceRecorderStatus==VOICERECORDERSTATUS.Stop)return;
            int seconds=Math.Max(1,_recorder.RecordedSeconds);_timer.Stop();_recorder.Stop(true);
            if(_pacsInfo==null){MessageBox.Show("PacsInfo is not active. WAV kept at:\r\n"+_tempFile);SetStopped();return;}
            VoiceResult.VoiceInfo v=new VoiceResult.VoiceInfo();
            v.serverID=_pacsInfo.serverID;v.PID=_pacsInfo.PID;v.userID=1;v.VoiceSeconds=seconds;
            VoiceResult.VoiceInfo saved=VoiceCommand.Add(v,0);
            if(saved!=null&&saved.voiceID>0)
            {
                string dir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data\\server"+_pacsInfo.serverID+"\\ClientVoice");
                Directory.CreateDirectory(dir);string dst=Path.Combine(dir,saved.voiceID+".wav");
                if(File.Exists(dst))File.Delete(dst);File.Move(_tempFile,dst);_tempFile=null;
            }
            else MessageBox.Show("VoiceCommand.Add is a test stub. WAV kept at:\r\n"+_tempFile);
            CalculateSumVoice();SetStopped();
        }

        private void TimerTick(object sender,EventArgs e)
        {
            _meter.Value=VoiceRecorderStatus==VOICERECORDERSTATUS.Recordording?_recorder.Level:0;
            int s=_recorder.RecordedSeconds;_time.Text=(s/60).ToString("00")+":"+(s%60).ToString("00");
        }
        private void CalculateSumVoice(){if(_pacsInfo!=null)VoiceCommand.Count(_pacsInfo.PID);}
        private void ShowRecording(bool visible){_meter.Visible=visible;_pause.Visible=visible;_time.Visible=visible;_ok.Visible=visible;_cancel.Visible=visible;}
        private void SetStopped(){VoiceRecorderStatus=VOICERECORDERSTATUS.Stop;_mic.ForeColor=Color.DodgerBlue;_meter.Value=0;ShowRecording(false);_pause.Text="II";_time.Text="00:00";}
        protected override void Dispose(bool disposing){if(disposing){_timer.Stop();_timer.Dispose();_recorder.Dispose();}base.Dispose(disposing);}
    }
}

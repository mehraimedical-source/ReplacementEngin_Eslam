using System.Drawing;
using System.Windows.Forms;
namespace MEHR_PACSViewer
{
    internal sealed class VoiceRecorderTestForm : Form
    {
        public VoiceRecorderTestForm()
        {
            Text="MEHR Voice Recorder Test"; ClientSize=new Size(360,145);
            StartPosition=FormStartPosition.CenterScreen; BackColor=Color.FromArgb(35,35,35);
            uc_VoiceRecorder r=new uc_VoiceRecorder(); r.Location=new Point(105,25); Controls.Add(r);
            PacsResult.PacsInfo p=new PacsResult.PacsInfo();
            p.PID=1001; p.pacsID=1; p.serverID=1; p.studyDate=20260101; p.patientID="TEST"; p.patientName="Voice Test";
            r.Active(p);
            Label l=new Label(); l.AutoSize=true; l.ForeColor=Color.White; l.Location=new Point(42,90);
            l.Text="Blue mic: Start   Red: Recording   II: Pause   OK: Save   X: Cancel"; Controls.Add(l);
        }
    }
}
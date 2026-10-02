using System.Drawing;
using System.Windows.Forms;

namespace MEHR_PACSViewer
{
    internal sealed class VoiceRecorderTestForm : Form
    {
        public VoiceRecorderTestForm()
        {
            Text="Voice Recorder Test"; ClientSize=new Size(330,130); StartPosition=FormStartPosition.CenterScreen;
            BackColor=Color.FromArgb(35,35,35); Font=new Font("Tahoma",8.25F);
            uc_VoiceRecorder recorder=new uc_VoiceRecorder(); recorder.Location=new Point(90,25); Controls.Add(recorder);
            PacsResult.PacsInfo p=new PacsResult.PacsInfo();
            p.PID=1001; p.pacsID=1; p.serverID=1; p.studyDate=20260101; p.patientID="TEST"; p.patientName="Voice Test";
            recorder.Active(p);
            Label help=new Label(); help.AutoSize=true; help.ForeColor=Color.White; help.Location=new Point(38,86);
            help.Text="Blue mic = Start   Red mic = Recording   OK = Save   X = Cancel"; Controls.Add(help);
        }
    }
}

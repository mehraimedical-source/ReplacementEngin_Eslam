using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ReplacementEngin_Eslam.WinForms
{
    public sealed class MainForm : Form
    {
        private readonly ReplacementEngine _engine = new ReplacementEngine();
        private string _taskId;

        private readonly TextBox _patient = new TextBox();
        private readonly TextBox _task = new TextBox();
        private readonly TextBox _normal = MultiLine();
        private readonly TextBox _voice = MultiLine();
        private readonly TextBox _ocr = MultiLine();
        private readonly TextBox _imageId = new TextBox();
        private readonly TextBox _report = MultiLine();
        private readonly TextBox _diagnostics = MultiLine();
        private readonly Label _status = new Label();

        public MainForm()
        {
            Text = "Replacement Engine - Real Time Test";
            Width = 1180; Height = 820; StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Tahoma", 9F);
            BuildUi();
            _engine.ResultReady += Engine_ResultReady;
        }

        private static TextBox MultiLine()
        {
            return new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, AcceptsReturn = true };
        }

        private void BuildUi()
        {
            var root = new TableLayoutPanel { Dock=DockStyle.Fill, ColumnCount=2, RowCount=6, Padding=new Padding(10) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute,65));
            root.RowStyles.Add(new RowStyle(SizeType.Percent,28));
            root.RowStyles.Add(new RowStyle(SizeType.Percent,28));
            root.RowStyles.Add(new RowStyle(SizeType.Percent,30));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute,50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent,14));
            Controls.Add(root);

            var top = new FlowLayoutPanel { Dock=DockStyle.Fill, FlowDirection=FlowDirection.LeftToRight, WrapContents=false };
            top.Controls.Add(new Label { Text="Patient ID", AutoSize=true, Padding=new Padding(0,7,0,0) });
            _patient.Width=120; top.Controls.Add(_patient);
            var create=Button("Create Task", CreateTask); top.Controls.Add(create);
            top.Controls.Add(new Label { Text="Task ID", AutoSize=true, Padding=new Padding(12,7,0,0) });
            _task.Width=300; _task.ReadOnly=true; top.Controls.Add(_task);
            _status.AutoSize=true; _status.Padding=new Padding(12,7,0,0); _status.Text="No task";
            top.Controls.Add(_status);
            root.Controls.Add(top,0,0); root.SetColumnSpan(top,2);

            root.Controls.Add(Group("Normal / Template Text", _normal, Button("Send Normal", SendNormal)),0,1);
            root.Controls.Add(Group("Voice Text", _voice, Button("Send Voice", SendVoice)),1,1);

            var ocrPanel = new Panel { Dock=DockStyle.Fill };
            var ocrTop = new FlowLayoutPanel { Dock=DockStyle.Top, Height=35 };
            ocrTop.Controls.Add(new Label { Text="Image ID", AutoSize=true, Padding=new Padding(0,7,0,0) });
            _imageId.Width=140; ocrTop.Controls.Add(_imageId); ocrTop.Controls.Add(Button("Send OCR", SendOcr));
            _ocr.Dock=DockStyle.Fill; ocrPanel.Controls.Add(_ocr); ocrPanel.Controls.Add(ocrTop);
            root.Controls.Add(Group("OCR Text", ocrPanel, null),0,2);

            _diagnostics.ReadOnly=true; root.Controls.Add(Group("Diagnostics", _diagnostics, null),1,2);
            _report.ReadOnly=true; _report.RightToLeft=RightToLeft.Yes;
            root.Controls.Add(Group("Current / Final Report", _report, null),0,3); root.SetColumnSpan(root.GetControlFromPosition(0,3),2);

            var actions=new FlowLayoutPanel { Dock=DockStyle.Fill };
            actions.Controls.Add(Button("Get Current Result", GetCurrent));
            actions.Controls.Add(Button("Finish Task", FinishTask));
            actions.Controls.Add(Button("New Task", ResetTask));
            root.Controls.Add(actions,0,4); root.SetColumnSpan(actions,2);

            var help=new Label { Dock=DockStyle.Fill, AutoSize=false,
                Text="Order is intentionally unrestricted after Create Task: Normal, Voice and OCR may be sent repeatedly in any order. Finish Task explicitly requests the final result." };
            root.Controls.Add(help,0,5); root.SetColumnSpan(help,2);
        }

        private static Control Group(string title, Control content, Button action)
        {
            var box=new GroupBox { Text=title, Dock=DockStyle.Fill };
            if(action==null){ content.Dock=DockStyle.Fill; box.Controls.Add(content); return box; }
            var p=new Panel { Dock=DockStyle.Fill }; action.Dock=DockStyle.Bottom; action.Height=30; content.Dock=DockStyle.Fill;
            p.Controls.Add(content); p.Controls.Add(action); box.Controls.Add(p); return box;
        }

        private static Button Button(string text, EventHandler click)
        {
            var b=new Button { Text=text, AutoSize=true }; b.Click+=click; return b;
        }

        private bool HasTask()
        {
            if(!String.IsNullOrEmpty(_taskId)) return true;
            MessageBox.Show("Create a task first.", "Replacement Engine", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private void CreateTask(object sender, EventArgs e)
        {
            try {
                _taskId=_engine.CreateTask(_patient.Text);
                _task.Text=_taskId; _status.Text="Active"; _report.Clear(); _diagnostics.Clear();
            } catch(Exception ex){ Error(ex); }
        }

        private void SendNormal(object sender, EventArgs e) { if(HasTask()) Safe(()=>_engine.SendNormalText(_taskId,_normal.Text)); }
        private void SendVoice(object sender, EventArgs e) { if(HasTask()) Safe(()=>_engine.SendVoiceText(_taskId,_voice.Text)); }
        private void SendOcr(object sender, EventArgs e) { if(HasTask()) Safe(()=>_engine.SendOcrText(_taskId,_imageId.Text,_ocr.Text)); }
        private void GetCurrent(object sender, EventArgs e) { if(HasTask()) Safe(()=>_engine.GetCurrentResult(_taskId)); }
        private void FinishTask(object sender, EventArgs e)
        {
            if(!HasTask()) return;
            Safe(()=>_engine.FinishTask(_taskId));
        }
        private void ResetTask(object sender, EventArgs e)
        {
            _taskId=null; _task.Clear(); _patient.Clear(); _normal.Clear(); _voice.Clear(); _ocr.Clear();
            _imageId.Clear(); _report.Clear(); _diagnostics.Clear(); _status.Text="No task";
        }

        private void Engine_ResultReady(object sender, ReplacementResultEventArgs e)
        {
            if(InvokeRequired){ BeginInvoke(new Action(()=>ShowResult(e.Result))); return; }
            ShowResult(e.Result);
        }

        private void ShowResult(ReplacementResult r)
        {
            _report.Text=r.ReportText ?? String.Empty;
            var sb=new StringBuilder();
            sb.AppendLine("PatientId: "+r.PatientId);
            sb.AppendLine("TaskId: "+r.TaskId);
            sb.AppendLine("Type: "+r.Type);
            sb.AppendLine("HasUnresolvedItems: "+r.HasUnresolvedItems);
            sb.AppendLine();
            sb.AppendLine("Confirmed:");
            foreach(var p in r.ConfirmedParameters)
                sb.AppendLine(String.Format("{0} = {1} {2} {3} [{4}]",p.Name,p.Value,p.Unit,p.GestationalAge,p.Source));
            sb.AppendLine();
            sb.AppendLine("Diagnostics:");
            foreach(var d in r.Diagnostics) sb.AppendLine(d);
            _diagnostics.Text=sb.ToString();
            _status.Text=r.Type==ResultType.Final ? "Completed" : "Active / Intermediate";
        }

        private void Safe(Func<ReplacementResult> action)
        {
            try { ShowResult(action()); } catch(Exception ex) { Error(ex); }
        }
        private static void Error(Exception ex)
        {
            MessageBox.Show(ex.Message,"Replacement Engine",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }
    }
}
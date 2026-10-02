using System;
using System.Drawing;
using System.Windows.Forms;

namespace MEHR_PACSViewer
{
    internal sealed class VoiceLevelMeter : Control
    {
        private int _value;
        public int Value
        {
            get { return _value; }
            set { int v=value; if(v<0)v=0; if(v>100)v=100; if(_value!=v){_value=v;Invalidate();} }
        }
        public VoiceLevelMeter()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            BackColor=Color.Black; Size=new Size(8,38);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int segments=12,gap=1,h=Math.Max(1,(Height-11*gap)/segments);
            int active=(int)Math.Ceiling(_value*segments/100.0);
            for(int i=0;i<segments;i++)
            {
                int y=Height-(i+1)*h-i*gap;
                Color c=i>=9?Color.Red:(i>=7?Color.Yellow:(i>=3?Color.LimeGreen:Color.DeepSkyBlue));
                using(Brush b=new SolidBrush(i<active?c:Color.FromArgb(35,35,35)))
                    e.Graphics.FillRectangle(b,0,y,Width,h);
            }
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DicomViewer_ChatGPT
{
    public partial class MedicalDicomViewerControl : UserControl
    {
        private ProcessedDicomImage[] volume;
        private int width,height,depth,xIndex,yIndex,zIndex;
        private double spacingX=1,spacingY=1,spacingZ=1,windowCenter=40,windowWidth=400;
        private double axialAngle=0,coronalAngle=0,sagittalAngle=0;
        private PictureBox rotatingView;

        public MedicalDicomViewerControl()
        {
            InitializeComponent();
            axial.MouseEnter += delegate { axial.Focus(); }; sagittal.MouseEnter += delegate { sagittal.Focus(); }; coronal.MouseEnter += delegate { coronal.Focus(); };
            axial.MouseWheel += delegate(object s,MouseEventArgs e){if(depth>0){zIndex=Clamp(zIndex+Math.Sign(e.Delta),0,depth-1);RefreshViews();}};
            sagittal.MouseWheel += delegate(object s,MouseEventArgs e){if(width>0){xIndex=Clamp(xIndex+Math.Sign(e.Delta),0,width-1);RefreshViews();}};
            coronal.MouseWheel += delegate(object s,MouseEventArgs e){if(height>0){yIndex=Clamp(yIndex+Math.Sign(e.Delta),0,height-1);RefreshViews();}};
            HookRotation(axial);HookRotation(sagittal);HookRotation(coronal);
        }

        public void Active(string[] dicomFiles){Active(DicomSeriesLoader.Load(dicomFiles));}
        public void ActiveFolder(string folder){var f=DicomSeriesLoader.FindLargestImageSeries(folder);if(f.Length==0)throw new InvalidOperationException("No image DICOM series was found.");Active(f);}
        public void Active(ProcessedDicomImage[] images)
        {
            if(images==null||images.Length==0)throw new ArgumentException("No images supplied.");
            foreach(var i in images)i.Validate();
            width=images[0].Width;height=images[0].Height;
            if(images.Any(i=>i.Width!=width||i.Height!=height))throw new ArgumentException("All slices must have the same dimensions.");
            volume=images;depth=images.Length;CalculateVoxelSpacing();
            // Use DICOM window values only when high precision modality data is available.
            if(volume.All(i=>i.HasModality16))
            {
                double min=Double.MaxValue,max=Double.MinValue;
                foreach(var s in volume)foreach(short v in s.Modality16){if(v<min)min=v;if(v>max)max=v;}
                if(min<max){windowCenter=(min+max)/2.0;windowWidth=Math.Max(1,max-min);}
            }
            xIndex=width/2;yIndex=height/2;zIndex=depth/2;axialAngle=coronalAngle=sagittalAngle=0;RefreshViews();
        }

        private void RefreshViews()
        {
            SetImage(axial,BuildAxial());SetImage(sagittal,BuildSagittal());SetImage(coronal,BuildCoronal());SetImage(volume3D,BuildMip());
            status.Text=String.Format("Volume {0}x{1}x{2}   spacing {3:0.###} x {4:0.###} x {5:0.###} mm",width,height,depth,spacingX,spacingY,spacingZ);
        }

        private void HookRotation(PictureBox box)
        {
            box.MouseDown += delegate(object s,MouseEventArgs e){if(e.Button==MouseButtons.Right)rotatingView=box;};
            box.MouseUp += delegate(object s,MouseEventArgs e){if(rotatingView==box)rotatingView=null;};
            box.MouseMove += delegate(object s,MouseEventArgs e)
            {
                if(rotatingView!=box||box.Image==null)return;
                Rectangle r=GetImageRectangle(box);
                if(!r.Contains(e.Location))return;
                double cx=r.Left+r.Width/2.0,cy=r.Top+r.Height/2.0;
                double angle=Math.Atan2(e.Y-cy,e.X-cx);
                if(box==axial)axialAngle=angle;
                else if(box==coronal)coronalAngle=angle;
                else if(box==sagittal)sagittalAngle=angle;
                RefreshViews();
            };
        }

        private static Rectangle GetImageRectangle(PictureBox box)
        {
            if(box.Image==null)return box.ClientRectangle;
            double ir=(double)box.Image.Width/box.Image.Height,br=(double)box.ClientSize.Width/box.ClientSize.Height;
            if(ir>br){int h=(int)Math.Round(box.ClientSize.Width/ir);return new Rectangle(0,(box.ClientSize.Height-h)/2,box.ClientSize.Width,h);}
            int w=(int)Math.Round(box.ClientSize.Height*ir);return new Rectangle((box.ClientSize.Width-w)/2,0,w,box.ClientSize.Height);
        }

        private Bitmap BuildPlane(double[] center,double[] axisU,double[] axisV,double physicalW,double physicalH,double pixel,double angle,Color vertical,Color horizontal)
        {
            RotateAxes(ref axisU,ref axisV,angle);
            int outW=PhysicalOutputSize(physicalW,pixel),outH=PhysicalOutputSize(physicalH,pixel);
            byte[] data=new byte[outW*outH];
            double halfW=(outW-1)*pixel/2.0,halfH=(outH-1)*pixel/2.0;
            for(int y=0;y<outH;y++)
            {
                double v=y*pixel-halfH;
                for(int x=0;x<outW;x++)
                {
                    double u=x*pixel-halfW;
                    data[y*outW+x]=SamplePatient(center[0]+axisU[0]*u+axisV[0]*v,center[1]+axisU[1]*u+axisV[1]*v,center[2]+axisU[2]*u+axisV[2]*v);
                }
            }
            Bitmap bmp=GrayBitmap(data,outW,outH);
            DrawRotatedCrosshair(bmp,outW/2,outH/2,angle,vertical,horizontal);
            return bmp;
        }

        private static void RotateAxes(ref double[] u,ref double[] v,double a)
        {
            double ca=Math.Cos(a),sa=Math.Sin(a);
            double[] nu={u[0]*ca+v[0]*sa,u[1]*ca+v[1]*sa,u[2]*ca+v[2]*sa};
            double[] nv={-u[0]*sa+v[0]*ca,-u[1]*sa+v[1]*ca,-u[2]*sa+v[2]*ca};
            u=nu;v=nv;
        }

        private static void DrawRotatedCrosshair(Bitmap b,int cx,int cy,double angle,Color vertical,Color horizontal)
        {
            double ca=Math.Cos(angle),sa=Math.Sin(angle),len=Math.Sqrt(b.Width*b.Width+b.Height*b.Height);
            using(Graphics g=Graphics.FromImage(b))
            using(Pen pv=new Pen(vertical,1))
            using(Pen ph=new Pen(horizontal,1))
            {
                g.DrawLine(ph,(float)(cx-len*ca),(float)(cy-len*sa),(float)(cx+len*ca),(float)(cy+len*sa));
                g.DrawLine(pv,(float)(cx+len*sa),(float)(cy-len*ca),(float)(cx-len*sa),(float)(cy+len*ca));
            }
        }

        private Bitmap BuildAxial()
        {
            if(!HasPatientGeometry()) return BuildSourceAxial();
            Bounds b=GetPatientBounds();double[] cp=GetCurrentPatientPoint();
            double pixel=Math.Min(spacingX,Math.Min(spacingY,spacingZ));
            return BuildPlane(cp,new[]{1.0,0.0,0.0},new[]{0.0,1.0,0.0},b.MaxX-b.MinX,b.MaxY-b.MinY,pixel,axialAngle,Color.Cyan,Color.Magenta);
        }

        private Bitmap BuildCoronal()
        {
            if(!HasPatientGeometry()) return BuildLegacyCoronal();
            Bounds b=GetPatientBounds();double[] cp=GetCurrentPatientPoint();
            double pixel=Math.Min(spacingX,Math.Min(spacingY,spacingZ));
            return BuildPlane(cp,new[]{1.0,0.0,0.0},new[]{0.0,0.0,-1.0},b.MaxX-b.MinX,b.MaxZ-b.MinZ,pixel,coronalAngle,Color.Cyan,Color.Yellow);
        }

        private Bitmap BuildSagittal()
        {
            if(!HasPatientGeometry()) return BuildLegacySagittal();
            Bounds b=GetPatientBounds();double[] cp=GetCurrentPatientPoint();
            double pixel=Math.Min(spacingX,Math.Min(spacingY,spacingZ));
            return BuildPlane(cp,new[]{0.0,1.0,0.0},new[]{0.0,0.0,-1.0},b.MaxY-b.MinY,b.MaxZ-b.MinZ,pixel,sagittalAngle,Color.Magenta,Color.Yellow);
        }

        private Bitmap BuildSourceAxial()
        {
            Bitmap b=GrayBitmap((byte[])volume[zIndex].Gray8.Clone(),width,height);
            DrawCrosshair(b,xIndex,yIndex,Color.Cyan,Color.Magenta);return b;
        }

        private Bitmap BuildLegacyCoronal()
        {
            int outW=width,outH=PhysicalOutputSize((depth-1)*spacingZ,spacingX);
            byte[] p=new byte[outW*outH];
            for(int oy=0;oy<outH;oy++)
            {
                double z=(depth-1)*(outH-1-oy)/(double)Math.Max(1,outH-1);
                int z0=Clamp((int)Math.Floor(z),0,depth-1),z1=Clamp(z0+1,0,depth-1);double t=z-z0;
                for(int x=0;x<outW;x++)p[oy*outW+x]=LerpByte(volume[z0].Gray8[yIndex*width+x],volume[z1].Gray8[yIndex*width+x],t);
            }
            return GrayBitmap(p,outW,outH);
        }

        private Bitmap BuildLegacySagittal()
        {
            int outW=height,outH=PhysicalOutputSize((depth-1)*spacingZ,spacingY);
            byte[] p=new byte[outW*outH];
            for(int oy=0;oy<outH;oy++)
            {
                double z=(depth-1)*(outH-1-oy)/(double)Math.Max(1,outH-1);
                int z0=Clamp((int)Math.Floor(z),0,depth-1),z1=Clamp(z0+1,0,depth-1);double t=z-z0;
                for(int y=0;y<outW;y++)p[oy*outW+y]=LerpByte(volume[z0].Gray8[y*width+xIndex],volume[z1].Gray8[y*width+xIndex],t);
            }
            return GrayBitmap(p,outW,outH);
        }

        private bool HasPatientGeometry()
        {
            return volume!=null&&depth>0&&volume[0].ImagePositionPatient!=null&&volume[0].ImagePositionPatient.Length>=3&&
                volume[0].ImageOrientationPatient!=null&&volume[0].ImageOrientationPatient.Length>=6;
        }

        private double[] GetCurrentPatientPoint()
        {
            double[] o=volume[0].ImageOrientationPatient,p=volume[0].ImagePositionPatient;
            double[] n=Normal(o);
            double basePos=Dot(p,n);
            double slicePos=volume[zIndex].ImagePositionPatient!=null?Dot(volume[zIndex].ImagePositionPatient,n):basePos+zIndex*spacingZ;
            double dz=slicePos-basePos;
            return new double[]{
                p[0]+o[0]*xIndex*spacingX+o[3]*yIndex*spacingY+n[0]*dz,
                p[1]+o[1]*xIndex*spacingX+o[4]*yIndex*spacingY+n[1]*dz,
                p[2]+o[2]*xIndex*spacingX+o[5]*yIndex*spacingY+n[2]*dz};
        }

        private byte SamplePatient(double px,double py,double pz)
        {
            double[] o=volume[0].ImageOrientationPatient,p=volume[0].ImagePositionPatient,n=Normal(o);
            double dx=px-p[0],dy=py-p[1],dz=pz-p[2];
            double fx=(dx*o[0]+dy*o[1]+dz*o[2])/spacingX;
            double fy=(dx*o[3]+dy*o[4]+dz*o[5])/spacingY;
            double targetPos=px*n[0]+py*n[1]+pz*n[2];

            double first=Dot(volume[0].ImagePositionPatient,n);
            double last=Dot(volume[depth-1].ImagePositionPatient,n);
            double fz=(Math.Abs(last-first)>.000001)?(targetPos-first)*(depth-1)/(last-first):0;

            if(fx<0||fy<0||fz<0||fx>width-1||fy>height-1||fz>depth-1)return 0;
            int x0=Clamp((int)Math.Floor(fx),0,width-1),x1=Clamp(x0+1,0,width-1);
            int y0=Clamp((int)Math.Floor(fy),0,height-1),y1=Clamp(y0+1,0,height-1);
            int z0=Clamp((int)Math.Floor(fz),0,depth-1),z1=Clamp(z0+1,0,depth-1);
            double tx=fx-x0,ty=fy-y0,tz=fz-z0;
            if(volume[z0].HasModality16&&volume[z1].HasModality16)
            {
                double a=Lerp(volume[z0].Modality16[y0*width+x0],volume[z0].Modality16[y0*width+x1],tx);
                double b=Lerp(volume[z0].Modality16[y1*width+x0],volume[z0].Modality16[y1*width+x1],tx);
                double c=Lerp(volume[z1].Modality16[y0*width+x0],volume[z1].Modality16[y0*width+x1],tx);
                double d=Lerp(volume[z1].Modality16[y1*width+x0],volume[z1].Modality16[y1*width+x1],tx);
                return WindowToByte(Lerp(Lerp(a,b,ty),Lerp(c,d,ty),tz));
            }
            else
            {
                double a=Lerp(volume[z0].Gray8[y0*width+x0],volume[z0].Gray8[y0*width+x1],tx);
                double b=Lerp(volume[z0].Gray8[y1*width+x0],volume[z0].Gray8[y1*width+x1],tx);
                double cc=Lerp(volume[z1].Gray8[y0*width+x0],volume[z1].Gray8[y0*width+x1],tx);
                double d=Lerp(volume[z1].Gray8[y1*width+x0],volume[z1].Gray8[y1*width+x1],tx);
                return (byte)Math.Round(Lerp(Lerp(a,b,ty),Lerp(cc,d,ty),tz));
            }
        }

        private Bounds GetPatientBounds()
        {
            double[] o=volume[0].ImageOrientationPatient,n=Normal(o);
            Bounds b=new Bounds();
            b.MinX=b.MinY=b.MinZ=Double.MaxValue;b.MaxX=b.MaxY=b.MaxZ=Double.MinValue;
            int[] xs={0,width-1},ys={0,height-1},zs={0,depth-1};
            foreach(int z in zs)
            {
                double[] sp=volume[z].ImagePositionPatient??volume[0].ImagePositionPatient;
                foreach(int y in ys)foreach(int x in xs)
                {
                    double px=sp[0]+o[0]*x*spacingX+o[3]*y*spacingY;
                    double py=sp[1]+o[1]*x*spacingX+o[4]*y*spacingY;
                    double pz=sp[2]+o[2]*x*spacingX+o[5]*y*spacingY;
                    b.MinX=Math.Min(b.MinX,px);b.MaxX=Math.Max(b.MaxX,px);
                    b.MinY=Math.Min(b.MinY,py);b.MaxY=Math.Max(b.MaxY,py);
                    b.MinZ=Math.Min(b.MinZ,pz);b.MaxZ=Math.Max(b.MaxZ,pz);
                }
            }
            return b;
        }

        private sealed class Bounds{public double MinX,MaxX,MinY,MaxY,MinZ,MaxZ;}
        private static double[] Normal(double[] o){return new[]{o[1]*o[5]-o[2]*o[4],o[2]*o[3]-o[0]*o[5],o[0]*o[4]-o[1]*o[3]};}
        private static double Dot(double[] a,double[] b){return a[0]*b[0]+a[1]*b[1]+a[2]*b[2];}
        private static double Lerp(double a,double b,double t){return a+(b-a)*t;}
        private byte WindowToByte(double value)
        {
            double low=windowCenter-windowWidth/2.0,high=windowCenter+windowWidth/2.0;
            if(value<=low)return 0;if(value>=high)return 255;
            return (byte)Math.Round((value-low)*255.0/(high-low));
        }
        private static byte LerpByte(byte a,byte b,double t){return (byte)Math.Round(Lerp(a,b,t));}
        private static int PhysicalOutputSize(double physicalLength,double pixelSpacing){return Math.Max(2,(int)Math.Round(Math.Abs(physicalLength)/pixelSpacing)+1);}
        private static int PatientToPixel(double value,double min,double spacing,int count){return Clamp((int)Math.Round((value-min)/spacing),0,count-1);}

        private Bitmap BuildMip()
        {
            // Temporary pseudo-colored 3D MIP preview. This keeps the current
            // stable Gray8 decode path; a true volume renderer will replace it later.
            byte[] p=new byte[width*height];
            for(int z=0;z<depth;z++){byte[] s=volume[z].Gray8;for(int i=0;i<p.Length;i++)if(s[i]>p[i])p[i]=s[i];}
            return ColorMipBitmap(p,width,height);
        }

        private static Bitmap ColorMipBitmap(byte[] pixels,int w,int h)
        {
            var b=new Bitmap(w,h,PixelFormat.Format24bppRgb);
            var d=b.LockBits(new Rectangle(0,0,w,h),ImageLockMode.WriteOnly,PixelFormat.Format24bppRgb);
            try
            {
                byte[] row=new byte[Math.Abs(d.Stride)];
                for(int y=0;y<h;y++)
                {
                    Array.Clear(row,0,row.Length);
                    for(int x=0;x<w;x++)
                    {
                        byte v=pixels[y*w+x];
                        byte r,g,bl;
                        // Simple CT-style transfer function for the preview:
                        // low values stay dark, soft tissue is warm, dense structures become ivory/white.
                        if(v<55){r=(byte)(v/4);g=(byte)(v/6);bl=(byte)(v/8);}
                        else if(v<150)
                        {
                            double t=(v-55)/95.0;
                            r=(byte)(45+150*t);g=(byte)(25+90*t);bl=(byte)(20+55*t);
                        }
                        else
                        {
                            double t=(v-150)/105.0;
                            r=(byte)(195+60*t);g=(byte)(115+140*t);bl=(byte)(75+180*t);
                        }
                        int i=x*3;row[i]=bl;row[i+1]=g;row[i+2]=r;
                    }
                    Marshal.Copy(row,0,IntPtr.Add(d.Scan0,y*d.Stride),row.Length);
                }
            }
            finally{b.UnlockBits(d);}
            return b;
        }

        private void CalculateVoxelSpacing()
        {
            spacingX=spacingY=spacingZ=1;
            if(volume[0].PixelSpacing!=null&&volume[0].PixelSpacing.Length>=2){spacingY=Math.Abs(volume[0].PixelSpacing[0]);spacingX=Math.Abs(volume[0].PixelSpacing[1]);}
            if(depth>1&&volume[0].ImageOrientationPatient!=null)
            {
                double[] o=volume[0].ImageOrientationPatient;double nx=o[1]*o[5]-o[2]*o[4],ny=o[2]*o[3]-o[0]*o[5],nz=o[0]*o[4]-o[1]*o[3];double sum=0;int n=0;
                for(int i=1;i<depth;i++){var a=volume[i-1].ImagePositionPatient;var b=volume[i].ImagePositionPatient;if(a==null||b==null)continue;double d=Math.Abs((b[0]-a[0])*nx+(b[1]-a[1])*ny+(b[2]-a[2])*nz);if(d>.0001){sum+=d;n++;}}
                if(n>0)spacingZ=sum/n;
            }
            if(spacingZ<=0&&volume[0].SpacingBetweenSlices>0)spacingZ=Math.Abs(volume[0].SpacingBetweenSlices);
            if(spacingZ<=0&&volume[0].SliceThickness>0)spacingZ=Math.Abs(volume[0].SliceThickness);
            if(spacingX<=0)spacingX=1;if(spacingY<=0)spacingY=1;if(spacingZ<=0)spacingZ=1;
        }

        private static Bitmap GrayBitmap(byte[] pixels,int w,int h)
        {
            var b=new Bitmap(w,h,PixelFormat.Format24bppRgb);var d=b.LockBits(new Rectangle(0,0,w,h),ImageLockMode.WriteOnly,PixelFormat.Format24bppRgb);
            try{byte[] row=new byte[Math.Abs(d.Stride)];for(int y=0;y<h;y++){Array.Clear(row,0,row.Length);for(int x=0;x<w;x++){byte v=pixels[y*w+x];int i=x*3;row[i]=row[i+1]=row[i+2]=v;}Marshal.Copy(row,0,IntPtr.Add(d.Scan0,y*d.Stride),row.Length);}}finally{b.UnlockBits(d);}return b;
        }
        private static void DrawCrosshair(Bitmap b,int x,int y,Color v,Color h){using(Graphics g=Graphics.FromImage(b))using(var pv=new Pen(v,1))using(var ph=new Pen(h,1)){g.DrawLine(pv,x,0,x,b.Height-1);g.DrawLine(ph,0,y,b.Width-1,y);}}
        private static void SetImage(PictureBox box,Image image){var old=box.Image;box.Image=image;if(old!=null)old.Dispose();}
        private static int Clamp(int v,int min,int max){return Math.Max(min,Math.Min(max,v));}
        protected override void Dispose(bool disposing){if(disposing){if(axial.Image!=null)axial.Image.Dispose();if(sagittal.Image!=null)sagittal.Image.Dispose();if(coronal.Image!=null)coronal.Image.Dispose();if(volume3D.Image!=null)volume3D.Image.Dispose();}base.Dispose(disposing);}
    }
}

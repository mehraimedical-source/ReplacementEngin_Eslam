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

        public MedicalDicomViewerControl()
        {
            InitializeComponent();
            axial.MouseEnter += delegate { axial.Focus(); }; sagittal.MouseEnter += delegate { sagittal.Focus(); }; coronal.MouseEnter += delegate { coronal.Focus(); };
            axial.MouseWheel += delegate(object s,MouseEventArgs e){if(depth>0){zIndex=Clamp(zIndex+Math.Sign(e.Delta),0,depth-1);RefreshViews();}};
            sagittal.MouseWheel += delegate(object s,MouseEventArgs e){if(width>0){xIndex=Clamp(xIndex+Math.Sign(e.Delta),0,width-1);RefreshViews();}};
            coronal.MouseWheel += delegate(object s,MouseEventArgs e){if(height>0){yIndex=Clamp(yIndex+Math.Sign(e.Delta),0,height-1);RefreshViews();}};
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
            windowCenter=127.5;windowWidth=255.0;
            xIndex=width/2;yIndex=height/2;zIndex=depth/2;RefreshViews();
        }

        private void RefreshViews()
        {
            SetImage(axial,BuildAxial());SetImage(sagittal,BuildSagittal());SetImage(coronal,BuildCoronal());SetImage(volume3D,BuildMip());
            status.Text=String.Format("Volume {0}x{1}x{2}   spacing {3:0.###} x {4:0.###} x {5:0.###} mm   WL {6:0}/{7:0}",width,height,depth,spacingX,spacingY,spacingZ,windowCenter,windowWidth);
        }

        private Bitmap BuildAxial()
        {
            byte[] p=new byte[width*height];for(int y=0;y<height;y++)for(int x=0;x<width;x++)p[y*width+x]=DisplayVoxel(x,y,zIndex);
            Bitmap b=GrayBitmap(p,width,height);DrawCrosshair(b,xIndex,yIndex,Color.Cyan,Color.Magenta);return b;
        }

        private Bitmap BuildCoronal()
        {
            double mmW=(width-1)*spacingX,mmH=(depth-1)*spacingZ;
            int outW=width,outH=Math.Max(2,(int)Math.Round(mmH/spacingX)+1);
            byte[] p=new byte[outW*outH];
            for(int oy=0;oy<outH;oy++){double z=(outH==1?0:(depth-1)*(outH-1-oy)/(double)(outH-1));for(int x=0;x<outW;x++)p[oy*outW+x]=SampleDisplay(x,yIndex,z);}
            Bitmap b=GrayBitmap(p,outW,outH);
            int cy=(int)Math.Round((depth-1-zIndex)*(outH-1)/(double)Math.Max(1,depth-1));
            DrawCrosshair(b,xIndex,cy,Color.Cyan,Color.Yellow);return b;
        }

        private Bitmap BuildSagittal()
        {
            double mmH=(depth-1)*spacingZ;
            int outW=height,outH=Math.Max(2,(int)Math.Round(mmH/spacingY)+1);
            byte[] p=new byte[outW*outH];
            for(int oy=0;oy<outH;oy++){double z=(depth-1)*(outH-1-oy)/(double)Math.Max(1,outH-1);for(int y=0;y<outW;y++)p[oy*outW+y]=SampleDisplay(xIndex,y,z);}
            Bitmap b=GrayBitmap(p,outW,outH);
            int cy=(int)Math.Round((depth-1-zIndex)*(outH-1)/(double)Math.Max(1,depth-1));
            DrawCrosshair(b,yIndex,cy,Color.Magenta,Color.Yellow);return b;
        }

        private Bitmap BuildMip()
        {
            byte[] p=new byte[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++){byte m=0;for(int z=0;z<depth;z++){byte v=DisplayVoxel(x,y,z);if(v>m)m=v;}p[y*width+x]=m;}
            return GrayBitmap(p,width,height);
        }

        private byte SampleDisplay(double x,double y,double z)
        {
            int z0=Clamp((int)Math.Floor(z),0,depth-1),z1=Clamp(z0+1,0,depth-1);double t=z-z0;
            double a=HuAt(Clamp((int)Math.Round(x),0,width-1),Clamp((int)Math.Round(y),0,height-1),z0);
            double b=HuAt(Clamp((int)Math.Round(x),0,width-1),Clamp((int)Math.Round(y),0,height-1),z1);
            return Window(a+(b-a)*t);
        }
        private byte DisplayVoxel(int x,int y,int z){return Window(HuAt(x,y,z));}
        private double HuAt(int x,int y,int z)
        {
            var s=volume[z];int i=y*width+x;
            return s.Gray8[i];
        }
        private byte Window(double value)
        {
            double lo=windowCenter-windowWidth/2.0,hi=windowCenter+windowWidth/2.0;
            if(value<=lo)return 0;if(value>=hi)return 255;return (byte)Math.Round((value-lo)*255.0/(hi-lo));
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

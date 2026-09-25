using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DicomViewer_ChatGPT
{
    public partial class MedicalDicomViewerControl : UserControl
    {
        private ProcessedDicomImage[] volume;
        private int width,height,depth,xIndex,yIndex,zIndex;
        private double spacingX=1,spacingY=1,spacingZ=1,windowCenter=40,windowWidth=400;
        private Plane axialPlane,coronalPlane,sagittalPlane;
        private PictureBox dragView;
        private Plane dragPlane,dragCompanion;
        private double dragStartAngle;
        private double[] dragNormal0,dragCompanionNormal0,dragU0,dragV0,dragCompanionU0,dragCompanionV0;
        private DateTime lastInteractiveRender=DateTime.MinValue;
        private bool interactiveRendering;
        private bool draggingCenter;
        private Point centerDragStartMouse;
        private double[] centerDragStartPatient;
        private double[] crosshairPatient;
        private byte[] displayVolume;
        private int sliceStride;
        private double rowX,rowY,rowZ,colX,colY,colZ,normX,normY,normZ;
        private double originX,originY,originZ,invSpacingX,invSpacingY,voxelZScale,firstProjection;

        public MedicalDicomViewerControl()
        {
            InitializeComponent();
            axial.MouseEnter += delegate { axial.Focus(); }; sagittal.MouseEnter += delegate { sagittal.Focus(); }; coronal.MouseEnter += delegate { coronal.Focus(); };
            axial.MouseWheel += delegate(object s,MouseEventArgs e){if(depth>0){zIndex=Clamp(zIndex+Math.Sign(e.Delta),0,depth-1);RefreshViews();}};
            sagittal.MouseWheel += delegate(object s,MouseEventArgs e){if(width>0){xIndex=Clamp(xIndex+Math.Sign(e.Delta),0,width-1);RefreshViews();}};
            coronal.MouseWheel += delegate(object s,MouseEventArgs e){if(height>0){yIndex=Clamp(yIndex+Math.Sign(e.Delta),0,height-1);RefreshViews();}};
            HookPlaneLines(axial);HookPlaneLines(sagittal);HookPlaneLines(coronal);
        }

        private void btnResetAxes_Click(object sender,EventArgs e)
        {
            if(volume==null||depth==0)return;
            InitializePlanes();
            xIndex=width/2;yIndex=height/2;zIndex=depth/2;
            crosshairPatient=GetCurrentPatientPoint();
            dragView=null;dragPlane=null;dragCompanion=null;draggingCenter=false;
            centerDragStartPatient=null;interactiveRendering=false;
            axial.Cursor=sagittal.Cursor=coronal.Cursor=Cursors.Default;
            RefreshViews();
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
            PrepareFastVolume();
            SetImage(volume3D,BuildMip());
            xIndex=width/2;yIndex=height/2;zIndex=depth/2;InitializePlanes();crosshairPatient=GetCurrentPatientPoint();RefreshViews();
        }

        private void RefreshViews()
        {
            SetImage(axial,BuildAxial());SetImage(sagittal,BuildSagittal());SetImage(coronal,BuildCoronal());
            if(volume3D.Image==null)SetImage(volume3D,BuildMip());
            status.Text=String.Format("Volume {0}x{1}x{2}   spacing {3:0.###} x {4:0.###} x {5:0.###} mm",width,height,depth,spacingX,spacingY,spacingZ);
        }

        private void RefreshViewsExcept(PictureBox fixedView)
        {
            // RadiAnt-style center drag: the image under the mouse is the reference
            // and remains fixed; the other two MPR views are reconstructed through
            // the moved patient-space intersection.
            if(fixedView!=axial)SetImage(axial,BuildAxial());
            if(fixedView!=sagittal)SetImage(sagittal,BuildSagittal());
            if(fixedView!=coronal)SetImage(coronal,BuildCoronal());
            // Redraw the fixed view only to move its crosshair overlay. This currently
            // requires a bitmap rebuild because overlays are still baked into images.
            // Preserve its slice center temporarily so the underlying anatomy does not move.
            int ox=xIndex,oy=yIndex,oz=zIndex;
            SetIndicesFromPatient(centerDragStartPatient);
            if(fixedView==axial)SetImage(axial,BuildAxial());
            else if(fixedView==sagittal)SetImage(sagittal,BuildSagittal());
            else if(fixedView==coronal)SetImage(coronal,BuildCoronal());
            xIndex=ox;yIndex=oy;zIndex=oz;
            status.Text=String.Format("Volume {0}x{1}x{2}   spacing {3:0.###} x {4:0.###} x {5:0.###} mm",width,height,depth,spacingX,spacingY,spacingZ);
        }

        private void PrepareFastVolume()
        {
            sliceStride=width*height;displayVolume=new byte[sliceStride*depth];
            for(int z=0;z<depth;z++)
            {
                int dst=z*sliceStride;
                if(volume[z].HasModality16)
                    for(int i=0;i<sliceStride;i++)displayVolume[dst+i]=WindowToByte(volume[z].Modality16[i]);
                else Buffer.BlockCopy(volume[z].Gray8,0,displayVolume,dst,sliceStride);
            }
            double[] o=volume[0].ImageOrientationPatient,p=volume[0].ImagePositionPatient;
            rowX=o[0];rowY=o[1];rowZ=o[2];colX=o[3];colY=o[4];colZ=o[5];
            normX=rowY*colZ-rowZ*colY;normY=rowZ*colX-rowX*colZ;normZ=rowX*colY-rowY*colX;
            originX=p[0];originY=p[1];originZ=p[2];invSpacingX=1.0/spacingX;invSpacingY=1.0/spacingY;
            firstProjection=originX*normX+originY*normY+originZ*normZ;
            double[] lp=volume[depth-1].ImagePositionPatient;
            double last=lp[0]*normX+lp[1]*normY+lp[2]*normZ;
            voxelZScale=Math.Abs(last-firstProjection)>.000001?(depth-1)/(last-firstProjection):0;
        }

        private sealed class Plane
        {
            public double[] U,V,N;
            public Color Color;
            public Plane(double[] u,double[] v,Color color){U=u;V=v;N=Normalize(Cross(u,v));Color=color;}
        }

        private void InitializePlanes()
        {
            // Canonical patient planes: axial(Z), coronal(Y), sagittal(X).
            axialPlane=new Plane(new[]{1.0,0.0,0.0},new[]{0.0,1.0,0.0},Color.Yellow);
            coronalPlane=new Plane(new[]{1.0,0.0,0.0},new[]{0.0,0.0,-1.0},Color.Magenta);
            sagittalPlane=new Plane(new[]{0.0,1.0,0.0},new[]{0.0,0.0,-1.0},Color.Cyan);
        }

        private void HookPlaneLines(PictureBox box)
        {
            box.MouseDown += delegate(object s,MouseEventArgs e)
            {
                if(e.Button!=MouseButtons.Left||box.Image==null||!HasPatientGeometry())return;
                if(IsNearCenter(box,e.Location))
                {
                    draggingCenter=true;dragView=box;centerDragStartMouse=e.Location;
                    centerDragStartPatient=(double[])(crosshairPatient??GetCurrentPatientPoint()).Clone();box.Cursor=Cursors.SizeAll;return;
                }
                Plane hit=HitTestReferenceLine(box,e.Location);
                if(hit!=null)
                {
                    dragView=box;dragPlane=hit;dragCompanion=OtherReferencePlane(box,hit);
                    dragStartAngle=MouseAngleInImage(box,e.Location);
                    dragNormal0=(double[])dragPlane.N.Clone();dragU0=(double[])dragPlane.U.Clone();dragV0=(double[])dragPlane.V.Clone();
                    if(dragCompanion!=null){dragCompanionNormal0=(double[])dragCompanion.N.Clone();dragCompanionU0=(double[])dragCompanion.U.Clone();dragCompanionV0=(double[])dragCompanion.V.Clone();}
                    box.Cursor=Cursors.Hand;
                }
            };
            box.MouseLeave += delegate { if(dragView==null)box.Cursor=Cursors.Default; };
            box.MouseUp += delegate(object s,MouseEventArgs e)
            {
                if(dragView==box){dragView=null;dragPlane=null;dragCompanion=null;draggingCenter=false;centerDragStartPatient=null;interactiveRendering=false;box.Cursor=Cursors.Default;RefreshViews();}
            };
            box.MouseMove += delegate(object s,MouseEventArgs e)
            {
                if(box.Image==null)return;
                if(dragView==null)
                {
                    if(IsNearCenter(box,e.Location))box.Cursor=Cursors.SizeAll;
                    else if(HitTestReferenceLine(box,e.Location)!=null)box.Cursor=Cursors.Hand;
                    else box.Cursor=Cursors.Default;
                    return;
                }
                if(dragView!=box)return;
                if(draggingCenter)
                {
                    MoveCenterFromMouse(box,e.Location);
                    SetIndicesFromPatient(crosshairPatient);
                    if((DateTime.UtcNow-lastInteractiveRender).TotalMilliseconds>=33)
                    {
                        lastInteractiveRender=DateTime.UtcNow;
                        RefreshViewsExcept(box);
                    }
                    return;
                }
                if(dragPlane==null)return;
                Rectangle r=GetImageRectangle(box);if(!r.Contains(e.Location))return;
                Plane view=PlaneForView(box);
                double delta=NormalizeAngle(MouseAngleInImage(box,e.Location)-dragStartAngle);
                // RadiAnt-style coupled rotation: both reference planes rotate by
                // the same delta around the current view normal, so they stay 90 degrees apart.
                ApplyRotatedPlane(dragPlane,dragNormal0,dragU0,dragV0,view.N,delta);
                if(dragCompanion!=null)ApplyRotatedPlane(dragCompanion,dragCompanionNormal0,dragCompanionU0,dragCompanionV0,view.N,delta);
                // Keep full resolution; throttle only redundant mouse events.
                if((DateTime.UtcNow-lastInteractiveRender).TotalMilliseconds>=33)
                {
                    lastInteractiveRender=DateTime.UtcNow;interactiveRendering=true;RefreshViews();
                }
            };
        }

        private bool IsNearCenter(PictureBox box,Point mouse)
        {
            if(box.Image==null)return false;Rectangle r=GetImageRectangle(box);if(!r.Contains(mouse))return false;
            Plane view=PlaneForView(box);double[] center=GetCurrentPatientPoint(),ch=crosshairPatient??center;
            double pixel=Math.Min(spacingX,Math.Min(spacingY,spacingZ));
            double ix=(box.Image.Width-1)/2.0+Dot(Sub(ch,center),view.U)/pixel;
            double iy=(box.Image.Height-1)/2.0+Dot(Sub(ch,center),view.V)/pixel;
            double cx=r.Left+ix*(r.Width-1)/Math.Max(1.0,box.Image.Width-1);
            double cy=r.Top+iy*(r.Height-1)/Math.Max(1.0,box.Image.Height-1);
            double dx=mouse.X-cx,dy=mouse.Y-cy;return dx*dx+dy*dy<=100;
        }

        private void MoveCenterFromMouse(PictureBox box,Point mouse)
        {
            // IMPORTANT: calculate displacement from the fixed MouseDown state.
            // Using the newly reconstructed image/center on every MouseMove creates
            // positive feedback (the crosshair runs ahead and all views drift).
            Rectangle r=GetImageRectangle(box);if(centerDragStartPatient==null||r.Width<2||r.Height<2)return;
            Plane view=PlaneForView(box);
            // Map the mouse delta through the actual rendered image rectangle.
            // PictureBox Zoom can scale X/Y differently from the source pixel count
            // after rounding, so use image-pixels-per-screen-pixel directly.
            double pixel=Math.Min(spacingX,Math.Min(spacingY,spacingZ));
            double scaleX=(box.Image.Width-1)/(double)Math.Max(1,r.Width-1);
            double scaleY=(box.Image.Height-1)/(double)Math.Max(1,r.Height-1);
            double du=(mouse.X-centerDragStartMouse.X)*scaleX*pixel;
            double dv=(mouse.Y-centerDragStartMouse.Y)*scaleY*pixel;
            crosshairPatient=Add(centerDragStartPatient,Add(Scale(view.U,du),Scale(view.V,dv)));
        }

        private void SetIndicesFromPatient(double[] q)
        {
            double[] o=volume[0].ImageOrientationPatient,p=volume[0].ImagePositionPatient,n=Normal(o);
            double dx=q[0]-p[0],dy=q[1]-p[1],dz=q[2]-p[2];
            xIndex=Clamp((int)Math.Round((dx*o[0]+dy*o[1]+dz*o[2])/spacingX),0,width-1);
            yIndex=Clamp((int)Math.Round((dx*o[3]+dy*o[4]+dz*o[5])/spacingY),0,height-1);
            double first=Dot(p,n),last=Dot(volume[depth-1].ImagePositionPatient,n),pos=Dot(q,n);
            zIndex=Clamp(Math.Abs(last-first)>.000001?(int)Math.Round((pos-first)*(depth-1)/(last-first)):0,0,depth-1);
        }

        private Plane HitTestReferenceLine(PictureBox box,Point mouse)
        {
            Rectangle r=GetImageRectangle(box);if(!r.Contains(mouse))return null;
            Plane view=PlaneForView(box);if(view==null)return null;
            double ix=(mouse.X-r.Left)*(box.Image.Width-1)/(double)Math.Max(1,r.Width-1);
            double iy=(mouse.Y-r.Top)*(box.Image.Height-1)/(double)Math.Max(1,r.Height-1);
            // Reference lines pass through the movable patient-space crosshair,
            // not permanently through the bitmap center.
            double[] center=GetCurrentPatientPoint(),ch=crosshairPatient??center;
            double pixel=Math.Min(spacingX,Math.Min(spacingY,spacingZ));
            double cx=(box.Image.Width-1)/2.0+Dot(Sub(ch,center),view.U)/pixel;
            double cy=(box.Image.Height-1)/2.0+Dot(Sub(ch,center),view.V)/pixel;
            Plane[] candidates=box==axial?new[]{coronalPlane,sagittalPlane}:box==coronal?new[]{axialPlane,sagittalPlane}:new[]{axialPlane,coronalPlane};
            Plane best=null;double bestD=8.0;
            foreach(Plane p in candidates)
            {
                double[] dir=IntersectionDirection(view,p);
                double lx=Dot(dir,view.U),ly=Dot(dir,view.V);
                double d=Math.Abs((ix-cx)*ly-(iy-cy)*lx)/Math.Max(.0001,Math.Sqrt(lx*lx+ly*ly));
                if(d<bestD){bestD=d;best=p;}
            }
            return best;
        }

        private Plane PlaneForView(PictureBox box){return box==axial?axialPlane:box==coronal?coronalPlane:sagittalPlane;}
        private Plane OtherReferencePlane(PictureBox box,Plane selected)
        {
            Plane[] a=box==axial?new[]{coronalPlane,sagittalPlane}:box==coronal?new[]{axialPlane,sagittalPlane}:new[]{axialPlane,coronalPlane};
            return a[0]==selected?a[1]:a[0];
        }

        private static double MouseAngleInImage(PictureBox box,Point p)
        {
            Rectangle r=GetImageRectangle(box);
            double x=(p.X-r.Left)/(double)Math.Max(1,r.Width)-.5;
            double y=(p.Y-r.Top)/(double)Math.Max(1,r.Height)-.5;
            return Math.Atan2(y,x);
        }

        private static double NormalizeAngle(double a)
        {
            while(a>Math.PI)a-=Math.PI*2;while(a<-Math.PI)a+=Math.PI*2;return a;
        }

        private static void ApplyRotatedPlane(Plane p,double[] n0,double[] u0,double[] v0,double[] axis,double angle)
        {
            p.N=Normalize(RotateAroundAxis(n0,axis,angle));
            p.U=Normalize(RotateAroundAxis(u0,axis,angle));
            p.V=Normalize(RotateAroundAxis(v0,axis,angle));
        }

        private static double[] RotateAroundAxis(double[] v,double[] axis,double angle)
        {
            axis=Normalize(axis);double c=Math.Cos(angle),s=Math.Sin(angle),d=Dot(axis,v);
            double[] cr=Cross(axis,v);
            return new[]{v[0]*c+cr[0]*s+axis[0]*d*(1-c),v[1]*c+cr[1]*s+axis[1]*d*(1-c),v[2]*c+cr[2]*s+axis[2]*d*(1-c)};
        }


        private static void SetPlaneNormalKeepingIntersection(Plane plane,double[] normal,double[] intersection)
        {
            plane.N=Normalize(normal);
            plane.U=Normalize(intersection);
            plane.V=Normalize(Cross(plane.N,plane.U));
        }

        private static Rectangle GetImageRectangle(PictureBox box)
        {
            if(box.Image==null)return box.ClientRectangle;
            double ir=(double)box.Image.Width/box.Image.Height,br=(double)box.ClientSize.Width/box.ClientSize.Height;
            if(ir>br){int h=(int)Math.Round(box.ClientSize.Width/ir);return new Rectangle(0,(box.ClientSize.Height-h)/2,box.ClientSize.Width,h);}
            int w=(int)Math.Round(box.ClientSize.Height*ir);return new Rectangle((box.ClientSize.Width-w)/2,0,w,box.ClientSize.Height);
        }

        private Bitmap BuildPlane(Plane plane,double[] center,double physicalW,double physicalH,double pixel,Plane lineA,Plane lineB)
        {
            double renderPixel=pixel;
            int outW=PhysicalOutputSize(physicalW,renderPixel),outH=PhysicalOutputSize(physicalH,renderPixel);
            byte[] data=new byte[outW*outH];
            double halfW=(outW-1)*renderPixel/2.0,halfH=(outH-1)*renderPixel/2.0;
            // Incremental patient-space stepping avoids rebuilding the 3-D transform
            // for every output pixel. Rows are independent, so use all CPU cores.
            double sx=plane.U[0]*renderPixel,sy=plane.U[1]*renderPixel,sz=plane.U[2]*renderPixel;
            Parallel.For(0,outH,y=>
            {
                double v=y*renderPixel-halfH;
                double px=center[0]-plane.U[0]*halfW+plane.V[0]*v;
                double py=center[1]-plane.U[1]*halfW+plane.V[1]*v;
                double pz=center[2]-plane.U[2]*halfW+plane.V[2]*v;
                int row=y*outW;
                for(int x=0;x<outW;x++,px+=sx,py+=sy,pz+=sz)
                    data[row+x]=SamplePatientFast(px,py,pz);
            });
            Bitmap bmp=GrayBitmap(data,outW,outH);
            double[] ch=crosshairPatient??center;
            double cx=(outW-1)/2.0+Dot(Sub(ch,center),plane.U)/renderPixel;
            double cy=(outH-1)/2.0+Dot(Sub(ch,center),plane.V)/renderPixel;
            DrawPlaneLine(bmp,plane,lineA,lineA.Color,cx,cy);
            DrawPlaneLine(bmp,plane,lineB,lineB.Color,cx,cy);
            return bmp;
        }

        private static void DrawPlaneLine(Bitmap bmp,Plane view,Plane other,Color color,double cx,double cy)
        {
            double[] d=IntersectionDirection(view,other);
            double x=Dot(d,view.U),y=Dot(d,view.V),len=Math.Sqrt(bmp.Width*bmp.Width+bmp.Height*bmp.Height);
            using(Graphics g=Graphics.FromImage(bmp))using(Pen p=new Pen(color,1))
                g.DrawLine(p,(float)(cx-x*len),(float)(cy-y*len),(float)(cx+x*len),(float)(cy+y*len));
        }

        private static double[] IntersectionDirection(Plane a,Plane b)
        {
            double[] d=Cross(a.N,b.N);
            double l=Math.Sqrt(Dot(d,d));
            return l<.000001?new[]{1.0,0.0,0.0}:Scale(d,1.0/l);
        }

        private Bitmap BuildAxial()
        {
            if(!HasPatientGeometry()) return BuildSourceAxial();
            Bounds b=GetPatientBounds();double pixel=Math.Min(spacingX,Math.Min(spacingY,spacingZ));
            return BuildPlane(axialPlane,GetCurrentPatientPoint(),b.MaxX-b.MinX,b.MaxY-b.MinY,pixel,coronalPlane,sagittalPlane);
        }

        private Bitmap BuildCoronal()
        {
            if(!HasPatientGeometry()) return BuildLegacyCoronal();
            Bounds b=GetPatientBounds();double pixel=Math.Min(spacingX,Math.Min(spacingY,spacingZ));
            return BuildPlane(coronalPlane,GetCurrentPatientPoint(),b.MaxX-b.MinX,b.MaxZ-b.MinZ,pixel,axialPlane,sagittalPlane);
        }

        private Bitmap BuildSagittal()
        {
            if(!HasPatientGeometry()) return BuildLegacySagittal();
            Bounds b=GetPatientBounds();double pixel=Math.Min(spacingX,Math.Min(spacingY,spacingZ));
            return BuildPlane(sagittalPlane,GetCurrentPatientPoint(),b.MaxY-b.MinY,b.MaxZ-b.MinZ,pixel,axialPlane,coronalPlane);
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

        private byte SamplePatientFast(double px,double py,double pz)
        {
            double dx=px-originX,dy=py-originY,dz=pz-originZ;
            double fx=(dx*rowX+dy*rowY+dz*rowZ)*invSpacingX;
            double fy=(dx*colX+dy*colY+dz*colZ)*invSpacingY;
            double fz=((px*normX+py*normY+pz*normZ)-firstProjection)*voxelZScale;
            if(fx<0||fy<0||fz<0||fx>width-1||fy>height-1||fz>depth-1)return 0;
            int x0=(int)fx,y0=(int)fy,z0=(int)fz;
            int x1=x0<width-1?x0+1:x0,y1=y0<height-1?y0+1:y0,z1=z0<depth-1?z0+1:z0;
            double tx=fx-x0,ty=fy-y0,tz=fz-z0;
            int b0=z0*sliceStride,b1=z1*sliceStride;
            int i00=b0+y0*width+x0,i01=b0+y0*width+x1,i10=b0+y1*width+x0,i11=b0+y1*width+x1;
            int j00=b1+y0*width+x0,j01=b1+y0*width+x1,j10=b1+y1*width+x0,j11=b1+y1*width+x1;
            double a=Lerp(displayVolume[i00],displayVolume[i01],tx),b=Lerp(displayVolume[i10],displayVolume[i11],tx);
            double cc=Lerp(displayVolume[j00],displayVolume[j01],tx),d=Lerp(displayVolume[j10],displayVolume[j11],tx);
            return (byte)Math.Round(Lerp(Lerp(a,b,ty),Lerp(cc,d,ty),tz));
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
        private static double[] Cross(double[] a,double[] b){return new[]{a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0]};}
        private static double[] Normalize(double[] a){double l=Math.Sqrt(Dot(a,a));return l<.000001?new[]{0.0,0.0,0.0}:new[]{a[0]/l,a[1]/l,a[2]/l};}
        private static double[] Scale(double[] a,double s){return new[]{a[0]*s,a[1]*s,a[2]*s};}
        private static double[] Add(double[] a,double[] b){return new[]{a[0]+b[0],a[1]+b[1],a[2]+b[2]};}
        private static double[] Sub(double[] a,double[] b){return new[]{a[0]-b[0],a[1]-b[1],a[2]-b[2]};}
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

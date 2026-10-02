namespace MEHR_PACSViewer
{
    partial class uc_VoiceRecorderNew
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox picMicrophone;
        private VoiceLevelMeter levelMeter;
        private System.Windows.Forms.Label lbPause;
        private System.Windows.Forms.Label lbTime;
        private System.Windows.Forms.Label picOK;
        private System.Windows.Forms.Label picCancel;
        private System.Windows.Forms.Timer tmrVoiceTiming;

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(tmrVoiceTiming!=null)tmrVoiceTiming.Stop();
                _recorder.Dispose();
                if(components!=null)components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.picMicrophone = new System.Windows.Forms.PictureBox();
            this.levelMeter = new MEHR_PACSViewer.VoiceLevelMeter();
            this.lbPause = new System.Windows.Forms.Label();
            this.lbTime = new System.Windows.Forms.Label();
            this.picOK = new System.Windows.Forms.Label();
            this.picCancel = new System.Windows.Forms.Label();
            this.tmrVoiceTiming = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picMicrophone)).BeginInit();
            this.SuspendLayout();
            // 
            // picMicrophone
            // 
            this.picMicrophone.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picMicrophone.Image = global::MEHR_PACSViewer.Properties.Resources.Microphone_Blue;
            this.picMicrophone.Location = new System.Drawing.Point(2, 8);
            this.picMicrophone.Name = "picMicrophone";
            this.picMicrophone.Size = new System.Drawing.Size(24, 34);
            this.picMicrophone.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picMicrophone.TabIndex = 0;
            this.picMicrophone.TabStop = false;
            this.picMicrophone.Click += new System.EventHandler(this.picMicrophone_Click);
            // 
            // levelMeter
            // 
            this.levelMeter.BackColor = System.Drawing.Color.Black;
            this.levelMeter.Location = new System.Drawing.Point(27, 6);
            this.levelMeter.Name = "levelMeter";
            this.levelMeter.Size = new System.Drawing.Size(8, 37);
            this.levelMeter.TabIndex = 1;
            this.levelMeter.Value = 0;
            // 
            // lbPause
            // 
            this.lbPause.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lbPause.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbPause.ForeColor = System.Drawing.Color.White;
            this.lbPause.Location = new System.Drawing.Point(39, 2);
            this.lbPause.Name = "lbPause";
            this.lbPause.Size = new System.Drawing.Size(56, 18);
            this.lbPause.TabIndex = 2;
            this.lbPause.Text = "II";
            this.lbPause.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbPause.Click += new System.EventHandler(this.lbPause_Click);
            // 
            // lbTime
            // 
            this.lbTime.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbTime.ForeColor = System.Drawing.Color.White;
            this.lbTime.Location = new System.Drawing.Point(41, 20);
            this.lbTime.Name = "lbTime";
            this.lbTime.Size = new System.Drawing.Size(54, 20);
            this.lbTime.TabIndex = 3;
            this.lbTime.Text = "00:00";
            this.lbTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picOK
            // 
            this.picOK.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picOK.Font = new System.Drawing.Font("Arial", 17F, System.Drawing.FontStyle.Bold);
            this.picOK.ForeColor = System.Drawing.Color.LimeGreen;
            this.picOK.Location = new System.Drawing.Point(101, 13);
            this.picOK.Name = "picOK";
            this.picOK.Size = new System.Drawing.Size(22, 30);
            this.picOK.TabIndex = 4;
            this.picOK.Text = "✓";
            this.picOK.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.picOK.Click += new System.EventHandler(this.picOK_Click);
            // 
            // picCancel
            // 
            this.picCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picCancel.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Bold);
            this.picCancel.ForeColor = System.Drawing.Color.OrangeRed;
            this.picCancel.Location = new System.Drawing.Point(121, 13);
            this.picCancel.Name = "picCancel";
            this.picCancel.Size = new System.Drawing.Size(20, 32);
            this.picCancel.TabIndex = 5;
            this.picCancel.Text = "×";
            this.picCancel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.picCancel.Click += new System.EventHandler(this.picCancel_Click);
            // 
            // tmrVoiceTiming
            // 
            this.tmrVoiceTiming.Interval = 80;
            this.tmrVoiceTiming.Tick += new System.EventHandler(this.tmrVoiceTiming_Tick);
            // 
            // uc_VoiceRecorder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.picCancel);
            this.Controls.Add(this.picOK);
            this.Controls.Add(this.lbTime);
            this.Controls.Add(this.lbPause);
            this.Controls.Add(this.levelMeter);
            this.Controls.Add(this.picMicrophone);
            this.Name = "uc_VoiceRecorder";
            this.Size = new System.Drawing.Size(144, 49);
            ((System.ComponentModel.ISupportInitialize)(this.picMicrophone)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
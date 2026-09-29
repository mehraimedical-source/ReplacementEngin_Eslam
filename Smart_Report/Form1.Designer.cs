namespace Smart_Report
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.TextBox txtOcrInput;
        private System.Windows.Forms.TextBox txtBlockOutput;
        private System.Windows.Forms.Button btnBuildBlocks;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblInput = new System.Windows.Forms.Label();
            this.lblOutput = new System.Windows.Forms.Label();
            this.txtOcrInput = new System.Windows.Forms.TextBox();
            this.txtBlockOutput = new System.Windows.Forms.TextBox();
            this.btnBuildBlocks = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblInput
            // 
            this.lblInput.AutoSize = true;
            this.lblInput.Location = new System.Drawing.Point(12, 14);
            this.lblInput.Name = "lblInput";
            this.lblInput.Size = new System.Drawing.Size(82, 13);
            this.lblInput.TabIndex = 0;
            this.lblInput.Text = "Raw OCR Text:";
            // 
            // lblOutput
            // 
            this.lblOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(12, 310);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(72, 13);
            this.lblOutput.TabIndex = 3;
            this.lblOutput.Text = "Block Output:";
            // 
            // txtOcrInput
            // 
            this.txtOcrInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOcrInput.Location = new System.Drawing.Point(15, 33);
            this.txtOcrInput.Multiline = true;
            this.txtOcrInput.Name = "txtOcrInput";
            this.txtOcrInput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOcrInput.Size = new System.Drawing.Size(954, 218);
            this.txtOcrInput.TabIndex = 1;
            this.txtOcrInput.WordWrap = false;
            // 
            // txtBlockOutput
            // 
            this.txtBlockOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBlockOutput.Location = new System.Drawing.Point(15, 330);
            this.txtBlockOutput.Multiline = true;
            this.txtBlockOutput.Name = "txtBlockOutput";
            this.txtBlockOutput.ReadOnly = true;
            this.txtBlockOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBlockOutput.Size = new System.Drawing.Size(954, 258);
            this.txtBlockOutput.TabIndex = 4;
            this.txtBlockOutput.WordWrap = false;
            // 
            // btnBuildBlocks
            // 
            this.btnBuildBlocks.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnBuildBlocks.Location = new System.Drawing.Point(419, 263);
            this.btnBuildBlocks.Name = "btnBuildBlocks";
            this.btnBuildBlocks.Size = new System.Drawing.Size(145, 34);
            this.btnBuildBlocks.TabIndex = 2;
            this.btnBuildBlocks.Text = "Build Blocks";
            this.btnBuildBlocks.UseVisualStyleBackColor = true;
            this.btnBuildBlocks.Click += new System.EventHandler(this.btnBuildBlocks_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 601);
            this.Controls.Add(this.txtBlockOutput);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.btnBuildBlocks);
            this.Controls.Add(this.txtOcrInput);
            this.Controls.Add(this.lblInput);
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smart Report - OCR Block Builder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

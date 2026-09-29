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
        private System.Windows.Forms.Button btnInitialRules;
        private System.Windows.Forms.Button btnRunTests;
        private System.Windows.Forms.Button btnAnalyzeJson;
        private System.Windows.Forms.Label lblJsonOutput;
        private System.Windows.Forms.TextBox txtJsonOutput;

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
            this.btnInitialRules = new System.Windows.Forms.Button();
            this.btnRunTests = new System.Windows.Forms.Button();
            this.btnAnalyzeJson = new System.Windows.Forms.Button();
            this.lblJsonOutput = new System.Windows.Forms.Label();
            this.txtJsonOutput = new System.Windows.Forms.TextBox();
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
            this.txtBlockOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
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
            // btnInitialRules
            // 
            this.btnInitialRules.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnInitialRules.Location = new System.Drawing.Point(268, 263);
            this.btnInitialRules.Name = "btnInitialRules";
            this.btnInitialRules.Size = new System.Drawing.Size(145, 34);
            this.btnInitialRules.TabIndex = 5;
            this.btnInitialRules.Text = "Initial Rules";
            this.btnInitialRules.UseVisualStyleBackColor = true;
            this.btnInitialRules.Click += new System.EventHandler(this.btnInitialRules_Click);
            // 
            // btnRunTests
            // 
            this.btnRunTests.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnRunTests.Location = new System.Drawing.Point(570, 263);
            this.btnRunTests.Name = "btnRunTests";
            this.btnRunTests.Size = new System.Drawing.Size(145, 34);
            this.btnRunTests.TabIndex = 6;
            this.btnRunTests.Text = "Run Regression Tests";
            this.btnRunTests.UseVisualStyleBackColor = true;
            this.btnRunTests.Click += new System.EventHandler(this.btnRunTests_Click);
            // 
            // btnAnalyzeJson
            // 
            this.btnAnalyzeJson.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnAnalyzeJson.Location = new System.Drawing.Point(721, 263);
            this.btnAnalyzeJson.Name = "btnAnalyzeJson";
            this.btnAnalyzeJson.Size = new System.Drawing.Size(145, 34);
            this.btnAnalyzeJson.TabIndex = 7;
            this.btnAnalyzeJson.Text = "Analyze JSON";
            this.btnAnalyzeJson.UseVisualStyleBackColor = true;
            this.btnAnalyzeJson.Click += new System.EventHandler(this.btnAnalyzeJson_Click);
            // 
            // lblJsonOutput
            // 
            this.lblJsonOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblJsonOutput.AutoSize = true;
            this.lblJsonOutput.Location = new System.Drawing.Point(12, 601);
            this.lblJsonOutput.Name = "lblJsonOutput";
            this.lblJsonOutput.Size = new System.Drawing.Size(73, 13);
            this.lblJsonOutput.TabIndex = 8;
            this.lblJsonOutput.Text = "JSON Output:";
            // 
            // txtJsonOutput
            // 
            this.txtJsonOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtJsonOutput.Location = new System.Drawing.Point(15, 620);
            this.txtJsonOutput.Multiline = true;
            this.txtJsonOutput.Name = "txtJsonOutput";
            this.txtJsonOutput.ReadOnly = true;
            this.txtJsonOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtJsonOutput.Size = new System.Drawing.Size(954, 218);
            this.txtJsonOutput.TabIndex = 9;
            this.txtJsonOutput.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 851);
            this.Controls.Add(this.txtJsonOutput);
            this.Controls.Add(this.lblJsonOutput);
            this.Controls.Add(this.btnAnalyzeJson);
            this.Controls.Add(this.btnRunTests);
            this.Controls.Add(this.btnInitialRules);
            this.Controls.Add(this.txtBlockOutput);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.btnBuildBlocks);
            this.Controls.Add(this.txtOcrInput);
            this.Controls.Add(this.lblInput);
            this.MinimumSize = new System.Drawing.Size(700, 700);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smart Report - OCR Block Builder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

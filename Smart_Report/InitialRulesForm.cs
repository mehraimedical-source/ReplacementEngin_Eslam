using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Smart_Report
{
    public class InitialRulesForm : Form
    {
        private TextBox txtSections;
        private TextBox txtParameters;
        private TextBox txtMeasurements;
        private Button btnApply;
        private Button btnCancel;

        public OcrBlockRules Rules;

        public InitialRulesForm(OcrBlockRules rules)
        {
            Rules = rules.Clone();
            Text = "Initial OCR Block Rules";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(760, 520);
            MinimumSize = new Size(650, 420);

            Label l1 = new Label(); l1.Text = "Sections (one per line)"; l1.Location = new Point(12, 12); l1.AutoSize = true;
            Label l2 = new Label(); l2.Text = "Parameters (empty values are allowed)"; l2.Location = new Point(255, 12); l2.AutoSize = true;
            Label l3 = new Label(); l3.Text = "Measurements"; l3.Location = new Point(500, 12); l3.AutoSize = true;

            txtSections = MakeBox(12);
            txtParameters = MakeBox(255);
            txtMeasurements = MakeBox(500);

            txtSections.Text = Join(Rules.Sections);
            txtParameters.Text = Join(Rules.Parameters);
            txtMeasurements.Text = Join(Rules.Measurements);

            btnApply = new Button(); btnApply.Text = "Apply"; btnApply.Size = new Size(100, 30); btnApply.Location = new Point(270, 440); btnApply.Anchor = AnchorStyles.Bottom; btnApply.Click += new EventHandler(Apply_Click);
            btnCancel = new Button(); btnCancel.Text = "Cancel"; btnCancel.Size = new Size(100, 30); btnCancel.Location = new Point(380, 440); btnCancel.Anchor = AnchorStyles.Bottom; btnCancel.DialogResult = DialogResult.Cancel;

            Controls.Add(l1); Controls.Add(l2); Controls.Add(l3);
            Controls.Add(txtSections); Controls.Add(txtParameters); Controls.Add(txtMeasurements);
            Controls.Add(btnApply); Controls.Add(btnCancel);
            AcceptButton = btnApply; CancelButton = btnCancel;
        }

        private TextBox MakeBox(int x)
        {
            TextBox b = new TextBox();
            b.Multiline = true; b.ScrollBars = ScrollBars.Both; b.WordWrap = false;
            b.Location = new Point(x, 34); b.Size = new Size(230, 390);
            b.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            return b;
        }

        private string Join(List<string> items)
        {
            return String.Join(Environment.NewLine, items.ToArray());
        }

        private List<string> ReadLines(TextBox box)
        {
            List<string> result = new List<string>();
            string[] lines = box.Text.Replace("\r", "").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string value = lines[i].Trim();
                if (value.Length > 0 && !result.Contains(value))
                    result.Add(value);
            }
            return result;
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            Rules.Sections = ReadLines(txtSections);
            Rules.Parameters = ReadLines(txtParameters);
            Rules.Measurements = ReadLines(txtMeasurements);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

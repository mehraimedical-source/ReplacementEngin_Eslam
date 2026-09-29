using System;
using System.Windows.Forms;

namespace Smart_Report
{
    public partial class Form1 : Form
    {
        private OcrBlockRules _rules;
        private OcrBlockBuilder _blockBuilder;

        public Form1()
        {
            InitializeComponent();
            _rules = OcrBlockRules.CreateDefault();
            _blockBuilder = new OcrBlockBuilder(_rules);
        }

        private void btnInitialRules_Click(object sender, EventArgs e)
        {
            using (InitialRulesForm form = new InitialRulesForm(_rules))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _rules = form.Rules;
                    _blockBuilder.SetRules(_rules);
                }
            }
        }

        private void btnBuildBlocks_Click(object sender, EventArgs e)
        {
            txtBlockOutput.Text = _blockBuilder.BuildBracketText(txtOcrInput.Text);
        }

        private void btnRunTests_Click(object sender, EventArgs e)
        {
            OcrRegressionTests tests = new OcrRegressionTests();
            txtBlockOutput.Text = tests.Run(_blockBuilder);
        }
    }
}

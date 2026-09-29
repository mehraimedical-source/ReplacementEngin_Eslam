using System;
using System.Windows.Forms;

namespace Smart_Report
{
    public partial class Form1 : Form
    {
        private OcrBlockBuilder _blockBuilder;

        public Form1()
        {
            InitializeComponent();
            _blockBuilder = new OcrBlockBuilder();
        }

        private void btnBuildBlocks_Click(object sender, EventArgs e)
        {
            txtBlockOutput.Text = _blockBuilder.BuildBracketText(txtOcrInput.Text);
        }
    }
}

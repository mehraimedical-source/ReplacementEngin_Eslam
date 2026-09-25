using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RegeX_Sample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text = @"Order 123 was created.
                Order 456 was canceled.";

            MatchCollection results = Regex.Matches(text, @"\d+");
            text += "\r\nResult:";
            foreach (Match match in results)
            {
                text += match.Value + "\r\n";
            }
            MessageBox.Show(this, text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string input = "Product 125 costs 350 dollars.";
            string result = Regex.Replace(input, @"\d+", "{" + @"\d+" + "}");
            MessageBox.Show(this, result);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string input = "John:25\nSara:31\nAli:42\nmehrdad ";
            string result =  Regex.Replace(input, @"(\w+):(\d+)", "$1 is $2 years old");
            MessageBox.Show(this, result);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string input = "John:25\nSara:31\nAli:42\n42";
            string result = Regex.Replace(input, @"(?<name>\w+):(?<age>\d+)", "${name} is ${age} years old");
            MessageBox.Show(this, result);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string input = @"BPD 42.34 42.34 mm 18w5d±12d Hadlock
HC 157.42 157.42 mm 18w4d±10d Hadlock
AC 129.74 129.74 mm 18w3d±14d Hadlock";
            MatchCollection matches = Regex.Matches(input, @"\d+w\d+d±\d+d");

            ShowMatches(matches);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string input = "FL 28.37 28.37 mm 18w4d±12d Hadlock 60.01 Hadlock";
            MatchCollection matches = Regex.Matches(input, @"\d+\.\d+");
            ShowMatches(matches);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string input = "FL 28.37 28.37 mm 18w4d±12d Hadlock 60.01 Hadlock";
            MatchCollection matches = Regex.Matches(input, @"\d+");
            ShowMatches(matches);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string input = "BPD 42.34 mm 18w5d";
            MatchCollection matches = Regex.Matches(input, @"[0-9]+");
            ShowMatches(matches);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string input = @"BPD42.34
BPD 42.34
BPD       42.34";
            MatchCollection matches = Regex.Matches(input, @"BPD\s*\d+\.\d+", RegexOptions.IgnoreCase);
            ShowMatches(matches);

        }





        private void ShowMatches(MatchCollection matches)
        {
            string result = "";
            foreach (Match m in matches)
            {
                result += m.Value + "\n";
            }
            MessageBox.Show(this, result);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string input = "Fetal HR 142 bpm BPD 42.34 mm HC 157.42 mm";
            MatchCollection matches = Regex.Matches(input, @"\d+(\.\d+)?");
            ShowMatches(matches);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string input = "BPD 42.34 mm HC 157.42 mm AC 129.74 mm";
            MatchCollection matches = Regex.Matches(input, @"HC.*?mm");
            ShowMatches(matches);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string input = "BPD 42.34 mm HC 157.42 mm Fetal HR 142 bpm";
            MatchCollection matches = Regex.Matches(input, @"\d{2,3}");
            ShowMatches(matches);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MangaCrawlerTest
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        public void WriteLine(string a_str, Color a_color)
        {
            a_str = a_str + Environment.NewLine;
            int length = richTextBox.TextLength;
            richTextBox.AppendText(a_str);
            richTextBox.SelectionStart = length;
            richTextBox.SelectionLength = a_str.Length;
            richTextBox.SelectionColor = a_color;
        }
    }
}

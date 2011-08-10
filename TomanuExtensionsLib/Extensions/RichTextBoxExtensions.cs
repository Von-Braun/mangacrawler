using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TomanuExtensions
{
    public static class RichTextBoxExtensions
    {
        public static void ScrollToEnd(this RichTextBox a_edit)
        {
            a_edit.SelectionStart = a_edit.Text.Length;
            a_edit.ScrollToCaret();
        }
    }
}

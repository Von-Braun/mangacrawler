using System.Diagnostics;
using System.Windows.Forms;

namespace TomanuExtensions
{
    [DebuggerStepThrough]
    public static class RichTextBoxExtensions
    {
        public static void ScrollToEnd(this RichTextBox a_ruch_edit)
        {
            a_ruch_edit.SelectionStart = a_ruch_edit.Text.Length;
            a_ruch_edit.ScrollToCaret();
        }
    }
}
using System.Diagnostics;
using System.Windows.Forms;

namespace TomanuExtensions
{
    [DebuggerStepThrough]
    public static class TextBoxExtensions
    {
        public static void ScrollToEnd(this TextBox a_edit)
        {
            a_edit.SelectionStart = a_edit.Text.Length;
            a_edit.SelectionLength = 0;
            a_edit.ScrollToCaret();
        }
    }
}
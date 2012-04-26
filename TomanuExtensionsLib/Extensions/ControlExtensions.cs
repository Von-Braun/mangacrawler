using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TomanuExtensions
{
    public static class ControlExtensions
    {
        [DllImport("user32.dll", EntryPoint = "SendMessageA", ExactSpelling = true, CharSet = CharSet.Ansi, 
            SetLastError = true)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        private const int WM_SETREDRAW = 0xB;

        public static void SuspendDrawing(this Control a_control)
        {
            SendMessage(a_control.Handle, WM_SETREDRAW, 0, 0);
        }

        public static void ResumeDrawing(this Control a_control)
        {
            SendMessage(a_control.Handle, WM_SETREDRAW, 1, 0);
        }

    }
}

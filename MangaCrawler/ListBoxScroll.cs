
using System.Windows.Forms;

namespace MangaCrawler
{
    public class ListBoxScroll : ListBox
    {
        private const int WM_VSCROLL = 0x0115;
        private const int WM_HSCROLL = 0x0114;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;
        private const int SB_THUMBTRACK = 5;
        private const int SB_ENDSCROLL = 8;

        public delegate void ListBoxScrollDelegate(object a_sender, bool a_tracking);

        public event ListBoxScrollDelegate VerticalScroll;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_MOUSEWHEEL)
            {
                if (VerticalScroll != null)
                    VerticalScroll(this, false);
            }

            if (m.Msg == WM_MOUSEHWHEEL)
            {
                if (VerticalScroll != null)
                    VerticalScroll(this, false);
            }

            if (m.Msg == WM_VSCROLL)
            {
                int nfy = m.WParam.ToInt32() & 0xFFFF;
                if (VerticalScroll != null && (nfy == SB_THUMBTRACK || nfy == SB_ENDSCROLL))
                    VerticalScroll(this, nfy == SB_THUMBTRACK);
            }
        }
    }
}

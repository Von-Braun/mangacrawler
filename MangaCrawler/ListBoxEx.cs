using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using MangaCrawlerLib;
using System.Drawing;

namespace MangaCrawler
{
    public class ListBoxEx : ListBox
    {
        private bool m_reloading;

        private const int WM_VSCROLL = 0x0115;
        private const int SB_THUMBTRACK = 5;
        private const int SB_ENDSCROLL = 8;

        public delegate void ListBoxScrollDelegate(object a_sender, int a_topIndex, bool a_tracking);
        public event ListBoxScrollDelegate HorizontalScroll;

        public ListBoxEx()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |  ControlStyles.ResizeRedraw |  ControlStyles.UserPaint, true);
            DrawMode = DrawMode.OwnerDrawFixed;  
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Region region = new Region(e.ClipRectangle);
            e.Graphics.FillRegion(new SolidBrush(BackColor), region);

            if (Items.Count > 0)
            {
                for (int i = 0; i < Items.Count; ++i)
                {
                    Rectangle rect = GetItemRectangle(i);
                    if (e.ClipRectangle.IntersectsWith(rect))
                    {
                        if ((SelectionMode == SelectionMode.One && SelectedIndex == i) || 
                            (SelectionMode == SelectionMode.MultiSimple && SelectedIndices.Contains(i)) || 
                            (SelectionMode == SelectionMode.MultiExtended && SelectedIndices.Contains(i)))
                        {
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, rect, i, DrawItemState.Selected, ForeColor, BackColor));
                        }
                        else
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, rect, i, DrawItemState.Default, ForeColor, BackColor));

                        region.Complement(rect);
                    }
                }
            }

            base.OnPaint(e);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (!m_reloading)
                base.OnSelectedIndexChanged(e);
        }

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            if (!m_reloading)
                base.OnSelectedValueChanged(e);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_VSCROLL)
            {
                int nfy = m.WParam.ToInt32() & 0xFFFF;
                if (HorizontalScroll != null && (nfy == SB_THUMBTRACK || nfy == SB_ENDSCROLL))
                    HorizontalScroll(this, TopIndex, nfy == SB_THUMBTRACK);
            }
        }

        public void SelectAll()
        {
            BeginUpdate();

            try
            {
                int topIndex = IndexFromPoint(0, 0);
                int selectedIndex = SelectedIndex;

                for (int i = 0; i < Items.Count; i++)
                    SetSelected(i, true);

                TopIndex = topIndex;

                if (selectedIndex != -1)
                {
                    SetSelected(selectedIndex, false);
                    SetSelected(selectedIndex, true);
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        public void ReloadItems<T>(IEnumerable<T> a_enum, VisualState a_state) where T : class
        {
            if (Capture)
                return;

            BeginUpdate();
            m_reloading = true;

            try
            {
                Items.Clear();
                Items.AddRange(a_enum.ToArray());

                a_state.Restore();
            }
            finally
            {
                EndUpdate();
                m_reloading = false;
            }
        }

        public void RaiseSelectionChanged()
        {
            OnSelectedIndexChanged(new EventArgs());
        }
    }
}

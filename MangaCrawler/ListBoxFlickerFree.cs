using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

namespace MangaCrawler
{
    public class ListBoxFlickerFree : ListBoxScroll
    {
        public ListBoxFlickerFree()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |  ControlStyles.ResizeRedraw |  
                ControlStyles.UserPaint, true);
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
                            (SelectionMode == SelectionMode.MultiSimple && 
                                SelectedIndices.Contains(i)) || 
                            (SelectionMode == SelectionMode.MultiExtended && 
                                SelectedIndices.Contains(i)))
                        {
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, rect, i, 
                                DrawItemState.Selected, ForeColor, BackColor));
                        }
                        else
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, rect, i, 
                                DrawItemState.Default, ForeColor, BackColor));

                        region.Complement(rect);
                    }
                }
            }

            base.OnPaint(e);
        }
    }
}

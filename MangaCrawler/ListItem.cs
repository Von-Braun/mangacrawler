﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace MangaCrawler
{
    public abstract class ListItem
    {
        public override bool Equals(object a_obj)
        {
            if (a_obj == null)
                return false;
            ListItem li = a_obj as ListItem;
            if (li == null)
                return false;
            return ID == li.ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        protected void DrawItem(DrawItemEventArgs e, string a_text,
            Action<Rectangle, Font> a_draw_tip)
        {
            e.DrawBackground();

            if (e.State.HasFlag(DrawItemState.Selected))
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);

            var size = e.Graphics.MeasureString(a_text, e.Font);
            Rectangle bounds = new Rectangle(e.Bounds.X, e.Bounds.Y +
                (e.Bounds.Height - size.ToSize().Height) / 2,
                e.Bounds.Width, size.ToSize().Height);

            e.Graphics.DrawString(a_text, e.Font, Brushes.Black, bounds,
                StringFormat.GenericDefault);

            int left = (int)Math.Round(size.Width + e.Graphics.MeasureString(" ", e.Font).Width);
            Font font = new Font(e.Font.FontFamily, e.Font.Size * 9 / 10, FontStyle.Bold);
            size = e.Graphics.MeasureString("(ABGHRTW%)", font).ToSize();
            bounds = new Rectangle(left, e.Bounds.Y +
                (e.Bounds.Height - size.ToSize().Height) / 2 - 1,
                bounds.Width - left, bounds.Height);

            a_draw_tip(bounds, font);

            e.DrawFocusRectangle();
        }

        public abstract void DrawItem(DrawItemEventArgs a_args);
        public abstract int ID { get; }
    }
}

using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace TomanuExtensions.Utils
{
    public static class FormsUtils
    {
        public static void CenterControlInPanel(Panel a_panel)
        {
            if (a_panel.Controls.Count != 1)
                throw new InvalidOperationException();

            Control control = a_panel.Controls[0];
            
            a_panel.AutoScroll = false;
            a_panel.AutoSize = false;

            HScrollBar horz_bar = new HScrollBar();
            VScrollBar vert_bar = new VScrollBar();

            a_panel.Controls.Add(horz_bar);
            a_panel.Controls.Add(vert_bar);

            horz_bar.Dock = DockStyle.Bottom;
            vert_bar.Dock = DockStyle.Right;

            Panel panel = new Panel();
            a_panel.Controls.Add(panel);
            panel.Dock = DockStyle.Fill;

            a_panel.Controls.Remove(control);
            panel.Controls.Add(control);

            Action on_scroll = () =>
            {
                if (panel.ClientSize.Width < control.Width)
                {
                    if (horz_bar.Value > 0)
                        control.Left = -horz_bar.Value;
                    else
                        control.Left = 0;
                }
                else
                    control.Left = (panel.ClientSize.Width - control.Width) / 2;


                if (panel.ClientSize.Height < control.Height)
                {
                    if (vert_bar.Value > 0)
                        control.Top = -vert_bar.Value;
                    else
                        control.Top = 0;
                }
                else
                    control.Top = (panel.ClientSize.Height - control.Height) / 2;

            };

            Action on_resized = () =>
            {
                if (panel.ClientRectangle.Width < control.Width)
                    horz_bar.Visible = true;
                else
                    horz_bar.Visible = false;

                if (panel.ClientRectangle.Height < control.Height)
                    vert_bar.Visible = true;
                else
                    vert_bar.Visible = false;

                if (panel.ClientRectangle.Width < control.Width)
                    horz_bar.Visible = true;
                else
                    horz_bar.Visible = false;

                vert_bar.Minimum = 0;
                vert_bar.Maximum = control.Height;
                vert_bar.LargeChange = panel.ClientRectangle.Height;

                horz_bar.Minimum = 0;
                horz_bar.Maximum = control.Width;
                horz_bar.LargeChange = panel.ClientRectangle.Width;

                if (horz_bar.LargeChange + horz_bar.Value - 1 > control.Width)
                    horz_bar.Value = control.Width - horz_bar.LargeChange + 1;
                if (vert_bar.LargeChange + vert_bar.Value - 1 > control.Height)
                    vert_bar.Value = control.Height - vert_bar.LargeChange + 1;

                on_scroll();
            };

            panel.Resize += (s, e) => on_resized();
            horz_bar.Scroll += (s, e) => on_scroll();
            vert_bar.Scroll += (s, e) => on_scroll();
            control.Resize += (s, e) => on_resized();

            on_resized();
        }
    }
}
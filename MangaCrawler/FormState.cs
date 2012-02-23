using System;
using System.Configuration;
using System.Windows.Forms;
using YAXLib;
using System.Drawing;

namespace MangaCrawler
{
    public class FormState
    {
        [YAXNode]
        private FormWindowState WindowState = FormWindowState.Normal;

        [YAXNode]
        private Rectangle Bounds;

        [YAXNode]
        private bool Saved = false;

        public event Action Changed;
  
        public void Init(Form a_form)
        {
            a_form.Load += OnFormLoad;
            a_form.FormClosing += OnFormClosing;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            Form form = sender as Form;
            if (Saved)
                form.Bounds = Bounds;
            form.WindowState = WindowState;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            Form form = sender as Form;
            WindowState = form.WindowState;

            if (form.WindowState == FormWindowState.Normal)
            {
                Bounds = form.Bounds;
                Saved = true;
            }

            if (Changed != null)
                Changed();
        }
    }
}

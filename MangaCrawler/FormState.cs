using System;
using System.Configuration;
using System.Windows.Forms;
using YAXLib;
using System.Drawing;

namespace MangaCrawler
{
    public class FormState
    {
        [YAXNode("WindowState")]
        private FormWindowState m_window_state = FormWindowState.Normal;

        [YAXNode("Bounds")]
        private Rectangle m_bounds = Rectangle.Empty;

        public event Action Changed;
  
        public void Init(Form a_form)
        {
            a_form.Load += OnFormLoad;
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            Form form = sender as Form;

            RestoreFormState(form);

            form.FormClosing += OnFormClosing;
            form.Resize += OnFormResizedOrMoved;
            form.Move += OnFormResizedOrMoved;
            form.LocationChanged += OnFormResizedOrMoved;
        }

        public void RestoreFormState(Form a_form)
        {
            if (m_bounds != Rectangle.Empty)
                a_form.Bounds = m_bounds;
            a_form.WindowState = m_window_state;
        }

        private void OnFormResizedOrMoved(object sender, EventArgs e)
        {
            SaveFormState(sender as Form);
        }

        private void SaveFormState(Form a_form)
        {
            if (a_form.WindowState == FormWindowState.Normal)
                m_bounds = a_form.Bounds;
            if (a_form.WindowState != FormWindowState.Minimized)
                m_window_state = a_form.WindowState;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            SaveFormState(sender as Form);

            if (Changed != null)
                Changed();
        }
    }
}

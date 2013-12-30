using System;
using System.Configuration;
using System.Windows.Forms;
using System.Drawing;
using System.Xml.Linq;
using TomanuExtensions;

namespace MangaCrawler
{
    public class FormState
    {
        private FormWindowState m_window_state = FormWindowState.Normal;

        private Rectangle m_bounds = Rectangle.Empty;

        public event Action Changed;

        private static string XML_FORMSTATE = "FormState";
        private static string XML_WINDOWSTATE = "WindowState";
        private static string XML_BOUNDS = "Bounds";
  
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

        internal static FormState Load(XElement a_node)
        {
            if (a_node.Name != XML_FORMSTATE)
                throw new Exception();

            return new FormState()
            {
                m_window_state = (FormWindowState)Enum.Parse(
                    typeof(FormWindowState), a_node.Element(XML_WINDOWSTATE).Value), 
                m_bounds = RectangleExtensions.FromXml(a_node.Element(XML_BOUNDS))
            };
        }

        internal XElement GetAsXml()
        {
            return new XElement(XML_FORMSTATE,
                new XElement(XML_WINDOWSTATE, m_window_state), 
                m_bounds.GetAsXml(XML_BOUNDS));
        }
    }
}

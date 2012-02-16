using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Appender;
using System.Windows.Forms;
using log4net.Util;
using log4net.Core;
using System.Drawing;
using TomanuExtensions;
using System.Diagnostics;

namespace MangaCrawler
{
    public class RichTextBoxAppender : AppenderSkeleton
    {
        private RichTextBox m_rich_text_box = null;
        private LevelMapping m_level_mapping = new LevelMapping();
        public int MaxLines = 100000;

        public RichTextBoxAppender(RichTextBox a_rich_text_box)
            : base()
        {
            m_rich_text_box = a_rich_text_box;
        }

        private void UpdateControl(LoggingEvent a_logging_event)
        {
            LevelTextStyle selectedStyle = m_level_mapping.Lookup(a_logging_event.Level) as LevelTextStyle;

            if (selectedStyle != null)
            {
                m_rich_text_box.SelectionBackColor = selectedStyle.BackColor;
                m_rich_text_box.SelectionColor = selectedStyle.TextColor;
                m_rich_text_box.SelectionFont = new Font(m_rich_text_box.Font, selectedStyle.FontStyle);
            }

            m_rich_text_box.AppendText(RenderLoggingEvent(a_logging_event));

            // Clear if too big.
            if (MaxLines > 0)
            {
                if (m_rich_text_box.Lines.Length > MaxLines)
                {
                    int pos = m_rich_text_box.GetFirstCharIndexFromLine(1);
                    m_rich_text_box.Select(0, pos);
                    m_rich_text_box.SelectedText = String.Empty;
                }
            }

            // Autoscroll.
            m_rich_text_box.Select(m_rich_text_box.TextLength, 0);
            m_rich_text_box.ScrollToCaret();
        }

        protected override void Append(LoggingEvent a_logging_event)
        {
            if (m_rich_text_box.InvokeRequired)
            {
                Action<LoggingEvent> update_action = UpdateControl;
                m_rich_text_box.BeginInvoke(update_action, a_logging_event);
            }
            else
            {
                UpdateControl(a_logging_event);
            }
        }

        public void AddMapping(LevelTextStyle a_mapping)
        {
            m_level_mapping.Add(a_mapping);
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            m_level_mapping.ActivateOptions();
        }

        protected override bool RequiresLayout 
        { 
            get 
            { 
                return true; 
            }
        }
    }

    public class LevelTextStyle : LevelMappingEntry
    {
        public bool Bold;
        public bool Italic;
        public Color TextColor;
        public Color BackColor;
        public FontStyle FontStyle { get; private set; }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            if (Bold)
                FontStyle |= FontStyle.Bold;
            if (Italic)
                FontStyle |= FontStyle.Italic;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using MangaCrawlerLib;

namespace MangaCrawler
{
    public class ListBoxEx : ListBox
    {
        private bool m_reloading;

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

        public void ReloadItems<T>(IEnumerable<T> a_enum, ListBoxState a_state = null) where T : class
        {
            if (Capture)
                return;

            BeginUpdate();
            m_reloading = true;

            var prev_state = new ListBoxState(this);

            try
            {
                if (a_state == null)
                    a_state = new ListBoxState(this);

                Items.Clear();
                Items.AddRange(a_enum.ToArray());

                a_state.Restore();
            }
            finally
            {
                EndUpdate();
                m_reloading = false;
            }

            if (a_state.SelectedItem != prev_state.SelectedItem)
                a_state.RaiseSelectionChanged();

        }
    }
}

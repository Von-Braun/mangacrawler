using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace MangaCrawler
{
    public class ListBoxEx : ListBox
    {
        private bool m_refreshing;

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (!m_refreshing)
                base.OnSelectedIndexChanged(e);
        }

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            if (!m_refreshing)
                base.OnSelectedValueChanged(e);
        }

        public new virtual void RefreshItems()
        {
            BeginUpdate();
            m_refreshing = true;

            int topIndex = IndexFromPoint(0, 0);
            int selectedIndex = SelectedIndex;

            base.RefreshItems();

            TopIndex = topIndex;

            if (selectedIndex != -1)
            {
                SetSelected(selectedIndex, false);
                SetSelected(selectedIndex, true);
            }

            EndUpdate();
            m_refreshing = false;
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

        public void ReloadItems<T>(IEnumerable<T> a_enum) where T : class
        {
            BeginUpdate();
            try
            {
                int topIndex = IndexFromPoint(0, 0);
                var sel_items = SelectedItems.Cast<object>().Intersect(a_enum).Cast<T>().ToList();

                int selectedIndex = SelectedIndex;

                Items.Clear();
                Items.AddRange(a_enum.ToArray());

                foreach (var sel_item in sel_items)
                    SetSelected(Items.IndexOf(sel_item), true);

                if (selectedIndex != -1)
                {
                    if (selectedIndex < Items.Count)
                    {
                        SetSelected(selectedIndex, false);
                        SetSelected(selectedIndex, true);
                    }
                }

                TopIndex = topIndex;
            }
            finally
            {
                EndUpdate();
            }
        }
    }
}

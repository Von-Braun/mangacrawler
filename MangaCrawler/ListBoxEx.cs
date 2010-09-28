using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

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

        public void ReloadItems<T>(IEnumerable<T> a_enum) where T : class
        {
            if (Capture)
                return;

            BeginUpdate();
            m_reloading = true;

            try
            {
                int top_index = IndexFromPoint(0, 0);
                T top_item = null;
                if (top_index != -1)
                    top_item = (T)Items[top_index];

                var sel_items = SelectedItems.Cast<object>().Intersect(a_enum).Cast<T>().ToList();

                int selected_index = SelectedIndex;
                T selected_item = (T)SelectedItem;

                Items.Clear();
                Items.AddRange(a_enum.ToArray());

                if ((top_item != null) && (a_enum.Contains(top_item)))
                    TopIndex = Items.IndexOf(top_item);
                else
                    TopIndex = top_index;

                foreach (var sel_item in sel_items)
                    SetSelected(Items.IndexOf(sel_item), true);

                if ((selected_item != null) && (a_enum.Contains(selected_item)))
                    SelectedItem = selected_item;
                else if ((selected_index != -1) && (selected_index < Items.Count))
                    SelectedIndex = selected_index;
            }
            finally
            {
                EndUpdate();
                m_reloading = false;
            }
        }
    }
}

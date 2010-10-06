using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using MangaCrawlerLib;

namespace MangaCrawler
{
    public class ListBoxState : VisualState
    {
        private ListBoxEx m_listBox;
        private int m_topIndex;
        private Object m_topItem;
        private IList<Object> m_selectedItems;
        private Object m_selectedItem;
        private int m_selectedIndex;

        public ListBoxState(ListBoxEx a_list)
        {
            m_listBox = a_list;
            SaveState();
        }

        public override string ToString()
        {
            return String.Format("selected_item: {0}; selected_index: {1}; top_item: {2}; top_index: {3}",
                       (m_selectedItem != null) ? m_selectedItem : "null",
                       m_selectedIndex, (m_topItem != null) ? m_topItem : "null", m_topIndex);
        }

        private void SaveState()
        {
            m_topIndex = m_listBox.TopIndex;
            if ((m_topIndex != -1) && (m_topIndex < m_listBox.Items.Count))
                m_topItem = m_listBox.Items[m_listBox.TopIndex];
            m_selectedItems = m_listBox.SelectedItems.Cast<Object>().ToList().AsReadOnly();
            m_selectedItem = m_listBox.SelectedItem;
            m_selectedIndex = m_listBox.SelectedIndex;
        }

        public override void Restore()
        {
            if ((m_topItem != null) && (m_listBox.Items.Contains(m_topItem)) && (m_selectedItems.Count > 0))
                m_listBox.TopIndex = m_listBox.Items.IndexOf(m_topItem);
            else if (m_topIndex < m_listBox.Items.Count)
                m_listBox.TopIndex = m_topIndex;
            else if (m_topIndex != -1)
                m_listBox.TopIndex = m_listBox.Items.Count - 1;

            foreach (var sel_item in m_selectedItems)
                m_listBox.SetSelected(m_listBox.Items.IndexOf(sel_item), true);

            if ((m_selectedItem != null) && (m_listBox.Items.Contains(m_selectedItem)))
                m_listBox.SelectedItem = m_selectedItem;
            else if (m_selectedIndex < m_listBox.Items.Count)
                m_listBox.SelectedIndex = m_selectedIndex;
            else if (m_selectedIndex != -1)
                m_listBox.SelectedIndex = m_listBox.Items.Count - 1;
        }

        protected override void Clear()
        {
            m_topIndex = -1;
            m_topItem = null;
            m_selectedItems = new Object[0];
            m_selectedItem = null;
            m_selectedIndex = -1;
        }

        public override void RaiseSelectionChanged()
        {
            m_listBox.SelectedItem = m_listBox.SelectedItem;
        }

        protected override void Update<T>(IEnumerable<T> a_newItems)
        {
            //if (a_newItems.Intersect(m_listBox.Items.Cast<Object>()).Count() == 0)
            //    Clear();
            //else
            //{
            //    if (m_selectedItems.Intersect(m_listBox.Items.Cast<Object>()).Count() > 0)
            //        SaveState();
            //    else if (m_selectedIndex == -1)
            //    {
            //        if (a_newItems.Intersect(m_listBox.Items.Cast<Object>()).Count() > 0)
            //            SaveState();
            //    }
            //}
        }

        public override void ReloadItems<T>(IEnumerable<T> a_enum)
        {
            var prev_state = new ListBoxState(m_listBox);

            Update(a_enum);
            m_listBox.ReloadItems(a_enum, this);

            if (m_selectedItem != prev_state.m_selectedItem)
                RaiseSelectionChanged();
        }
    }
}

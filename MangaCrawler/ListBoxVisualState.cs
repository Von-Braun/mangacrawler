using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using MangaCrawlerLib;
using System.Xml.Linq;
using System.Xml;
using TomanuExtensions;

namespace MangaCrawler
{
    public class ListBoxVisualState
    {
        private ListBoxEx m_list_box;
        private int m_top_index;
        private ListItem m_top_item;
        private ListItem[] m_selected_items;
        private ListItem m_selected_item;
        private int m_selected_index;

        public ListBoxVisualState(ListBoxEx a_list)
        {
            m_list_box = a_list;
            SaveState();
        }

        public override string ToString()
        {
            return String.Format("selected_item: {0}; selected_index: {1}; top_item: {2}; top_index: {3}",
                (m_selected_item != null) ? m_selected_item.ToString() : "null",
                m_selected_index, (m_top_item != null) ? m_top_item.ToString() : "null", m_top_index);
        }

        private void SaveState()
        {
            m_top_index = m_list_box.TopIndex;
            if ((m_top_index != -1) && (m_top_index < m_list_box.Items.Count))
                m_top_item = m_list_box.Items[m_list_box.TopIndex] as ListItem;
            m_selected_items = m_list_box.SelectedItems.Cast<ListItem>().ToArray();
            m_selected_item = m_list_box.SelectedItem as ListItem;
            m_selected_index = m_list_box.SelectedIndex;
        }

        public void Restore()
        {
            foreach (var sel_item in m_selected_items)
            {
                int index = m_list_box.Items.IndexOf(sel_item);

                if (index != -1)
                    m_list_box.SetSelected(index, true);
            }

            if ((m_selected_item != null) && (m_list_box.Items.Contains(m_selected_item)))
                m_list_box.SelectedItem = m_selected_item;
            else if (m_selected_index < m_list_box.Items.Count)
                m_list_box.SelectedIndex = m_selected_index;
            else if (m_selected_index != -1)
                m_list_box.SelectedIndex = m_list_box.Items.Count - 1;

            if ((m_top_item != null) && (m_list_box.Items.Contains(m_top_item)) && (m_selected_items.Length > 0))
                m_list_box.TopIndex = m_list_box.Items.IndexOf(m_top_item);
            else if (m_top_index < m_list_box.Items.Count)
                m_list_box.TopIndex = m_top_index;
            else if (m_top_index != -1)
                m_list_box.TopIndex = m_list_box.Items.Count - 1;
        }

        public void Clear()
        {
            m_top_index = -1;
            m_top_item = null;
            m_selected_items = new ListItem[0];
            m_selected_item = null;
            m_selected_index = -1;
        }

        public void ReloadItems(IEnumerable<ListItem> a_enum)
        {
            m_list_box.ReloadItems(a_enum, this);
        }
    }
}

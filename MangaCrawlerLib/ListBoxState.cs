using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MangaCrawlerLib
{
    // TODO: dodac poziom abstrakcji.
    public class ListBoxState
    {
        public readonly ListBox ListBox;
        public readonly int TopIndex;
        public readonly Object TopItem;
        public readonly IList<Object> SelectedItems;
        public readonly Object SelectedItem;
        public readonly int SelectedIndex;

        public ListBoxState(ListBox a_list)
        {
            ListBox = a_list;
            TopIndex = a_list.TopIndex;
            if (TopIndex != -1)
                TopItem = a_list.Items[a_list.TopIndex];
            SelectedItems = a_list.SelectedItems.Cast<Object>().ToList().AsReadOnly();
            SelectedItem = a_list.SelectedItem;
            SelectedIndex = a_list.SelectedIndex;
        }

        public void Restore()
        {
            if ((TopItem != null) && (ListBox.Items.Contains(TopItem)))
                ListBox.TopIndex = ListBox.Items.IndexOf(TopItem);
            else if (TopIndex < ListBox.Items.Count)
                ListBox.TopIndex = TopIndex;
            else if (TopIndex != -1)
                ListBox.TopIndex = ListBox.Items.Count - 1;

            foreach (var sel_item in SelectedItems)
                ListBox.SetSelected(ListBox.Items.IndexOf(sel_item), true);

            if ((SelectedItem != null) && (ListBox.Items.Contains(SelectedItem)))
                ListBox.SelectedItem = SelectedItem;
            else if (SelectedIndex < ListBox.Items.Count)
                ListBox.SelectedIndex = SelectedIndex;
            else if (SelectedIndex != -1)
                ListBox.SelectedIndex = ListBox.Items.Count - 1;
        }

        public void RaiseSelectionChanged()
        {
            ListBox.SelectedItem = ListBox.SelectedItem;
        }
    }
}

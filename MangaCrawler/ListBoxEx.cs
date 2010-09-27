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
    }
}

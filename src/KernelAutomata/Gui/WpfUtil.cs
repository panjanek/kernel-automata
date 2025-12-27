using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ComboBox = System.Windows.Controls.ComboBox;

namespace KernelAutomata.Gui
{
    public static class WpfUtil
    {
        public static string GetComboSelectionAsString(ComboBox combo)
        {
            if (combo.SelectedItem is ComboBoxItem)
            {
                var item = (ComboBoxItem)combo.SelectedItem;
                return item.Content?.ToString();
            }

            return null;
        }

        public static void SetComboStringSelection(ComboBox combo, string value)
        {
            foreach (var item in combo.Items)
            {
                if (item is ComboBoxItem)
                {
                    var comboItem = item as ComboBoxItem;
                    comboItem.IsSelected = comboItem.Content?.ToString() == value;
                }
            }
        }
    }
}

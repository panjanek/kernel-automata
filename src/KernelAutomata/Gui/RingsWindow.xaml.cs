using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KernelAutomata.Models;
using KernelAutomata.Utils;

namespace KernelAutomata.Gui
{
    /// <summary>
    /// Interaction logic for KernelConfig.xaml
    /// </summary>
    public partial class RingsWindow : Window
    {
        private KernelRecipe recipe;

        private Action dataChanged;

        public RingsWindow(KernelRecipe recipe, Action dataChanged)
        {
            this.recipe = recipe;
            this.dataChanged = dataChanged;
            DataContext = recipe;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            closeButton.Click += (s, e) => Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void RingConfigurator_DataCommitted(object sender, RoutedEventArgs e) => dataChanged();
    }
}

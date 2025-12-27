using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace KernelAutomata.Gui
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private MainWindow mainWindow;
        public ConfigWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            minimizeButton.Click += (s,e) => WindowState = WindowState.Minimized;
            Closing += (s, e) => { e.Cancel = true; WindowState = WindowState.Minimized; };
            ContentRendered += (s, e) => {  };
            Loaded += (s, e) => { };
            
        }

        private void global_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fieldSize != null && channelsCount != null)
            {
                var sizeStr = WpfUtil.GetComboSelectionAsString(fieldSize);
                var channlesCountStr = WpfUtil.GetComboSelectionAsString(channelsCount);
                if (!string.IsNullOrWhiteSpace(sizeStr) && !string.IsNullOrWhiteSpace(channlesCountStr))
                {
                    int newSize = int.Parse(sizeStr.Split('x')[0]);
                    int newChannelsCount = int.Parse(channlesCountStr);
                    var recipe = mainWindow.recipe;
                    if (recipe.channels.Length != newChannelsCount)
                    {
                        if (newChannelsCount == 1)
                            recipe = RecipeFactory.LoadFromResource("orbs-ch1.json");
                        else
                            recipe = RecipeFactory.LoadFromResource("orbs-ch2.json");
                    }

                    recipe.size = newSize;
                    mainWindow.StartNewSimulation(recipe);

                }
            }
        }
    }
}

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
using KernelAutomata.Utils;
using AppContext = KernelAutomata.Models.AppContext;

namespace KernelAutomata.Gui
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private AppContext app;
        public ConfigWindow(AppContext app)
        {
            this.app = app;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            minimizeButton.Click += (s,e) => WindowState = WindowState.Minimized;
            Closing += (s, e) => { e.Cancel = true; WindowState = WindowState.Minimized; };
            ContentRendered += (s, e) => { UpdateControls(app.recipe); };
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
                    var recipe = app.recipe;
                    if (recipe.channels.Length != newChannelsCount)
                    {
                        if (newChannelsCount == 1)
                            recipe = RecipeFactory.LoadFromResource("orbs-ch1.json");
                        else
                            recipe = RecipeFactory.LoadFromResource("caterpillar1-ch2.json");
                    }

                    recipe.size = newSize;
                    app.StartNewSimulation(recipe);
                    UpdateControls(recipe);

                }
            }
        }

        private void UpdateControls(SimulationRecipe recipe)
        {
            WpfUtil.SetComboStringSelection(fieldSize, $"{recipe.size}x{recipe.size}");
            WpfUtil.SetComboStringSelection(channelsCount, recipe.channels.Length.ToString());
            foreach (var slider in WpfUtil.FindVisualChildren<Slider>(this))
            {
                WpfUtil.AddTooltipToSlider(slider);
                var tag = WpfUtil.GetTagAsString(slider);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    slider.Value = ReflectionUtil.GetObjectValue<float>(recipe, tag);

                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var tag = WpfUtil.GetTagAsString(sender);
            if (!string.IsNullOrWhiteSpace(tag))
            {
                ReflectionUtil.SetObjectValue<float>(app.recipe, tag, (float)e.NewValue);
                app.simulation.UpdateRecipe(app.recipe);
                app.simulation.ResetFields();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
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
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using AppContext = KernelAutomata.Models.AppContext;
using Channel = KernelAutomata.Models.Channel;
using Window = System.Windows.Window;

namespace KernelAutomata.Gui
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private AppContext app;

        private bool updating;

        public ConfigWindow(AppContext app)
        {
            this.app = app;
            DataContext = new SimulationDataContext(app);
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            minimizeButton.Click += (s,e) => WindowState = WindowState.Minimized;
            Closing += (s, e) => { e.Cancel = true; WindowState = WindowState.Minimized; };
            ContentRendered += (s, e) => { UpdateActiveControls(app.recipe); UpdatePassiveControls(app.recipe); };
            Loaded += (s, e) => { };
            
        }

        private void global_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fieldSize != null && channelsCount != null && !updating)
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

                    WpfUtil.FindVisualChildren<KernelConfigurator>(this).ToList().ForEach(k => k.CloseRingsWindow());
                    recipe.size = newSize;
                    app.StartNewSimulation(recipe);
                    DataContext = new SimulationDataContext(app);
                    UpdateActiveControls(recipe);
                    UpdatePassiveControls(recipe);
                }
            }
        }

        private void UpdateActiveControls(SimulationRecipe recipe)
        {
            updating = true;
            WpfUtil.SetComboStringSelection(fieldSize, $"{recipe.size}x{recipe.size}");
            WpfUtil.SetComboStringSelection(channelsCount, recipe.channels.Length.ToString());
            foreach (var slider in WpfUtil.FindVisualChildren<Slider>(this))
            {
                if (!WpfUtil.CheckIfPathContains<KernelConfigurator>(slider))
                {
                    WpfUtil.AddTooltipToSlider(slider);
                    var tag = WpfUtil.GetTagAsString(slider);
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        slider.Value = ReflectionUtil.GetObjectValue<float>(recipe, tag);
                    }
                }
            }
            updating = false;
        }

        private void UpdatePassiveControls(SimulationRecipe recipe)
        {
            foreach (var text in WpfUtil.FindVisualChildren<TextBlock>(this))
            {
                if (!WpfUtil.CheckIfPathContains<KernelConfigurator>(text))
                {
                    var tag = WpfUtil.GetTagAsString(text);
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        var value = ReflectionUtil.GetObjectValue<float>(app.recipe, tag);
                        text.Text = value.ToString("0.000", CultureInfo.InvariantCulture);

                    }
                }
            }

            foreach (var graph in WpfUtil.FindVisualChildren<FunctionGraph>(this))
            {
                if (!WpfUtil.CheckIfPathContains<KernelConfigurator>(graph))
                {
                    var tag = WpfUtil.GetTagAsString(graph);
                    var tagSplit = tag.Split('.');
                    if (tagSplit.Length == 2)
                    {
                        var channel = ReflectionUtil.GetObjectValue<ChannelRecipe>(recipe, tag);
                        if (channel != null)
                            graph.Draw(200, 0, 1, x => channel.GrowthFunction(x));
                        else
                            graph.Children.Clear();
                    }
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!updating)
            {
                var tag = WpfUtil.GetTagAsString(sender);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    ReflectionUtil.SetObjectValue<float>(app.recipe, tag, (float)e.NewValue);
                    app.simulation.UpdateRecipe(app.recipe);
                    app.simulation.ResetFields();
                    UpdatePassiveControls(app.recipe);
                }
            }
        }

        private void infoText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tag = WpfUtil.GetTagAsString(sender);
            if (!string.IsNullOrWhiteSpace(tag))
                WpfUtil.FindVisualChildren<Slider>(this).Where(s => WpfUtil.GetTagAsString(s) == tag).FirstOrDefault()?.Focus();
        }

        private void Configurator_DataChanged(object sender, RoutedEventArgs e)
        {
            app.simulation.UpdateRecipe(app.recipe);
            app.simulation.ResetFields();
            UpdatePassiveControls(app.recipe);
        }
    }
}

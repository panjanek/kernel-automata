using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KernelAutomata.Models;
using KernelAutomata.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
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

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog { Title = "Open configuration json file", DefaultExtension = "json" };
            dialog.Filters.Add(new CommonFileDialogFilter("JSON files", "*.json"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var newRecipe = RecipeFactory.LoadFromFile(dialog.FileName);
                app.StartNewSimulation(newRecipe);
                DataContext = new SimulationDataContext(app);
                UpdateActiveControls(newRecipe);
                UpdatePassiveControls(newRecipe);
                PopupMessage.Show(app.mainWindow , $"Config loaded from {dialog.FileName}");
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonSaveFileDialog { Title = "Save configuration json file", DefaultExtension = "json" };
            dialog.Filters.Add(new CommonFileDialogFilter("JSON files", "*.json"));
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RecipeFactory.SaveToFile(app.recipe, dialog.FileName);
                PopupMessage.Show(app.mainWindow, $"Config saved to {dialog.FileName}");
            }
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            app.simulation.ResetFields();
            UpdatePassiveControls(app.recipe);
            PopupMessage.Show(app.mainWindow, $"Simulation restarted");
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
                if (!WpfUtil.CheckIfPathContains<KernelConfigurator>(text))
                    WpfUtil.UpdateTextBlockForSlider(this, text, app.recipe);

            ToggleVisibility();
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

        private void ToggleVisibility()
        {
            foreach (var ch in contentGrid.Children)
            {
                if (ch is FrameworkElement)
                {
                    var element = (FrameworkElement)ch;
                    var row = (int)element.GetValue(Grid.RowProperty);
                    var column = (int)element.GetValue(Grid.ColumnProperty);
                    if (app.recipe.channels.Length == 1)
                    {
                        Width = 250 + 35;
                        Height = 590 + 45;
                        if (column >= 3 || row >= 14)
                        {
                            element.Visibility = Visibility.Collapsed;
                            
                        }
                    }
                    else if (app.recipe.channels.Length == 2)
                    {
                        Width = 420 + 30;
                        Height = 840 + 45;
                        element.Visibility = Visibility.Visible;
                        
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
                    UpdateSimulationWithRecipe();
                }
            }
        }

        private void Configurator_DataChanged(object sender, RoutedEventArgs e) => UpdateSimulationWithRecipe();

        private void infoText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tag = WpfUtil.GetTagAsString(sender);
            if (!string.IsNullOrWhiteSpace(tag))
                WpfUtil.FindVisualChildren<Slider>(this).Where(s => WpfUtil.GetTagAsString(s) == tag).FirstOrDefault()?.Focus();
        }

        private void UpdateSimulationWithRecipe()
        {
            app.simulation.UpdateRecipe(app.recipe);
            if (autoRestart.IsChecked == true)
                app.simulation.ResetFields();
            UpdatePassiveControls(app.recipe);
        }

        private void StartingConditions_Click(object sender, RoutedEventArgs e)
        {
            var tag = WpfUtil.GetTagAsString(sender);
            var channel = ReflectionUtil.GetObjectValue<ChannelRecipe>(app.recipe, tag);
            var aaa = channel;
        }
    }
}

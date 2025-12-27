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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using AppContext = KernelAutomata.Models.AppContext;
using Window = System.Windows.Window;

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
            ContentRendered += (s, e) => { UpdateActiveControls(app.recipe); UpdatePassiveControls(app.recipe); };
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
                    UpdateActiveControls(recipe);
                    UpdatePassiveControls(recipe);
                }
            }
        }

        private void UpdateActiveControls(SimulationRecipe recipe)
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

        private void UpdatePassiveControls(SimulationRecipe recipe)
        {
            foreach (var graph in WpfUtil.FindVisualChildren<FunctionGraph>(this))
            {
                var tag = WpfUtil.GetTagAsString(graph);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    var tagSplit = tag.Split('.');
                    if (tagSplit.Length == 2)
                    {
                        var channel = ReflectionUtil.GetObjectValue<ChannelRecipe>(recipe, tag);
                        if (channel != null)
                            graph.Draw(200, 0, 1, x => channel.GrowthFunction(x));
                        else
                            graph.Children.Clear();
                    }
                    else if (tagSplit.Length == 4)
                    {
                        var kernelRec = ReflectionUtil.GetObjectValue<KernelRecipe>(recipe, tag);
                        if (kernelRec != null)
                        {
                            int channelIdx = int.Parse(tagSplit[1]);
                            int kernelIdx = int.Parse(tagSplit[3]);
                            var kernel = app.simulation.channels[channelIdx].kernels[kernelIdx];
                            var globalMaxR = app.recipe.channels.SelectMany(c => kernel.rings.Where(r => r.weight != 0).Select(r => (int)Math.Ceiling(r.maxR))).Max();
                            var intersection = new double[globalMaxR];
                            for (int x = 0; x < intersection.Length; x++)
                                intersection[x] = kernel.kernelBuffer[x * 4] * 1000;
                            graph.Draw(globalMaxR, 0, globalMaxR, x =>
                            {
                                int ix = (int)x;
                                if (ix < 0) ix = 0;
                                if (ix >= intersection.Length) ix = intersection.Length - 1;
                                return intersection[ix];
                            });

                        }
                    }

                }
            }

            foreach (var image in WpfUtil.FindVisualChildren<KernelImage>(this))
            {
                var tag = WpfUtil.GetTagAsString(image);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    var tagSplit = tag.Split('.');
                    if (tagSplit.Length == 4)
                    {
                        var kernelRec = ReflectionUtil.GetObjectValue<KernelRecipe>(recipe, tag);
                        if (kernelRec != null)
                        {
                            int channelIdx = int.Parse(tagSplit[1]);
                            int kernelIdx = int.Parse(tagSplit[3]);
                            var kernel = app.simulation.channels[channelIdx].kernels[kernelIdx];
                            var globalMaxR = app.recipe.channels.SelectMany(c => kernel.rings.Where(r => r.weight != 0).Select(r => (int)Math.Ceiling(r.maxR))).Max();
                            image.Draw(kernel.kernelBuffer, kernel.fieldSize, globalMaxR);
                        }
                    }
                }
            }

            foreach (var text in WpfUtil.FindVisualChildren<TextBlock>(this))
            {
                var tag = WpfUtil.GetTagAsString(text);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    var value = ReflectionUtil.GetObjectValue<float>(app.recipe, tag);
                    text.Text = value.ToString("0.000", CultureInfo.InvariantCulture);

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
                UpdatePassiveControls(app.recipe);
            }
        }

        private void infoText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tag = WpfUtil.GetTagAsString(sender);
            if (!string.IsNullOrWhiteSpace(tag))
                WpfUtil.FindVisualChildren<Slider>(this).Where(s => WpfUtil.GetTagAsString(s) == tag).FirstOrDefault()?.Focus();
        }
    }
}

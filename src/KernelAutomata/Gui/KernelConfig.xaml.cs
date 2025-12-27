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
    public partial class KernelConfig : Window
    {
        private KernelRecipe recipe;

        private Action dataChanged;

        private bool updating = false;
        public KernelConfig(KernelRecipe recipe, Action dataChanged)
        {
            this.recipe = recipe;
            this.dataChanged = dataChanged;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            closeButton.Click += (s, e) => Close();
            ContentRendered += (s, e) => { UpdateActiveControls(); UpdatePassiveControls(); };
        }

        private void UpdateActiveControls()
        {
            updating = true;
            foreach (var slider in WpfUtil.FindVisualChildren<Slider>(this))
            {
                WpfUtil.AddTooltipToSlider(slider);
                var tag = WpfUtil.GetTagAsString(slider);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    slider.Value = ReflectionUtil.GetObjectValue<float>(recipe, tag);
                }
            }
            updating = false;
        }

        private void UpdatePassiveControls()
        {
            foreach (var text in WpfUtil.FindVisualChildren<TextBlock>(this))
            {
                var tag = WpfUtil.GetTagAsString(text);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    var value = ReflectionUtil.GetObjectValue<float>(recipe, tag);
                    text.Text = value.ToString("0.000", CultureInfo.InvariantCulture);

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
                    ReflectionUtil.SetObjectValue<float>(recipe, tag, (float)e.NewValue);
                    UpdatePassiveControls();
                    dataChanged();
                }
            }
        }

        private void infoText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tag = WpfUtil.GetTagAsString(sender);
            if (!string.IsNullOrWhiteSpace(tag))
                WpfUtil.FindVisualChildren<Slider>(this).Where(s => WpfUtil.GetTagAsString(s) == tag).FirstOrDefault()?.Focus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

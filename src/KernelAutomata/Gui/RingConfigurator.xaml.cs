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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KernelAutomata.Models;
using KernelAutomata.Utils;
using UserControl = System.Windows.Controls.UserControl;

namespace KernelAutomata.Gui
{
    /// <summary>
    /// Interaction logic for RingConfigurator.xaml
    /// </summary>
    public partial class RingConfigurator : UserControl
    {
        public static readonly RoutedEvent DataCommittedEvent =
           EventManager.RegisterRoutedEvent(
        nameof(DataCommitted),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(RingConfigurator));

        public event RoutedEventHandler DataCommitted
        {
            add { AddHandler(DataCommittedEvent, value); }
            remove { RemoveHandler(DataCommittedEvent, value); }
        }


        private RingRecipe recipe;

        private bool updating = false;
        public RingConfigurator()
        {
            InitializeComponent();
            Loaded += (s, e) => { UpdateActiveControls(); UpdatePassiveControls(); };
            DataContextChanged += (s, e) =>
            {
                if (DataContext is RingRecipe)
                {
                    this.recipe = (RingRecipe)DataContext;
                    UpdateActiveControls();
                    UpdatePassiveControls();
                }
            };
        }

        private void UpdateActiveControls()
        {
            var a = DataContext;
            if (recipe != null)
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
        }

        private void UpdatePassiveControls()
        {
            if (recipe != null)
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
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!updating && recipe!=null)
            {
                var tag = WpfUtil.GetTagAsString(sender);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    ReflectionUtil.SetObjectValue<float>(recipe, tag, (float)e.NewValue);
                    UpdatePassiveControls();
                    RaiseEvent(new RoutedEventArgs(DataCommittedEvent));
                }
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

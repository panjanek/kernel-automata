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
using System.Windows.Navigation;
using System.Windows.Shapes;
using KernelAutomata.Models;
using KernelAutomata.Utils;

namespace KernelAutomata.Gui
{
    /// <summary>
    /// Interaction logic for KernelConfigurator.xaml
    /// </summary>
    public partial class KernelConfigurator : System.Windows.Controls.UserControl
    {
        public static readonly RoutedEvent DataCommittedEvent =
               EventManager.RegisterRoutedEvent(
            nameof(KernelDataCommitted),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(RingConfigurator));

        public event RoutedEventHandler KernelDataCommitted
        {
            add { AddHandler(DataCommittedEvent, value); }
            remove { RemoveHandler(DataCommittedEvent, value); }
        }

        private KernelDataContext KernelContext => DataContext is KernelDataContext ? (KernelDataContext)DataContext : null;

        private bool updating = false;

        private RingsWindow ringsWindow;

        public KernelConfigurator()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                if (KernelContext != null)
                {
                    UpdateActiveControls();
                    UpdatePassiveControls();
                }
            };

            Loaded += (s, e) =>
            {
                UpdatePassiveControls();
            };
        }

        private void UpdateActiveControls()
        {
            if (KernelContext != null && KernelContext.IsActive)
            {
                updating = true;
                foreach (var slider in WpfUtil.FindVisualChildren<Slider>(this))
                {
                    WpfUtil.AddTooltipToSlider(slider);
                    var tag = WpfUtil.GetTagAsString(slider);
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        slider.Value = ReflectionUtil.GetObjectValue<float>(KernelContext.Recipe, tag);
                    }
                }
                updating = false;
            }
        }

        private void UpdatePassiveControls()
        {
            if (KernelContext != null && KernelContext.IsActive)
            {
                foreach (var text in WpfUtil.FindVisualChildren<TextBlock>(this))
                    WpfUtil.UpdateTextBlockForSlider(this, text, KernelContext.Recipe);

                var kernel = KernelContext.Kernel;
                var globalMaxR = KernelContext.Recipe.rings.Select(r => (int)Math.Ceiling(r.maxR)).Max();
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

                image.Draw(kernel.kernelBuffer, kernel.fieldSize, globalMaxR);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!updating && KernelContext != null)
            {
                var tag = WpfUtil.GetTagAsString(sender);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    ReflectionUtil.SetObjectValue<float>(KernelContext.Recipe, tag, (float)e.NewValue);
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

        private void EditButton_Click(object sender, RoutedEventArgs e) => OpenKernelConfigurationDialog();

        private void KernelGraph_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => OpenKernelConfigurationDialog();

        private void OpenKernelConfigurationDialog()
        {
            WpfUtil.FindVisualChildren<KernelConfigurator>(Parent).ToList().ForEach(k => k.CloseRingsWindow());
            if (ringsWindow != null)
                ringsWindow.Close();
            ringsWindow = new RingsWindow(KernelContext.Recipe, () =>
            {
                UpdatePassiveControls();
                RaiseEvent(new RoutedEventArgs(DataCommittedEvent));
            });
            ringsWindow.Show();
        }

        public void CloseRingsWindow()
        {
            if (ringsWindow != null)
            {
                ringsWindow.Close();
                ringsWindow = null;
            }
        }
    }
}

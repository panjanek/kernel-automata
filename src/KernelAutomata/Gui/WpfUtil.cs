using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using KernelAutomata.Utils;
using OpenTK.Graphics.OpenGL;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Orientation = System.Windows.Controls.Orientation;
using TextBox = System.Windows.Controls.TextBox;
using ToolTip = System.Windows.Controls.ToolTip;

namespace KernelAutomata.Gui
{
    public static class WpfUtil
    {
        public static void AddShortcutsToAllSliders(FrameworkElement parent, Action<object, RoutedPropertyChangedEventArgs<double>> changed)
        {
            foreach (var slider in WpfUtil.FindVisualChildren<Slider>(parent))
                AddShortcutsToSlider(slider, changed);
        }

        public static void AddShortcutsToAllSlidersExluding<TExclude>(FrameworkElement parent, Action<object, RoutedPropertyChangedEventArgs<double>> changed)
        {
            foreach (var slider in WpfUtil.FindVisualChildren<Slider>(parent))
                if (!WpfUtil.CheckIfPathContains<TExclude>(slider))
                    AddShortcutsToSlider(slider, changed);
        }

        public static void AddShortcutsToSlider(Slider slider, Action<object, RoutedPropertyChangedEventArgs<double>> changed)
        {
            slider.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.D0)
                {
                    var oldValue = slider.Value;
                    if (slider.Minimum <= 0 && slider.Maximum >= 0)
                        slider.Value = 0;
                    else
                        slider.Value = slider.Minimum;

                    var ev = new RoutedPropertyChangedEventArgs<double>(oldValue, slider.Value);
                    changed(s, ev);
                }
            };
        }

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

        public static string GetTagAsString(object element)
        {
            if (element is FrameworkElement)
            {
                var el = (FrameworkElement)element;
                if (el.Tag is string)
                    return el.Tag as string;
                else
                    return null;
            }
            else
                return null;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
        {
            if (parent == null)
                yield break;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T t)
                    yield return t;

                foreach (var descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        public static void AddTooltipToSlider(Slider slider)
        {
            if (slider.ToolTip == null)
            {
                var textBlock = new TextBlock();

                textBlock.SetBinding(TextBlock.TextProperty, new Binding("Value")
                {
                    Source = slider,
                    StringFormat = "0.000"
                });


                var toolTip = new ToolTip
                {
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                    StaysOpen = true,
                    Content = textBlock
                };

                slider.ToolTip = toolTip;
                slider.PreviewMouseLeftButtonDown += (_, _) => toolTip.IsOpen = true;
                slider.PreviewMouseLeftButtonUp += (_, _) => toolTip.IsOpen = false;
            }
        }

        public static bool CheckIfPathContains<T>(FrameworkElement element)
        {
            List<FrameworkElement> path = new List<FrameworkElement>();
            while (element.Parent is FrameworkElement && element.Parent!=null)
            {
                path.Add(element);
                element = (FrameworkElement)element.Parent;
            }

            return path.Any(p=>p is T);
        }

        public static void UpdateTextBlockForSlider(FrameworkElement parent, TextBlock text, object recipe)
        {
            var tag = WpfUtil.GetTagAsString(text);
            if (!string.IsNullOrWhiteSpace(tag))
            {
                string format = "0.000";
                var slider = WpfUtil.FindVisualChildren<Slider>(parent).Where(s => !WpfUtil.CheckIfPathContains<KernelConfigurator>(s)).FirstOrDefault(s => WpfUtil.GetTagAsString(s) == tag);
                if (slider != null)
                {
                    switch (slider.SmallChange)
                    {
                        case 1:
                            format = "0";
                            break;
                        case 0.1:
                            format = "0.0";
                            break;
                        case 0.01:
                            format = "0.00";
                            break;
                        case 0.001:
                            format = "0.000";
                            break;
                        case 0.0001:
                            format = "0.0000";
                            break;
                    }
                }

                var value = ReflectionUtil.GetObjectValue<float>(recipe, tag);
                text.Text = value.ToString(format, CultureInfo.InvariantCulture);
                text.Background = Brushes.Black;
                text.Foreground = Brushes.White;
            }
        }

        public static void ShowAboutDialog()
        {
            // Window
            Window dialog = new Window()
            {
                Width = 400,
                Height = 200,
                Title = "KernelAutomata by panjanek",
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                Owner = Application.Current.MainWindow
            };

            // Layout
            StackPanel panel = new StackPanel() { Margin = new Thickness(10) };
            TextBlock txt = new TextBlock() { Margin = new Thickness(0, 0, 0, 10), TextWrapping = TextWrapping.Wrap };
            txt.Inlines.Add(new Run("KernelAutomata by panjanek\n\n"));
            txt.Inlines.Add(new Run("Exploring artificial life emerging from continous cellular automata, extension of Conway's \"Game of Life.\n\n"));
            txt.Inlines.Add(new Run("Read more: "));
            var link = new Hyperlink(new Run("https://github.com/panjanek/kernel-automata"))  { NavigateUri = new Uri("https://github.com/panjanek/kernel-automata") };
            link.RequestNavigate += (s, e) => { Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });  };
            txt.Inlines.Add(link);
            txt.Inlines.Add(new Run("\n\nThings to try: loading different presets, rightclick context menu on kernel configuration, fullscreen..."));
            StackPanel buttonPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Button ok = new Button() { Content = "OK", Width = 70, Margin = new Thickness(5, 0, 0, 0) };
            ok.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };

            buttonPanel.Children.Add(ok);

            // Compose UI
            panel.Children.Add(txt);
            panel.Children.Add(buttonPanel);

            dialog.Content = panel;

            // Show input dialog
            dialog.ShowDialog();
        }
    }
}

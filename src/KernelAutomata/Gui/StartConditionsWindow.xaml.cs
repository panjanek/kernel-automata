using System;
using System.Collections.Generic;
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
    /// Interaction logic for StartConditionsWindow.xaml
    /// </summary>
    public partial class StartConditionsWindow : Window
    {
        public const int ImageSize = 200;

        private ChannelRecipe recipe;

        private Action dataChanged;

        private bool updating;

        private float[] initBuffer = new float[ImageSize*ImageSize*4];

        private byte[] pixels = new byte[ImageSize * ImageSize * 4];

        private int channelIndex;
        public StartConditionsWindow(int channelIdx, ChannelRecipe recipe, Action dataChanged)
        {
            this.recipe = recipe;
            this.dataChanged = dataChanged;
            this.channelIndex = channelIdx;
            DataContext = recipe;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            ContentRendered += (s, e) => { UpdateActiveControls(); UpdatePassiveControls(); };
            closeButton.Click += (s, e) => Close();
            Loaded += (s, e) => { WpfUtil.AddShortcutsToAllSliders(this, (s, e) => Slider_ValueChanged(s, e)); };
        }

        private void UpdateActiveControls()
        {
            updating = true;
            foreach (var slider in WpfUtil.FindVisualChildren<Slider>(this))
            {
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
            Title = $"Starting conditions for channel {channelIndex}";
            titleBox.Text = Title;
            foreach (var text in WpfUtil.FindVisualChildren<TextBlock>(this))
                    WpfUtil.UpdateTextBlockForSlider(this, text, recipe);
            recipe.initialization.FillInitBufferWithRandomData(ImageSize, initBuffer);
            int r = (channelIndex == 0 || channelIndex == 2) ? 1 : 0;
            int g = (channelIndex == 1 || channelIndex == 2) ? 1 : 0;
            int b = (channelIndex == 0 || channelIndex == 1) ? 1 : 0;
            for (int i=0; i<initBuffer.Length/4; i++)
            {

                pixels[i * 4 + 0] = (byte)(b * 255 * initBuffer[i * 4 + 0]);
                pixels[i * 4 + 1] = (byte)(g * 255 * initBuffer[i * 4 + 0]);
                pixels[i * 4 + 2] = (byte)(r * 255 * initBuffer[i * 4 + 0]);
                pixels[i * 4 + 3] = (byte)(255);
            }

            image.DrawRaw(ImageSize, pixels);
        }

        public void SetTitle(string title)
        {

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!updating)
            {
                var tag = WpfUtil.GetTagAsString(sender);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    ReflectionUtil.SetObjectValue<float>(recipe, tag, (float)e.NewValue);
                    dataChanged();
                    UpdatePassiveControls();
                }
            }
        }

        private void infoText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tag = WpfUtil.GetTagAsString(sender);
            if (!string.IsNullOrWhiteSpace(tag))
                WpfUtil.FindVisualChildren<Slider>(this).Where(s => WpfUtil.GetTagAsString(s) == tag).FirstOrDefault()?.Focus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}

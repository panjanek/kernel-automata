using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Image = System.Windows.Controls.Image;

namespace KernelAutomata.Gui
{
    public class KernelImage : Image
    {
        public KernelImage()
            : base()
        {
            
        }

        public void Draw(float[] buffer, int fieldSize, int maxR)
        {
            var a = Width;
            var bb = ActualWidth;
            var cc = ActualHeight;
            int imageSize = 2 * maxR;
            var pixels = new byte[imageSize * imageSize * 4];
            double minVal = 1000000000;
            double maxVal = -1000000000;
            for(int x=0; x<imageSize; x++)
            {
                for(int y=0; y<imageSize; y++)
                {
                    double val = buffer[4*(y * fieldSize + x)];
                    if (val < minVal)
                        minVal = val;
                    if (val > maxVal)
                        maxVal = val;
                }
            }

            for (int x = 0; x < imageSize; x++)
            {
                for (int y = 0; y < imageSize; y++)
                {
                    double val = buffer[4*(y * fieldSize + x)];
                    int r = 0;
                    int b = 0;
                    if (val < 0)
                        b = (int)Math.Round(-val * 255.0 / (-minVal));
                    else
                        r = (int)Math.Round(val * 255 / maxVal);

                    SetPixel(pixels, imageSize, maxR + x, maxR + y, r, b);
                    SetPixel(pixels, imageSize, maxR - x, maxR + y, r, b);
                    SetPixel(pixels, imageSize, maxR + x, maxR - y, r, b);
                    SetPixel(pixels, imageSize, maxR - x, maxR - y, r, b);
                }
            }

            var bitmap = new WriteableBitmap(imageSize, imageSize, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, imageSize, imageSize), pixels, imageSize*4, 0);
            Source = bitmap;
        }

        private void SetPixel(byte[] pixels, int size, int x, int y, int r, int b)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                pixels[(y * size + x) * 4 + 0] = (byte)b;
                pixels[(y * size + x) * 4 + 1] = 0;
                pixels[(y * size + x) * 4 + 2] = (byte)r;
                pixels[(y * size + x) * 4 + 3] = 255;
            }
        }
    }
}

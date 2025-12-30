using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using OpenTK.Graphics.ES11;
using Brush = System.Windows.Media.Brush;

namespace KernelAutomata.Gui
{
    public class FunctionGraph : Canvas
    {
        public void Draw(int steps, double startX, double endX, Func<double, double> func, bool showIntegrals = false)
        {
            try
            {
                var width = ActualWidth;
                var height = ActualHeight;
                Children.Clear();
                Background = Brushes.Black;
                ClipToBounds = true;
                double minY = 1000000000;
                double maxY = -1000000000;
                double positiveSum = 0;
                double negativeSum = 0;
                for (int i = 0; i < steps; i++)
                {
                    var x = startX + i * (endX - startX) / steps;
                    var y = func(x);
                    if (y < 0)
                        negativeSum += y;
                    if (y > 0)
                        positiveSum += y;

                    if (minY > y)
                        minY = y;

                    if (maxY < y)
                        maxY = y;
                }

                var dy = maxY - minY;
                maxY += dy * 0.1;
                minY -= dy * 0.1;
                dy = maxY - minY;
                double scaleY = dy > 0.01 ? height / dy : height / 0.01;
                for (int i = 0; i < steps; i++)
                {
                    var x = startX + i * (endX - startX) / steps;
                    var y = func(x);
                    var nx = startX + (i + 1) * (endX - startX) / steps;
                    var ny = func(nx);
                    Line l = new Line();
                    l.Stroke = Brushes.White;
                    l.StrokeThickness = 1;
                    l.X1 = i * width / steps;
                    l.Y1 = height - (y - minY) * scaleY;
                    l.X2 = (i + 1) * width / steps;
                    l.Y2 = height - (ny - minY) * scaleY;
                    Children.Add(l);
                }

                var axisY = height + minY * scaleY;
                Line axis = new Line();
                axis.Stroke = Brushes.DarkGray;
                axis.StrokeThickness = 1;
                axis.X1 = 0;
                axis.Y1 = axisY;
                axis.X2 = width;
                axis.Y2 = axisY;
                Children.Add(axis);

                if (showIntegrals)
                {
                    maxY = positiveSum * 1.1;
                    minY = negativeSum * 1.1;
                    dy = maxY - minY;
                    scaleY = dy > 0.01 ? height / dy : height / 0.01;
                    double thick = width / 20;
                    var total = positiveSum + negativeSum;
                    AddRect(width - 2 * thick, axisY, positiveSum * scaleY, Brushes.Red);
                    AddRect(width - 2 * thick, axisY, negativeSum * scaleY, Brushes.LightBlue);
                    AddRect(width - 1 * thick, axisY, total * scaleY, (total > 0) ? Brushes.DarkRed : Brushes.DarkBlue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AddRect(double leftX, double axisY,  double value, Brush color)
        {
            var width = ActualWidth;
            var height = ActualHeight;
            double thick = width / 20;
            var rect = new System.Windows.Shapes.Rectangle
            {
                Width = width / 20,
                Height = Math.Abs(value),
                Fill = color
            };
            
            SetLeft(rect, leftX);
            SetTop(rect, value > 0 ? axisY-value : axisY);

            Children.Add(rect);
        }
    }
}

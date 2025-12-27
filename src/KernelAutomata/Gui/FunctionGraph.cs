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

namespace KernelAutomata.Gui
{
    public class FunctionGraph : Canvas
    {
        public void Draw(int steps, double startX, double endX, Func<double, double> func)
        {
            var width = ActualWidth;
            var height = ActualHeight;
            Children.Clear();
            Background = Brushes.Black;
            double minY = 1000000000;
            double maxY = -1000000000;
            for (int i = 0; i < steps; i++)
            {
                var x = startX + i * (endX - startX) / steps;
                var y = func(x);
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
                l.X2 = (i+1) * width / steps;
                l.Y2 = height - (ny - minY) * scaleY;
                Children.Add(l);
            }

            Line axis = new Line();
            axis.Stroke = Brushes.DarkGray;
            axis.StrokeThickness = 1;
            axis.X1 = 0;
            axis.Y1 = height - (0 - minY) * scaleY;
            axis.X2 = width;
            axis.Y2 = height - (0 - minY) * scaleY;
            Children.Add(axis);
        }
    }
}

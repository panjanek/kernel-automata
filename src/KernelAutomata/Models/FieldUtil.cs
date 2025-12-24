using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelAutomata.Models
{
    public static class FieldUtil
    {
        public static float[] InitRandom(int size)
        {
            float[] fieldData = new float[size * size * 4];

            Random rng = new Random();

            for (int i = 0; i < size * size; i++)
            {
                fieldData[4 * i + 0] = (float)rng.NextDouble(); // real
                fieldData[4 * i + 1] = 0f;                      // imag
                fieldData[4 * i + 2] = 0f;
                fieldData[4 * i + 3] = 0f;

                var x = i % size;
                var y = i / size;
                var r = Math.Sqrt((x - size / 2) * (x - size / 2) + (y - size / 2) * (y - size/2));
                if (r < 50)
                    fieldData[4 * i + 0] = 1.0f;

                //fieldData[4 * i + 0] = 1.0f;
            }

            return fieldData;
        }
    }
}

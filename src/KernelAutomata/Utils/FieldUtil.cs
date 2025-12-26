using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace KernelAutomata.Utils
{
    public static class FieldUtil
    {
        public static float[] RandomRingWithDisk(int fieldSize, float centerX, float centerY, float noiseRadius, float blobRadius)
        {
            float[] fieldData = new float[fieldSize * fieldSize * 4];

            Random rng = new Random(1);

            for (int i = 0; i < fieldSize * fieldSize; i++)
            {
                fieldData[4 * i + 0] = (float)rng.NextDouble()*0.5f; 
                fieldData[4 * i + 1] = 0f;                     
                fieldData[4 * i + 2] = 0f;
                fieldData[4 * i + 3] = 0f;

                var x = i % fieldSize;
                var y = fieldSize - 1 - i / fieldSize;
                var cx = centerX * fieldSize;
                var cy = centerY * fieldSize;
                var distX = GetTorusDistance(x, cx, fieldSize);
                var distY = GetTorusDistance(y, cy, fieldSize);
                var r = Math.Sqrt(distX*distX + distY*distY);
                if (r < blobRadius * fieldSize)
                    fieldData[4 * i + 0] = 1.0f;

                if (r > noiseRadius * fieldSize) fieldData[4 * i + 0] = 0.0f;
            }

            return fieldData;
        }

        public static double GetTorusDistance(double d1, double d2, double size)
        {
            double d = d2 - d1;
            if (Math.Abs(d) > size / 2)
            {
                d = d - size * Math.Sign(d);
            }

            return d;
        }
    }
}

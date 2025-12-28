using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Utils;

namespace KernelAutomata.Models
{
    public class SimulationRecipe
    {
        public int size;

        public float dt;

        public ChannelRecipe[] channels;
    }

    public class ChannelRecipe
    {
        public float mu; 

        public float sigma;

        public float mu2;

        public float sigma2;

        public float weight2;

        public float decay;

        public KernelRecipe[] kernels;

        public InitializationRecipe initialization;

        public double GrowthFunction(double u)
        {
            if (sigma > 0.0001)
            {
                var g = MathUtil.Growth(u, mu, sigma);
                if (mu2 > 0.0001 && sigma2 > 0.0001 && weight2 > 0.0001)
                {
                    var g2 = MathUtil.Growth(u, mu2, sigma2);
                    g = (g + g2*weight2) / (1 + weight2);
                }
                return g;
            }
            else
            {
                return 0;
            }
        }
    }

    public class KernelRecipe
    {
        public float weight;

        public RingRecipe[] rings { get; set; }

        public KernelRecipe Clone()
        {
            return new KernelRecipe() { weight = weight, rings = rings.Select(r => r.Clone()).ToArray() };
        }

        public void Overwrite(KernelRecipe recipe)
        {
            this.weight = recipe.weight;
            for(int i=0; i<rings.Length; i++)
            {
                rings[i].Overwrite(recipe.rings[i]);
            }
        }
    }

    public class RingRecipe
    {
        public float maxR;

        public float center;

        public float width;

        public float weight;

        public float innerSlope = 1.0f;

        public float outerSlope = 1.0f;

        public RingRecipe Clone()
        {
            return new RingRecipe()
            {
                maxR = maxR,
                center = center,
                innerSlope = innerSlope,
                outerSlope = outerSlope,
                weight = weight,
                width = width,
            };
        }

        public void Overwrite(RingRecipe recipe)
        {
            maxR = recipe.maxR;
            center = recipe.center;
            width = recipe.width;
            weight = recipe.weight;
            innerSlope = recipe.innerSlope;
            outerSlope = recipe.outerSlope;
        }
    }

    public class InitializationRecipe
    {
        public float centerX;

        public float centerY;

        public float density;

        public float noiseRadius;

        public float blobRadius;

        public float mu;

        public float sigma;

        public void FillInitBufferWithRandomData(int size, float[] initBuffer)
        {
            Random rng = new Random(1);

            for (int i = 0; i < size * size; i++)
            {
                var rnd = (float)rng.NextDouble();
                initBuffer[4 * i + 0] = rnd * density;
                if (mu>0 && sigma > 0)
                {
                    double u1 = 1.0 - rnd;
                    double u2 = 1.0 - rng.NextDouble();

                    // Box–Muller transform
                    double standardNormal =
                        Math.Sqrt(-2.0 * Math.Log(u1)) *
                        Math.Cos(2.0 * Math.PI * u2);

                    // Scale and shift
                    double value = mu + sigma * standardNormal;

                    // Clamp to [0,1] for pixel use
                    var v = Math.Min(1.0, Math.Max(0.0, value));

                    initBuffer[4 * i + 0] = (float)v;
                }
               


                initBuffer[4 * i + 1] = 0f;
                initBuffer[4 * i + 2] = 0f;
                initBuffer[4 * i + 3] = 0f;

                var x = i % size;
                var y = size - 1 - i / size;
                var cx = centerX * size;
                var cy = centerY * size;
                var distX = MathUtil.GetTorusDistance(x, cx, size);
                var distY = MathUtil.GetTorusDistance(y, cy, size);
                var r = Math.Sqrt(distX * distX + distY * distY);
                if (r < blobRadius * size)
                    initBuffer[4 * i + 0] = 1.0f;

                if (r > noiseRadius * size)
                    initBuffer[4 * i + 0] = 0.0f;
            }
        }
    }
}

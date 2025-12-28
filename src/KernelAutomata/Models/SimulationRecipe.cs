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
    }

    public class RingRecipe
    {
        public float maxR;

        public float center;

        public float width;

        public float weight;

        public float innerSlope = 1.0f;

        public float outerSlope = 1.0f;
    }

    public class InitializationRecipe
    {
        public float centerX;

        public float centerY;

        public float density;

        public float noiseRadius;

        public float blobRadius;
    }
}

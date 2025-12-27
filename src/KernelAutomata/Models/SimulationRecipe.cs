using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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

        public float decay;

        public KernelRecipe[] kernels;

        public InitializationRecipe initialization;

        public double GrowthFunction(double u)
        {
            double x = (u - mu) / sigma;
            return (2.0 * Math.Exp(-x * x) - 1.0);
        }
    }

    public class KernelRecipe
    {
        public float weight;

        public RingRecipe[] rings;
    }

    public class RingRecipe
    {
        public float maxR;

        public float center;

        public float width;

        public float weight;
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

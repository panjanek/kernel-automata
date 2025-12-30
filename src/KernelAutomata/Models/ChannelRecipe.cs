using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Utils;

namespace KernelAutomata.Models
{
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

        public ChannelRecipe Clone()
        {
            return new ChannelRecipe()
            {
                mu = mu,
                sigma = sigma,
                mu2 = mu2,
                sigma2 = sigma2,
                weight2 = weight2,
                decay = decay,
                kernels = kernels.Select(k => k.Clone()).ToArray(),
                initialization = initialization.Clone()
            };
        }

        public void OverwriteWith(ChannelRecipe recipe)
        {
            mu = recipe.mu;
            sigma = recipe.sigma;
            mu2 = recipe.mu2;
            sigma2 = recipe.sigma2;
            weight2 = recipe.weight2;
            decay = recipe.decay;
            initialization.OverwriteWith(recipe.initialization);
            for (int k = 0; k < kernels.Length; k++)
                kernels[k].OverwriteWith(recipe.kernels[k]);
        }

        public double GrowthFunction(double u)
        {
            if (sigma > 0.0001)
            {
                var g = MathUtil.Growth(u, mu, sigma);
                if (mu2 > 0.0001 && sigma2 > 0.0001 && weight2 > 0.0001)
                {
                    var g2 = MathUtil.Growth(u, mu2, sigma2);
                    g = (g + g2 * weight2) / (1 + weight2);
                }
                return g;
            }
            else
            {
                return 0;
            }
        }
    }
}

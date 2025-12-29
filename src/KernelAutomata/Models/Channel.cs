using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using KernelAutomata.Gpu;
using KernelAutomata.Utils;

namespace KernelAutomata.Models
{
    public class Channel
    {
        private Simulation simulation;

        public float growthMu;

        public float growthSigma;

        public float growthMu2;

        public float growthSigma2;

        public float growthWeight2;

        public float decay;

        public Kernel[] kernels;

        public GpuChannel gpu;

        private GpuContext gpuContext;

        private InitializationRecipe initializationRecipe;

        private float[] initBuffer;

        public Channel(Simulation simulation, GpuContext gpuContext, ChannelRecipe recipe)
        {
            this.simulation = simulation;
            this.gpuContext = gpuContext;
            gpu = new GpuChannel(simulation.fieldSize, simulation.channels.Length, gpuContext.convolutionProgram, gpuContext.growthProgram);
            initBuffer = new float[simulation.fieldSize * simulation.fieldSize * 4];

            kernels = new Kernel[simulation.channels.Length];
            for (int k = 0; k < kernels.Length; k++)
                kernels[k] = new Kernel(simulation.fieldSize, gpuContext, recipe.kernels[k]);

            UpdateRecipe(recipe);
            ResetField();
        }

        public void UpdateRecipe(ChannelRecipe recipe)
        {
            growthMu = recipe.mu;
            growthSigma = recipe.sigma;
            growthMu2 = recipe.mu2;
            growthSigma2 = recipe.sigma2;
            growthWeight2 = recipe.weight2;
            decay = recipe.decay;
            if (recipe.kernels.Length != kernels.Length)
                throw new Exception($"Cannot change kernels/channels count from {kernels.Length} to {recipe.kernels.Length}. Must recreate simulation and gpu context");

            for(int k=0; k<kernels.Length; k++)
            {
                kernels[k].UpdateRecipe(recipe.kernels[k]);
            }

            initializationRecipe = recipe.initialization;
        }

        public void Convolve()
        {
            gpu.Convolve(kernels.Select(k=>k.gpu.FftTex).ToArray());
        }

        public void Grow(int myConv, float myWeight, int differentConv, float differentWeight, int differentConv2, float differentWeight2)
        {
            gpu.Grow(myConv, differentConv, differentConv2, myWeight, differentWeight, differentWeight2, growthMu, growthSigma, growthMu2, growthSigma2, growthWeight2, decay, simulation.dt);
        }

        public void ResetField()
        {
            initializationRecipe.FillInitBufferWithRandomData(simulation.fieldSize, initBuffer);
            gpu.UploadData(initBuffer);
        }

        public void UploadData(float[] fieldData)
        {
            gpu.UploadData(fieldData);
        }

        public void Destroy()
        {
            gpu.Destroy();
            foreach (var kernel in kernels)
                kernel.Destroy();
        }
    }
}

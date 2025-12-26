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

        public void Grow(int myConv, float myWeight, int differentConv, float differentWeight)
        {
            gpu.Grow(myConv, differentConv, myWeight, differentWeight, growthMu, growthSigma, decay, simulation.dt);
        }

        public void ResetField()
        {
            var data = FieldUtil.RandomRingWithDisk(simulation.fieldSize, initializationRecipe.centerX, initializationRecipe.centerY, initializationRecipe.noiseRadius, initializationRecipe.blobRadius);
            gpu.UploadData(data);
        }

        public void UploadData(float[] fieldData)
        {
            gpu.UploadData(fieldData);
        }
    }
}

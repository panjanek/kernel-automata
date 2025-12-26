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

        public Channel(Simulation simulation, GpuContext gpuContext, float mu, float sigma, float dec)
        {
            this.simulation = simulation;
            this.gpuContext = gpuContext;
            growthMu = mu;
            growthSigma = sigma;
            decay = dec;
            gpu = new GpuChannel(simulation.fieldSize, simulation.channels.Length, gpuContext.convolutionProgram, gpuContext.growthProgram);

            kernels = new Kernel[simulation.channels.Length];
            for (int k = 0; k < kernels.Length; k++)
                kernels[k] = new Kernel(simulation.fieldSize, gpuContext);
        }

        public void RecalculateKernels()
        {
            for (int k = 0; k < kernels.Length; k++)
                kernels[k].Recalculate();
        }

        public void Convolve()
        {
            gpu.Convolve(kernels.Select(k=>k.gpu.FftTex).ToArray());
        }

        public void Grow(int myConv, int competeConv)
        {
            gpu.Grow(myConv, competeConv, kernels[0].kernelWeight, kernels[1].kernelWeight, growthMu, growthSigma, decay, simulation.dt);
        }

        public void UploadData(float[] fieldData)
        {
            gpu.UploadData(fieldData);
        }
    }
}

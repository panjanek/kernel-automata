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

        public GpuChannel gpu;

        private GpuContext gpuContext;

        public Channel(Simulation simulation, GpuContext gpuContext)
        {
            this.simulation = simulation;
            this.gpuContext = gpuContext;
            gpu = new GpuChannel(simulation.fieldSize, simulation.channels, gpuContext.convolutionProgram, gpuContext.growthProgram);
        }

        public void Convolve(params Kernel[] kernels)
        {
            gpu.Convolve(kernels.Select(k=>k.gpu.FftTex).ToArray());
        }
    }
}

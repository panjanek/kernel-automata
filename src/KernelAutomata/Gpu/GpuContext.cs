using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelAutomata.Gpu
{
    public class GpuContext
    {
        public GpuContext(int size)
        {
            convolutionProgram = new ConvolutionProgram();
            growthProgram = new GrowthProgram();
            debugProgram = new DebugProgram();
            displayProgram = new DisplayProgram();
            fieldSize = size;
        }

        public ConvolutionProgram convolutionProgram;

        public GrowthProgram growthProgram;

        public DisplayProgram displayProgram;

        public DebugProgram debugProgram;

        public int fieldSize;
    }
}

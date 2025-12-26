using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Gpu;

namespace KernelAutomata.Models
{
    public class Kernel
    {
        private GpuContext gpuContext;
        public Kernel(int size, GpuContext gpuContext)
        {
            this.gpuContext = gpuContext;
            fieldSize = size;
            rings = new Ring[5];
            for(int r=0; r<rings.Length; r++)
                rings[r] = new Ring(size);
            kernelBuffer = new float[fieldSize * fieldSize * 4];
            gpu = new GpuKernel(fieldSize, gpuContext.convolutionProgram);
        }

        public GpuKernel gpu;

        public float kernelWeight = 1.0f;

        public int fieldSize;

        public Ring[] rings;

        public float[] kernelBuffer;

        public void Recalculate()
        {
            Array.Fill<float>(kernelBuffer, 0);
            for (int i = 0; i < rings.Length; i++)
            {
                var ring = rings[i];
                if (ring != null && ring.weight != 0)
                {
                    ring.Recalculate();
                    for (int j = 0; j < kernelBuffer.Length; j++)
                        kernelBuffer[j] += ring.ringBuffer[j] * ring.weight;
                }
            }

            //gpu.UploadData(kernelBuffer);
        }
    }
}

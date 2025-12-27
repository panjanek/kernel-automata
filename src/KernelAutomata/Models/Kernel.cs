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

        public GpuKernel gpu;

        public float kernelWeight = 1.0f;

        public int fieldSize;

        public Ring[] rings;

        public float[] kernelBuffer;

        public Kernel(int size, GpuContext gpuContext, KernelRecipe recipe)
        {
            this.gpuContext = gpuContext;
            fieldSize = size;
            rings = new Ring[5];
            for(int r=0; r<rings.Length; r++)
                rings[r] = new Ring(size);
            kernelBuffer = new float[fieldSize * fieldSize * 4];
            gpu = new GpuKernel(fieldSize, gpuContext.convolutionProgram);
            UpdateRecipe(recipe);
        }

        public void UpdateRecipe(KernelRecipe recipe)
        {
            kernelWeight = recipe.weight;
            for(int r=0; r<rings.Length; r++)
            {
                if (r >= recipe.rings.Length)
                {
                    rings[r].maxR = 0;
                    rings[r].width = 0;
                    rings[r].center = 0;
                    rings[r].weight = 0;
                }
                else
                {
                    var ringRecipe = recipe.rings[r];
                    rings[r].maxR = ringRecipe.maxR;
                    rings[r].width = ringRecipe.width;
                    rings[r].center = ringRecipe.center;
                    rings[r].weight = ringRecipe.weight;
                }
            }

            Recalculate();
        }

        public void Destroy()
        {
            gpu.Destroy();
        }

        private void Recalculate()
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

            gpu.UploadData(kernelBuffer);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace KernelAutomata.Models
{
    public class KernelRecipe
    {
        public float weight;

        public RingRecipe[] rings { get; set; }

        public KernelRecipe Clone()
        {
            return new KernelRecipe() { weight = weight, rings = rings.Select(r => r.Clone()).ToArray() };
        }

        public void OverwriteWith(KernelRecipe recipe)
        {
            this.weight = recipe.weight;
            for (int i = 0; i < rings.Length; i++)
            {
                rings[i].OverwriteWith(recipe.rings[i]);
            }
        }

        public void Invert()
        {
            foreach (var ring in rings)
                ring.Invert();
        }

        public void ChangeCenters(float delta)
        {
            foreach (var ring in rings)
                ring.ChangeCenter(delta);
        }

        public void NormalizeBy(float ratio, int size)
        {
            var kernelBuffer = new float[size * size * 4];
            var ringBuffer = new float[size * size * 4];
            FillBuffer(kernelBuffer, ringBuffer, size);
            var currentSum = kernelBuffer.Sum();
            NormalizeTo(currentSum * ratio, size);
        }

        public void NormalizeTo(float target, int size)
        {
            var kernelBuffer = new float[size * size * 4];
            var ringBuffer = new float[size * size * 4];
            FillBuffer(kernelBuffer, ringBuffer, size);
            var currentSum = kernelBuffer.Sum();
            var main = Clone();
            var copy = Clone();
            float previousDelta = Math.Abs(target - currentSum);
            float delta = 0;
            float sum = 0;
            for (int i = 0; i < 50; i++)
            {
                for (int r = 0; r < rings.Length; r++)
                {
                    var dir = Math.Sign(target - currentSum);
                    float ratio = (dir * main.rings[r].weight > 0) ? 1.05f : 0.95f;
                    copy.rings[r].weight = main.rings[r].weight * ratio;
                }

                copy.FillBuffer(kernelBuffer, ringBuffer, size);
                sum = kernelBuffer.Sum();
                delta = Math.Abs(target - sum);
                if (delta > previousDelta)
                    break;

                main = copy.Clone();
                previousDelta = delta;
            }

            OverwriteWith(main);
            FillBuffer(kernelBuffer, ringBuffer, size);
            var newSum = kernelBuffer.Sum();

            var a = 1;
        }

        public void FillBuffer(float[] kernelBuffer, float[] ringBuffer, int fieldSize)
        {
            Array.Fill<float>(kernelBuffer, 0);
            for (int i = 0; i < rings.Length; i++)
            {
                var ring = rings[i];
                if (ring != null && ring.weight != 0)
                {
                    ring.FillBuffer(ringBuffer, fieldSize);
                    for (int j = 0; j < kernelBuffer.Length; j++)
                        kernelBuffer[j] += ringBuffer[j] * ring.weight;
                }
            }
        }
    }
}

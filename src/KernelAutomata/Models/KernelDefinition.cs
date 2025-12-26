using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelAutomata.Models
{
    public class KernelDefinition
    {
        public KernelDefinition(int size)
        {
            fieldSize = size;
            rings = new RingDefinition[5];
            for(int r=0; r<rings.Length; r++)
                rings[r] = new RingDefinition(size);
            kernelBuffer = new float[fieldSize * fieldSize * 4];
        }

        public void Recalculate()
        {
            Array.Fill<float>(kernelBuffer, 0);
            for(int i=0; i<rings.Length; i++)
            {
                var ring = rings[i];
                if (ring != null && ring.weight != 0)
                {
                    ring.Recalculate();
                    for (int j = 0; j < kernelBuffer.Length; j++) 
                        kernelBuffer[j] += ring.ringBuffer[j] * ring.weight;
                }
            }
        }

        public int fieldSize;

        public RingDefinition[] rings;

        public float[] kernelBuffer;  
    }
}

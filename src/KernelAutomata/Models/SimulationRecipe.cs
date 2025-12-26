using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelAutomata.Models
{
    public class SimulationRecipe
    {
        public int size;

        public float dt;

        public ChannelRecipe[] channels;
    }

    public class ChannelRecipe
    {
        public float mu; 

        public float sigma;

        public float decay;
    }
}

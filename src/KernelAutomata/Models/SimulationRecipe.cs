using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Utils;

namespace KernelAutomata.Models
{
    public class SimulationRecipe
    {
        public int size;

        public float dt;

        public ChannelRecipe[] channels;

        public SimulationRecipe Clone()
        {
            return new SimulationRecipe()
            {
                size = size,
                dt = dt,
                channels = channels.Select(c => c.Clone()).ToArray()
            };
        }

        public void OverwriteWith(SimulationRecipe recipe)
        {
            size = recipe.size;
            dt = recipe.dt;
            for (int c = 0; c < channels.Length; c++)
                channels[c].OverwriteWith(recipe.channels[c]);
        }
    }
}

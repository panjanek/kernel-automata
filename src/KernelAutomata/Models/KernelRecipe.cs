using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void NormalizeTo(float value)
        {

        }
    }
}

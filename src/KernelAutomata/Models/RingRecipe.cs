using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelAutomata.Models
{
    public class RingRecipe
    {
        public float maxR;

        public float center;

        public float width;

        public float weight;

        public float innerSlope = 1.0f;

        public float outerSlope = 1.0f;

        public RingRecipe Clone()
        {
            return new RingRecipe()
            {
                maxR = maxR,
                center = center,
                innerSlope = innerSlope,
                outerSlope = outerSlope,
                weight = weight,
                width = width,
            };
        }

        public void OverwriteWith(RingRecipe recipe)
        {
            maxR = recipe.maxR;
            center = recipe.center;
            width = recipe.width;
            weight = recipe.weight;
            innerSlope = recipe.innerSlope;
            outerSlope = recipe.outerSlope;
        }

        public void Invert()
        {
            weight = -weight;
        }

        public void ChangeCenter(float delta)
        {
            center += delta;
            if (center < 0)
                center = 0;
            if (delta > 0)
                maxR += delta;
        }
    }
}

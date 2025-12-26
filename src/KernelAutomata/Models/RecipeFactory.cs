using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelAutomata.Models
{
    public static class RecipeFactory
    {
        public static SimulationRecipe OneChannelOrbs()
        {
            return new SimulationRecipe()
            {
                size = 1024,
                dt = 0.1f,
                channels =
                    [
                        new ChannelRecipe()
                        {
                            mu = 0.11f, 
                            sigma = 0.015f, 
                            decay = 0,
                            kernels = 
                            [
                                new KernelRecipe()
                                {
                                    weight = 1.0f,
                                    rings =
                                    [
                                        new RingRecipe() { maxR = 32, center = 10, width = 4, weight = 1.0f },
                                        new RingRecipe() { maxR = 32, center = 24, width = 7, weight = -0.36f }
                                    ]
                                }
                            ]
                        }
                    ]
            };
        }

        public static SimulationRecipe TwoChannelsOrbs()
        {
            return new SimulationRecipe()
            {
                size = 1024,
                dt = 0.1f,
                channels =
                    [
                         new ChannelRecipe()
                         {
                             mu = 0.11f,
                             sigma = 0.015f,
                             decay = 0,
                             kernels =
                             [
                                 new KernelRecipe()
                                 {
                                     weight = 1.0f,
                                     rings =
                                     [
                                         new RingRecipe() { maxR = 32, center = 10, width = 4, weight = 1.0f },
                                         new RingRecipe() { maxR = 32, center = 24, width = 7, weight = -0.36f }
                                     ]

                                 },
                                 new KernelRecipe()
                                 {
                                     weight = 0.5f,
                                     rings = [new RingRecipe() { maxR = 32, center = 7, width = 2, weight = 1.0f }]
                                 }
                             ]
                         },
                         new ChannelRecipe()
                         {
                             mu = 0.108f,
                             sigma = 0.015f,
                             decay = 0,
                             kernels =
                             [
                                 new KernelRecipe()
                                 {
                                     weight = 1.0f,
                                     rings =
                                     [
                                         new RingRecipe() { maxR = 32, center = 4, width = 2, weight = 0f },
                                         new RingRecipe() { maxR = 64, center = 12, width = 5, weight = 1.0f },
                                         new RingRecipe() { maxR = 64, center = 36, width = 8, weight = -0.35f }
                                     ]

                                 },
                                 new KernelRecipe()
                                 {
                                     weight = 0.01f,
                                     rings = [new RingRecipe() { maxR = 32, center = 7, width = 2, weight = 1.0f }]
                                 }
                             ]
                         }
                    ]
            };
        }

        public static SimulationRecipe TwoChannelsCaterpillar()
        {
            return new SimulationRecipe()
            {
                size = 1024,
                dt = 0.1f,
                channels =
                    [
                         new ChannelRecipe()
                         {
                             mu = 0.11f,
                             sigma = 0.015f,
                             decay = 0,
                             kernels =
                             [
                                 new KernelRecipe()
                                 {
                                     weight = 1.0f,
                                     rings =
                                     [
                                         new RingRecipe() { maxR = 32, center = 10, width = 4, weight = 1.0f },
                                         new RingRecipe() { maxR = 32, center = 24, width = 7, weight = -0.36f }
                                     ]

                                 },
                                 new KernelRecipe()
                                 {
                                     weight = 1.0f,
                                     rings = [
                                         new RingRecipe() { maxR = 32, center = 10, width = 4, weight = 0.3f },
                                         new RingRecipe() { maxR = 32, center = 3, width = 2, weight = -0.6f },
                                         ]
                                 }
                             ]
                         },
                         new ChannelRecipe()
                         {
                             mu = 0.13f,
                             sigma = 0.02f,
                             decay = 0.1f,
                             kernels =
                             [
                                 new KernelRecipe()
                                 {
                                     weight = 1.0f,
                                     rings =
                                     [
                                         new RingRecipe() { maxR = 32, center = 10, width = 4, weight = 1.0f },
                                         new RingRecipe() { maxR = 32, center = 3, width = 2, weight = -0.1f }
                                     ]

                                 },
                                 new KernelRecipe()
                                 {
                                     weight = 0.0f,
                                     rings = [new RingRecipe() { maxR = 32, center = 7, width = 2, weight = 1.0f }]
                                 }
                             ]
                         }
                    ]
            };
        }




    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Linq;

namespace KernelAutomata.Models
{
    public static class RecipeFactory
    {
        private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true };
        public static void SaveToFile(SimulationRecipe recipe, string fn)
        {
            var str = JsonSerializer.Serialize(recipe, serializerOptions);
            File.WriteAllText(fn, str);
        }

        public static SimulationRecipe LoadFromFile(string fn)
        {
            var str = File.ReadAllText(fn);
            return JsonSerializer.Deserialize<SimulationRecipe>(str, serializerOptions);
        }

        public static SimulationRecipe LoadFromResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var a = assembly.GetManifestResourceNames();
            var resourceName = $"KernelAutomata.recipes.{name}";
            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Resource not found: {resourceName}");
            using StreamReader reader = new StreamReader(stream);
            var str = reader.ReadToEnd();
            return JsonSerializer.Deserialize<SimulationRecipe>(str, serializerOptions);
        }
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
                            ],

                            initialization = new InitializationRecipe() { centerX = 0.5f, centerY = 0.5f, noiseRadius = 2.0f, blobRadius = 0.05f, density = 0.5f }
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
                             mu = 0.103f,
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
                             ],
                             initialization = new InitializationRecipe() { centerX = 0.3f, centerY = 0.3f, noiseRadius = 0.5f, blobRadius = 0.05f, density = 0.5f }
                         },
                         new ChannelRecipe()
                         {
                             mu = 0.1065f,
                             sigma = 0.015f,
                             decay = 0.004f,
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
                             ],
                             initialization = new InitializationRecipe() { centerX = 0.6f, centerY = 0.5f, noiseRadius = 0.7f, blobRadius = 0.1f, density = 0.5f }
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
                             decay = 0.0f,
                             kernels =
                             [
                                 new KernelRecipe()
                                 {
                                     weight = 1.0f,
                                     rings =
                                     [
                                         new RingRecipe() { maxR = 32, center = 10, width = 4, weight = 1.0f },
                                         new RingRecipe() { maxR = 32, center = 24, width = 7, weight = -0.36f },
                                         new RingRecipe(), new RingRecipe(), new RingRecipe()
                                     ]

                                 },
                                 new KernelRecipe()
                                 {
                                     weight = 1.0f,
                                     rings = [
                                         new RingRecipe() { maxR = 32, center = 10, width = 4, weight = 1.0f },
                                         new RingRecipe() { maxR = 32, center = 4, width = 2, weight = -0.3f },
                                         new RingRecipe(), new RingRecipe(), new RingRecipe()
                                         ]
                                 }
                             ],
                             initialization = new InitializationRecipe() { centerX = 0.5f, centerY = 0.5f, noiseRadius = 0.5f, blobRadius = 0.0f, density = 0.5f }
                         },
                         new ChannelRecipe()
                         {
                             mu = 0.149f,
                             sigma = 0.02f,
                             decay = 0.0f,
                             kernels =
                             [
                                 new KernelRecipe()
                                 {
                                     weight = 1.0f,
                                     rings =
                                     [
                                         new RingRecipe() { maxR = 32, center = 10, width = 5, weight = 0.7f },
                                         new RingRecipe() { maxR = 64, center = 40, width = 5, weight = -0.2f },
                                         new RingRecipe(), new RingRecipe(), new RingRecipe()
                                     ]

                                 },
                                 new KernelRecipe()
                                 {
                                     weight = 0.01f,
                                     rings = [
                                         new RingRecipe() { maxR = 32, center = 7, width = 2, weight = 1.0f },
                                         new RingRecipe(), new RingRecipe(), new RingRecipe(), new RingRecipe()
                                         ]
                                 }
                             ],
                             initialization = new InitializationRecipe() { centerX = 0.5f, centerY = 0.5f, noiseRadius = 0.5f, blobRadius = 0.0f, density = 0.5f }
                         }
                    ]
            };
        }




    }
}

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

        public static SimulationRecipe ExpandTo(SimulationRecipe recipe1, int targetCount)
        {
            var copy = recipe1.Clone();
            int numberToAdd = targetCount - copy.channels.Length;
            var channels = copy.channels.ToList();
            for(int i=0; i< numberToAdd; i++)
                channels.Add(copy.channels.Last().Clone());
            foreach(var channel in channels)
            {
                var kernels = channel.kernels.ToList();
                for (int i = 0; i < numberToAdd; i++)
                    kernels.Add(channel.kernels.Last().Clone());

                channel.kernels = kernels.ToArray();
            }

            copy.channels = channels.ToArray();
            return copy;
        }
    }
}

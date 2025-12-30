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
using OpenTK.Graphics.OpenGL;

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
            var str = LoadStringFromResource(name);
            return JsonSerializer.Deserialize<SimulationRecipe>(str, serializerOptions);
        }

        public static KernelRecipe LoadKernelPreset(string name)
        {
            var str = LoadStringFromResource(name);
            return JsonSerializer.Deserialize<KernelRecipe>(str, serializerOptions);
        }

        private static string LoadStringFromResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"KernelAutomata.recipes.{name}";
            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Resource not found: {resourceName}");
            using StreamReader reader = new StreamReader(stream);
            var str = reader.ReadToEnd();
            return str;
        }
        
        public static List<string> ListPresetsFromResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fullNames = assembly.GetManifestResourceNames().Where(n => n.StartsWith("KernelAutomata.recipes.presets.")).ToList();
            return fullNames.Select(n =>
            {
                var split = n.Split('.');
                return string.Join(".", split.Skip(2));
            }).ToList();
        }

        public static SimulationRecipe ChangeNumberOfChannelsTo(SimulationRecipe recipe, int targetCount)
        {
            var copy = recipe.Clone();
            if (targetCount == recipe.channels.Length)
                return copy;

            if (targetCount > recipe.channels.Length)
            {
                int numberToAdd = targetCount - copy.channels.Length;
                var channels = copy.channels.ToList();
                for (int i = 0; i < numberToAdd; i++)
                    channels.Add(copy.channels.Last().Clone());
                foreach (var channel in channels)
                {
                    var kernels = channel.kernels.ToList();
                    for (int i = 0; i < numberToAdd; i++)
                        kernels.Add(channel.kernels.Last().Clone());

                    channel.kernels = kernels.ToArray();
                }

                copy.channels = channels.ToArray();
                return copy;
            }
            else
            {
                copy.channels = copy.channels.Take(targetCount).ToArray();
                foreach (var channel in copy.channels)
                    channel.kernels = channel.kernels.Take(targetCount).ToArray();
                return copy;
            }     
        }
    }
}

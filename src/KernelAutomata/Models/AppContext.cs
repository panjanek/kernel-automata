using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Gpu;
using KernelAutomata.Gui;

namespace KernelAutomata.Models
{
    public class AppContext
    {
        public const int MaxChannelsCount = 3;
        
        public OpenGlRenderer renderer;

        public Simulation simulation;

        public SimulationRecipe recipe;

        public MainWindow mainWindow;

        public ConfigWindow configWindow;

        public System.Windows.Controls.Panel placeholder;

        public List<SimulationRecipe> UndoList { get; set; } = new();

        public List<SimulationRecipe> RedoList { get; set; } = new();

        public void StartNewSimulation(SimulationRecipe recipe)
        {
            if (simulation != null)
                simulation.Destroy();

            UndoList = new List<SimulationRecipe>();
            RedoList = new List<SimulationRecipe>();
            var gpu = new GpuContext(recipe.size, placeholder);
            simulation = new Simulation(recipe, gpu);
            simulation.ResetFields();
            renderer = new OpenGlRenderer(placeholder, this);
            this.recipe = recipe;
            UndoList.Add(recipe.Clone());
        }

        public void UpdateSimulationWithRecipe()
        {
            UndoList.Add(recipe.Clone());
            simulation.UpdateSimulationWithRecipe(recipe);
        }

        public void Undo()
        {
            if (UndoList.Count > 1)
            {
                RedoList.Add(recipe.Clone());
                var recipeToUse = UndoList[UndoList.Count - 2];
                UndoList.RemoveAt(UndoList.Count-1);
                recipe.OverwriteWith(recipeToUse);
                simulation.UpdateSimulationWithRecipe(recipe);
            }
        }

        public void Redo()
        {
            if (RedoList.Count > 0)
            {
                var recipeToUse = RedoList[RedoList.Count - 1];
                RedoList.RemoveAt(RedoList.Count-1);
                recipe.OverwriteWith(recipeToUse);
                UndoList.Add(recipe.Clone());
                simulation.UpdateSimulationWithRecipe(recipe);
            }
        }
    }
}

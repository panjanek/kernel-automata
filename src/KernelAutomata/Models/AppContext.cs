using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Gpu;

namespace KernelAutomata.Models
{
    public class AppContext
    {
        public OpenGlRenderer renderer;

        public Simulation simulation;

        public SimulationRecipe recipe;

        public MainWindow mainWindow;

        public System.Windows.Controls.Panel placeholder;

        public void StartNewSimulation(SimulationRecipe recipe)
        {
            if (simulation != null)
                simulation.Destroy();

            var gpu = new GpuContext(recipe.size, placeholder);
            simulation = new Simulation(recipe, gpu);
            simulation.ResetFields();
            renderer = new OpenGlRenderer(placeholder, simulation);
            this.recipe = recipe;
        }
    }
}

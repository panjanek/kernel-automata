using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using KernelAutomata.Gpu;
using KernelAutomata.Utils;
using OpenTK.Mathematics;

namespace KernelAutomata.Models
{
    public class Simulation
    {
        public GpuContext gpuContext;

        public int fieldSize;

        public float dt;

        public Channel[] channels;

        public Simulation(SimulationRecipe recipe, GpuContext gpu)
        {
            if (recipe.channels.Length == 0 || recipe.channels.Length > 2)
                throw new Exception($"Invalid channels count {recipe.channels.Length}");

            if (!GpuContext.ValidSizes.Contains(recipe.size))
                throw new Exception($"Invalid field size {fieldSize}");

            if (recipe.size != gpu.fieldSize)
                throw new Exception($"Simulation field size {recipe.size} must match gpu context size {gpu.fieldSize}");

            gpuContext = gpu;
            fieldSize = recipe.size;
            dt = recipe.dt;
            channels = new Channel[recipe.channels.Length];
            for (int c=0; c<channels.Length; c++)
            {
                channels[c] = new Channel(this, gpuContext, recipe.channels[c]);
            }
        }

        public void UpdateSimulationWithRecipe(SimulationRecipe recipe)
        {
            if (recipe.size != fieldSize)
                throw new Exception($"Cannot change size (from {fieldSize} to {recipe.size}). Must recreate simulation and GPU context");

            if (recipe.channels.Length != channels.Length)
                throw new Exception($"Cannot change channels count (from {channels.Length} to {recipe.channels.Length}). Must recreate simulation and GPU context");

            dt = recipe.dt;
            for(int c=0; c<channels.Length; c++)
            {
                channels[c].UpdateRecipe(recipe.channels[c]);
            }
        }

        public void ResetFields()
        {
            foreach (var channel in channels)
                channel.ResetField();
        }

        public void Step()
        {
            for(int c=0; c<channels.Length; c++)
            {
                var channel = channels[c];
                lock (this)
                {
                    channel.Convolve();
                    if (channels.Length == 1)
                    {
                        channel.Grow(channels[0].gpu.ConvTex[0], channels[0].kernels[0].kernelWeight, -1, 0);
                    }
                    else if (channels.Length == 2)
                    {
                        var differentChannel = channels[1 - c];
                        channel.Grow(channel.gpu.ConvTex[0], channel.kernels[0].kernelWeight, differentChannel.gpu.ConvTex[1], differentChannel.kernels[1].kernelWeight);
                    }
                }
            }
        }

        public void Destroy()
        {
            lock (this)
            {
                foreach (var channel in channels)
                    channel.Destroy();
            }
        }
    }
}

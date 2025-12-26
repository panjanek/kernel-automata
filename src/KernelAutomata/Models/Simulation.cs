using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Gpu;
using KernelAutomata.Utils;
using OpenTK.Mathematics;

namespace KernelAutomata.Models
{
    public class Simulation
    {
        public GpuContext gpuContext;
        public Simulation(SimulationRecipe recipe, GpuContext gpu)
        {
            if (recipe.channels.Length == 0 || recipe.channels.Length > 2)
                throw new Exception($"Invalid channels count {recipe.channels.Length}");

            if (!GpuContext.ValidSizes.Contains(fieldSize))
                throw new Exception($"Invalid field size {fieldSize}");

            if (recipe.size != gpu.fieldSize)
                throw new Exception($"Simulation field size {recipe.size} must match gpu context size {gpu.fieldSize}");

            gpuContext = gpu;
            fieldSize = recipe.size;
            channels = new Channel[recipe.channels.Length];
            dt = recipe.dt;

            for(int c=0; c<channels.Length; c++)
            {

            }



            if (channels.Length == 1)
            {
                ;
                //var red = new Channel(this, gpuContext, 0.11f, 0.015f, 0);
                var red = new Channel(this, gpuContext, new ChannelRecipe() { mu = 0.11f, sigma = 0.015f, decay = 0 });
                red.kernels[0].kernelWeight = 1.0f;                           //1.0 -0.36
                red.kernels[0].rings[0].Set(32, 10, 4, 1.0f);
                red.kernels[0].rings[1].Set(32, 24, 7, -0.36f);
                red.RecalculateKernels();
                red.UploadData(FieldUtil.RandomRingWithDisk(fieldSize, new Vector2(0.3f, 0.3f), 250 * fieldSize / 512, 25 * fieldSize / 512));
                channels[0] = red;
            }
            else if (channels.Length == 2)
            {
                //var red = new Channel(this, gpuContext, 0.11f, 0.015f, 0);
                //var green = new Channel(this, gpuContext, 0.108f, 0.015f, 0);

                var red = new Channel(this, gpuContext, new ChannelRecipe() { mu = 0.11f, sigma = 0.015f, decay = 0 });
                var green = new Channel(this, gpuContext, new ChannelRecipe() { mu = 0.108f, sigma = 0.015f, decay = 0 });

                red.kernels[0].kernelWeight = 1.0f;                    //1.0 -0.36
                red.kernels[0].rings[0].Set(32, 10, 4, 1.0f);
                red.kernels[0].rings[1].Set(32, 24, 7, -0.36f);

                red.kernels[1].kernelWeight = 0.01f;
                red.kernels[1].rings[0].Set(32, 7, 2f, 1.0f);

                red.RecalculateKernels();

                green.kernels[0].kernelWeight = 1.0f;
                green.kernels[0].rings[0].Set(32, 4, 2, 0.0f);
                green.kernels[0].rings[1].Set(64, 12, 5, 1.0f);
                green.kernels[0].rings[2].Set(64, 36, 8, -0.35f);

                green.kernels[1].kernelWeight = 0.5f;
                green.kernels[1].rings[0].Set(32, 7, 2f, 1.0f);

                green.RecalculateKernels();

                red.UploadData(FieldUtil.RandomRingWithDisk(fieldSize, new Vector2(0.3f, 0.3f), 250 * fieldSize / 512, 25 * fieldSize / 512));
                green.UploadData(FieldUtil.RandomRingWithDisk(fieldSize, new Vector2(0.6f, 0.6f), 350 * fieldSize / 512, 100 * fieldSize / 512));

                channels[0] = red;
                channels[1] = green;
            }
        }

        public void UpdateRecipe(SimulationRecipe recipe)
        {
            if (recipe.size != fieldSize)
                throw new Exception($"Cannot change size (from {fieldSize} to {recipe.size}). Must recreate simulation and GPU context");

            if (recipe.channels.Length != channels.Length)
                throw new Exception($"Cannot change channels count (from {channels.Length} to {recipe.channels.Length}). Must recreate simulation and GPU context");

            for(int c=0; c<channels.Length; c++)
            {
                channels[c].UpdateRecipe(recipe.channels[c]);
            }
        }

        public int fieldSize = 512*2;

        public float dt = 0.1f;

        public Channel[] channels;

        public void Step()
        {
            for(int c=0; c<channels.Length; c++)
            {
                var channel = channels[c];
                channel.Convolve();
                if (channels.Length == 1)
                {
                    channel.Grow(channels[0].gpu.ConvTex[0], -1);
                }
                else if (channels.Length == 2)
                {
                    var differentChannel = channels[1 - c];
                    channel.Grow(channel.gpu.ConvTex[0], differentChannel.gpu.ConvTex[1]);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using KernelAutomata.Models;
using OpenTK.Audio.OpenAL;
using AppContext = KernelAutomata.Models.AppContext;
using Channel = KernelAutomata.Models.Channel;

namespace KernelAutomata.Gui
{
    public class SimulationDataContext
    {
        public SimulationDataContext(AppContext app) 
        {
            Channels = new ChannelDataContext[AppContext.MaxChannelsCount];
            for (int channelIdx = 0; channelIdx < Channels.Length; channelIdx++)
            {
                Channels[channelIdx] = (channelIdx < app.recipe.channels.Length) ? new ChannelDataContext(app.recipe.channels[channelIdx], app.simulation.channels[channelIdx]) : new ChannelDataContext(null, null);
            }
        }
        public ChannelDataContext[] Channels { get; set; }
    }

    public class ChannelDataContext
    {
        public ChannelDataContext(ChannelRecipe channelRecipe, Channel channel)
        {
            if (channelRecipe == null || channel == null)
            {
                IsActive = false;
                return;
            }

            IsActive = true;
            Kernels = new KernelDataContext[AppContext.MaxChannelsCount];
            for (int kernelIdx = 0; kernelIdx < Kernels.Length; kernelIdx++)
            {
                Kernels[kernelIdx] = (kernelIdx < channelRecipe.kernels.Length) ? new KernelDataContext(channelRecipe.kernels[kernelIdx], channel.kernels[kernelIdx]) : new KernelDataContext(null, null);
            }
        }

        public KernelDataContext[] Kernels { get; set; }

        public bool IsActive { get; set; }
    }
    public class KernelDataContext
    {
        public KernelDataContext(KernelRecipe kernelRecipe, Kernel kernel)
        {
            if (kernelRecipe == null || kernel == null)
            {
                IsActive = false;
                return;
            }

            IsActive = true;
            Recipe = kernelRecipe;
            Kernel = kernel;
        }

        public KernelRecipe Recipe { get; set; }

        public Kernel Kernel { get; set; }

        public bool IsActive { get; set; }
    }
}

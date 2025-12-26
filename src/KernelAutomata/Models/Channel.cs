using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace KernelAutomata.Models
{
    public class Channel
    {
        private int fieldSize;

        public Kernel[] kernels;

        public Channel(int fieldSize, int channelsCount)
        {
            this.fieldSize = fieldSize;
            kernels = new Kernel[channelsCount];
        }
    }
}

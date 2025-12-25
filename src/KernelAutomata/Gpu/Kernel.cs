using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Models;
using OpenTK.Graphics.OpenGL;

namespace KernelAutomata.Gpu
{
    public class Kernel
    {
        private int sourceTex;

        private int tmpTex;

        private int kernelTex;

        private int kernelFftTex;

        private Simulation simulation;

        private ConvolutionProgram convolution;

        public Kernel(Simulation sim, ConvolutionProgram conv) 
        {
            convolution = conv;
            simulation = sim;
            kernelTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            kernelFftTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            sourceTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            tmpTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
        }

        public void UploadData(float[] kernelSum1)
        {
            if (kernelSum1.Length != simulation.fieldSize * simulation.fieldSize * 4)
                throw new Exception($"Invalid size of initialization array {kernelSum1.Length}, shoule be {simulation.fieldSize * simulation.fieldSize * 4}");

            GL.BindTexture(TextureTarget.Texture2D, kernelTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, kernelSum1);
            TextureUtil.CopyTexture2D(kernelTex, sourceTex, simulation.fieldSize, simulation.fieldSize);
            int resTex = convolution.DispatchFFT(
                sourceTex,
                tmpTex,
                simulation.fieldSize,
                inverse: false
            );
            TextureUtil.CopyTexture2D(resTex, kernelFftTex, simulation.fieldSize, simulation.fieldSize);

            //denormalize, debug only;
            var kernelMax1 = kernelSum1.Max();
            for (int i = 0; i < kernelSum1.Length; i++) kernelSum1[i] /= kernelMax1;
            GL.BindTexture(TextureTarget.Texture2D, kernelTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, kernelSum1);

        }

        public int FftTex => kernelFftTex;
    }
}

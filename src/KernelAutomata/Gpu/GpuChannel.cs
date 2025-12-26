using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KernelAutomata.Models;
using KernelAutomata.Utils;
using OpenTK.Graphics.OpenGL;

namespace KernelAutomata.Gpu
{
    public class GpuChannel
    {
        private int fieldTex;

        private int fieldNextTex;

        private ConvolutionTextureSet[] convTextures;

        private Simulation simulation;

        private ConvolutionProgram convolution;

        private GrowthProgram growth;

        public GpuChannel(Simulation sim, ConvolutionProgram conv, GrowthProgram gr)
        {
            simulation = sim;
            convolution = conv;
            growth = gr;

            //field ping pong buffers
            fieldTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            fieldNextTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);

            //convolution buffers and helper buffers
            convTextures = new ConvolutionTextureSet[sim.channels];
            for (int c = 0; c < convTextures.Length; c++)
                convTextures[c] = new ConvolutionTextureSet(sim.fieldSize);
        }

        public void UploadData(float[] fieldData)
        {
            if (fieldData.Length != simulation.fieldSize * simulation.fieldSize * 4)
                throw new Exception($"Invalid size of initialization array {fieldData.Length}, shoule be {simulation.fieldSize * simulation.fieldSize * 4}");

            GL.BindTexture(TextureTarget.Texture2D, fieldTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, fieldData);
        }

        public void Convolve(params int[] kernelsFftTex)
        {
            for (int i = 0; i < kernelsFftTex.Length; i++)
            {
                TextureUtil.CopyTexture2D(fieldTex, convTextures[i].sourceTex, simulation.fieldSize, simulation.fieldSize);
                convTextures[i].convTex = convolution.ConvolveFFT(
                    convTextures[i].sourceTex,
                    kernelsFftTex[i],
                    convTextures[i].fftTmpTex,
                    convTextures[i].tmpTex,
                    simulation.fieldSize);
            }
        }

        public void Grow(int myConv, int competeConv, float myWeight, float competeWeight, float mu, float sigma, float decay)
        {
            growth.DispatchGrowth(fieldTex, myConv, competeConv, myWeight, competeWeight, fieldNextTex, simulation.fieldSize, mu, sigma, simulation.dt, decay);
            (fieldTex, fieldNextTex) = (fieldNextTex, fieldTex);
        }

        public int[] ConvTex => convTextures.Select(t => t.convTex).ToArray();

        public int FieldTex => fieldNextTex;
    }

    public class ConvolutionTextureSet
    {
        public ConvolutionTextureSet(int size)
        {
            tmpTex = TextureUtil.CreateComplexTexture(size);
            sourceTex = TextureUtil.CreateComplexTexture(size);
            fftTmpTex = TextureUtil.CreateComplexTexture(size);
        }

        public int tmpTex;

        public int sourceTex;

        public int fftTmpTex;

        public int convTex;
    }
}

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
    public class Channel
    {
        private int fieldTex;

        private int fieldNextTex;

        private int myTmpTex;

        private int mySourceTex;

        private int myFftTmpTex;

        private int myConvTex;

        private int competeTmpTex;

        private int competeSourceTex;

        private int competeFftTmpTex;

        private int competeConvTex;

        private Simulation simulation;

        private ConvolutionProgram convolution;

        private GrowthProgram growth;

        public Channel(Simulation sim, ConvolutionProgram conv, GrowthProgram gr)
        {
            simulation = sim;
            convolution = conv;
            growth = gr;

            //field ping pong buffers
            fieldTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            fieldNextTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);

            //tmp buffers
            myTmpTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            mySourceTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            myFftTmpTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            competeTmpTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            competeSourceTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            competeFftTmpTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
        }

        public void UploadData(float[] fieldData)
        {
            if (fieldData.Length != simulation.fieldSize * simulation.fieldSize * 4)
                throw new Exception($"Invalid size of initialization array {fieldData.Length}, shoule be {simulation.fieldSize * simulation.fieldSize * 4}");

            GL.BindTexture(TextureTarget.Texture2D, fieldTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, fieldData);

        }

        public void Convolve(int myKernelFftTex, int competeKernelFftTex)
        {
            TextureUtil.CopyTexture2D(fieldTex, mySourceTex, simulation.fieldSize, simulation.fieldSize);
            myConvTex = convolution.ConvolveFFT(
                mySourceTex,
                myKernelFftTex,
                myFftTmpTex,
                myTmpTex,
                simulation.fieldSize);

            TextureUtil.CopyTexture2D(fieldTex, competeSourceTex, simulation.fieldSize, simulation.fieldSize);
            competeConvTex = convolution.ConvolveFFT(
                competeSourceTex,
                competeKernelFftTex,
                competeFftTmpTex,
                competeTmpTex,
                simulation.fieldSize);



        }

        public void Grow(int myConv, int competeConv, float competeWeight, float mu, float sigma)
        {
            growth.DispatchGrowth(fieldTex, myConv, competeConv, competeWeight, fieldNextTex, simulation.fieldSize, mu, sigma, simulation.dt);
            (fieldTex, fieldNextTex) = (fieldNextTex, fieldTex);
        }

        public int MyConvTex => myConvTex;

        public int CompeteConvTex => competeConvTex;

        public int FieldTex => fieldNextTex;
    }
}

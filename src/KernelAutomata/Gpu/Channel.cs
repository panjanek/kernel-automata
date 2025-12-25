using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Models;
using OpenTK.Graphics.OpenGL;

namespace KernelAutomata.Gpu
{
    public class Channel
    {
        private int fieldTex;

        private int fieldNextTex;

        private int tmp1Tex;

        private int source1Tex;

        private int fftTmpTex;

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
            tmp1Tex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            source1Tex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            fftTmpTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
        }

        public void UploadData(float[] fieldData)
        {
            if (fieldData.Length != simulation.fieldSize * simulation.fieldSize * 4)
                throw new Exception($"Invalid size of initialization array {fieldData.Length}, shoule be {simulation.fieldSize * simulation.fieldSize * 4}");

            GL.BindTexture(TextureTarget.Texture2D, fieldTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, fieldData);

        }

        public void ConvolveAndGrow(int kernelFftTex)
        {
            TextureUtil.CopyTexture2D(fieldTex, source1Tex, simulation.fieldSize, simulation.fieldSize);
            var resTex = convolution.ConvolveFFT(
                source1Tex,
                kernelFftTex,
                fftTmpTex,
                tmp1Tex,
                simulation.fieldSize);

            growth.DispatchGrowth(fieldTex, resTex, fieldNextTex, simulation.fieldSize, 0.1f, 0.015f, 0.1f);
            (fieldTex, fieldNextTex) = (fieldNextTex, fieldTex);


            /*
                float[] data = new float[simulation.fieldSize * simulation.fieldSize * 4];
                GL.BindTexture(TextureTarget.Texture2D, resTex);
                GL.GetTexImage(TextureTarget.Texture2D, level: 0,PixelFormat.Rgba, PixelType.Float, data);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                for (int i = 0; i < data.Length; i++) data[i] = data[i] / (simulation.fieldSize * simulation.fieldSize);
                var min = data.Min();
                var max = data.Max();
                MathUtil.MeanStd(data, out var mean, out var std); //0.0236145761   0.102233931
            */
        }

        public int FieldTex => fieldNextTex;
    }
}

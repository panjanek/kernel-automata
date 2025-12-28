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

        private int fieldSize;

        private int channelsCount;

        private ConvolutionProgram convolution;

        private GrowthProgram growth;

        public GpuChannel(int size, int channelsCnt, ConvolutionProgram conv, GrowthProgram gr)
        {
            fieldSize = size;
            channelsCount = channelsCnt;
            convolution = conv;
            growth = gr;

            //field ping pong buffers
            fieldTex = TextureUtil.CreateComplexTexture(fieldSize);
            fieldNextTex = TextureUtil.CreateComplexTexture(fieldSize);

            //convolution buffers and helper buffers
            convTextures = new ConvolutionTextureSet[channelsCount];
            for (int c = 0; c < convTextures.Length; c++)
                convTextures[c] = new ConvolutionTextureSet(fieldSize);
        }

        public void UploadData(float[] fieldData)
        {
            if (fieldData.Length != fieldSize * fieldSize * 4)
                throw new Exception($"Invalid size of initialization array {fieldData.Length}, shoule be {fieldSize * fieldSize * 4}");

            GL.BindTexture(TextureTarget.Texture2D, fieldTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, fieldSize, fieldSize, PixelFormat.Rgba, PixelType.Float, fieldData);
        }

        public void Convolve(params int[] kernelsFftTex)
        {
            for (int i = 0; i < kernelsFftTex.Length; i++)
            {
                TextureUtil.CopyTexture2D(fieldTex, convTextures[i].sourceTex, fieldSize, fieldSize);
                convTextures[i].convTex = convolution.ConvolveFFT(
                    convTextures[i].sourceTex,
                    kernelsFftTex[i],
                    convTextures[i].fftTmpTex,
                    convTextures[i].tmpTex,
                    fieldSize);
            }
        }

        public void Grow(int myConv, int competeConv, float myWeight, float competeWeight, float mu, float sigma, float mu2, float sigma2, float weight2, float decay, float dt)
        {
            growth.DispatchGrowth(fieldTex, myConv, competeConv, myWeight, competeWeight, fieldNextTex, fieldSize, mu, sigma, mu2, sigma2, weight2, dt, decay);
            (fieldTex, fieldNextTex) = (fieldNextTex, fieldTex);
        }

        public int[] ConvTex => convTextures.Select(t => t.convTex).ToArray();

        public int FieldTex => fieldNextTex;

        public int BackBufferTex => fieldTex;

        public void Destroy()
        {
            if (fieldTex != 0) GL.DeleteTexture(fieldTex);
            if (fieldNextTex != 0) GL.DeleteTexture(fieldNextTex);
            foreach (var texSet in convTextures)
                texSet.Destroy();
        }
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

        public void Destroy()
        {
            if (tmpTex != 0) GL.DeleteTexture(tmpTex);
            if (sourceTex != 0) GL.DeleteTexture(sourceTex);
            if (fftTmpTex != 0) GL.DeleteTexture(fftTmpTex);
        }
    }
}

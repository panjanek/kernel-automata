using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Models;
using KernelAutomata.Utils;
using OpenTK.Graphics.OpenGL;

namespace KernelAutomata.Gpu
{
    public class GpuKernel
    {
        private int sourceTex;

        private int tmpTex;

        private int kernelTex;

        private int kernelFftTex;

        private int size;

        private ConvolutionProgram convolution;

        public GpuKernel(int size, ConvolutionProgram conv) 
        {
            convolution = conv;
            this.size = size;
            kernelTex = TextureUtil.CreateComplexTexture(size);
            kernelFftTex = TextureUtil.CreateComplexTexture(size);
            sourceTex = TextureUtil.CreateComplexTexture(size);
            tmpTex = TextureUtil.CreateComplexTexture(size);
        }

        public void UploadData(float[] kernelSum1)
        {
            if (kernelSum1.Length != size * size * 4)
                throw new Exception($"Invalid size of initialization array {kernelSum1.Length}, shoule be {size * size * 4}");

            GL.BindTexture(TextureTarget.Texture2D, kernelTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, size, size, PixelFormat.Rgba, PixelType.Float, kernelSum1);
            TextureUtil.CopyTexture2D(kernelTex, sourceTex, size, size);
            int resTex = convolution.DispatchFFT(
                sourceTex,
                tmpTex,
                size,
                inverse: false
            );
            TextureUtil.CopyTexture2D(resTex, kernelFftTex, size, size);

            //denormalize, debug only;
            var kernelMax1 = kernelSum1.Max();
            for (int i = 0; i < kernelSum1.Length; i++) kernelSum1[i] /= kernelMax1;
            GL.BindTexture(TextureTarget.Texture2D, kernelTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, size, size, PixelFormat.Rgba, PixelType.Float, kernelSum1);

        }

        public int FftTex => kernelFftTex;
    }
}

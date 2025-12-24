using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace KernelAutomata.Gpu
{
    public class ConvolutionProgram
    {
        public int fftProgram;

        public int multiplyProgram;
        public ConvolutionProgram() 
        {
            fftProgram = ShaderUtil.CompileAndLinkComputeShader("fft_pass.comp");
            multiplyProgram = ShaderUtil.CompileAndLinkComputeShader("multiply.comp");
        }

        public void DispatchFFT(
                    int program,
                    int srcTex,
                    int dstTex,
                    int size,
                    bool inverse)
        {
            GL.UseProgram(program);

            GL.Uniform1(GL.GetUniformLocation(program, "uSize"), size);
            GL.Uniform1(GL.GetUniformLocation(program, "uInverse"), inverse ? -1 : 1);

            for (int stride = 1; stride < size; stride *= 2)
            {
                // Horizontal pass
                GL.Uniform1(GL.GetUniformLocation(program, "uStride"), stride);
                GL.Uniform1(GL.GetUniformLocation(program, "uStage"), stride);
                GL.Uniform1(GL.GetUniformLocation(program, "uHorizontal"), 1);

                GL.BindImageTexture(0, srcTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(1, dstTex, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

                GL.DispatchCompute(size / 16, size / 16, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

                // Swap
                (srcTex, dstTex) = (dstTex, srcTex);
            }

            for (int stride = 1; stride < size; stride *= 2)
            {
                // Vertical pass
                GL.Uniform1(GL.GetUniformLocation(program, "uStride"), stride);
                GL.Uniform1(GL.GetUniformLocation(program, "uStage"), stride);
                GL.Uniform1(GL.GetUniformLocation(program, "uHorizontal"), 0);

                GL.BindImageTexture(0, srcTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(1, dstTex, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

                GL.DispatchCompute(size / 16, size / 16, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

                (srcTex, dstTex) = (dstTex, srcTex);
            }
        }

        public void MultiplySpectra(int program, int aTex, int kTex, int outTex, int size)
        {
            GL.UseProgram(program);

            GL.BindImageTexture(0, aTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            GL.BindImageTexture(1, kTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            GL.BindImageTexture(2, outTex, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            GL.DispatchCompute(size / 16, size / 16, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }

        public void ConvolveFFT(
                int fftProgram,
                int mulProgram,
                int fieldTex,
                int kernelFftTex,
                int pingTex,
                int pongTex,
                int size)
        {
            // FFT(field)
            DispatchFFT(fftProgram, fieldTex, pingTex, size, inverse: false);

            int fieldFft = pingTex;

            // Multiply in frequency domain
            MultiplySpectra(mulProgram, fieldFft, kernelFftTex, pongTex, size);

            int resultFft = pongTex;

            // IFFT
            DispatchFFT(fftProgram, resultFft, pingTex, size, inverse: true);

            // pingTex now contains convolution result (needs 1/(N*N) normalization)
        }

    }
}

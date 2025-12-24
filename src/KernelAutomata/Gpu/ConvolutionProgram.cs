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
            int fftProgram,
            int inputTex,
            int pingTex,
            int pongTex,
            int size,
            bool inverse)
        {
            GL.UseProgram(fftProgram);

            GL.Uniform1(GL.GetUniformLocation(fftProgram, "uSize"), size);
            GL.Uniform1(GL.GetUniformLocation(fftProgram, "uInverse"), inverse ? -1 : 1);

            int srcTex;
            int dstTex;

            // =========================
            // 1. HORIZONTAL PASSES
            // =========================

            // First stage: inputTex -> pingTex (inputTex NEVER written)
            srcTex = inputTex;
            dstTex = pingTex;

            for (int stride = 1; stride < size; stride *= 2)
            {
                GL.Uniform1(GL.GetUniformLocation(fftProgram, "uStride"), stride);
                GL.Uniform1(GL.GetUniformLocation(fftProgram, "uStage"), stride);
                GL.Uniform1(GL.GetUniformLocation(fftProgram, "uHorizontal"), 1);

                GL.BindImageTexture(0, srcTex, 0, false, 0,
                    TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);

                GL.BindImageTexture(1, dstTex, 0, false, 0,
                    TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

                GL.DispatchCompute((size + 15) / 16, 1, size);

                //GL.DispatchCompute((size + 15) / 16, (size + 15) / 16, 1);

                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

                // After first pass, alternate ONLY between ping/pong
                srcTex = dstTex;
                dstTex = (dstTex == pingTex) ? pongTex : pingTex;
            }

            // =========================
            // 2. VERTICAL PASSES
            // =========================

            for (int stride = 1; stride < size; stride *= 2)
            {
                GL.Uniform1(GL.GetUniformLocation(fftProgram, "uStride"), stride);
                GL.Uniform1(GL.GetUniformLocation(fftProgram, "uStage"), stride);
                GL.Uniform1(GL.GetUniformLocation(fftProgram, "uHorizontal"), 0);

                GL.BindImageTexture(0, srcTex, 0, false, 0,
                    TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);

                GL.BindImageTexture(1, dstTex, 0, false, 0,
                    TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

                GL.DispatchCompute((size + 15) / 16, 1, size); 
                //GL.DispatchCompute((size + 15) / 16, (size + 15) / 16, 1);

                GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

                srcTex = dstTex;
                dstTex = (dstTex == pingTex) ? pongTex : pingTex;
            }

            // After completion:
            // - srcTex contains the FFT result
            // - inputTex is untouched
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
            DispatchFFT(fftProgram, fieldTex, pingTex, pongTex, size, inverse: false);

            int fieldFft = pingTex;

            // Multiply in frequency domain
            MultiplySpectra(mulProgram, fieldFft, kernelFftTex, pongTex, size);

            int resultFft = pongTex;

            // IFFT
            DispatchFFT(fftProgram, resultFft, pingTex, pongTex, size, inverse: true);

            // pingTex now contains convolution result (needs 1/(N*N) normalization)
        }
    }
}

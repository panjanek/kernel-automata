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
        private int fftProgram;

        private int multiplyProgram;
        public ConvolutionProgram() 
        {
            fftProgram = ShaderUtil.CompileAndLinkComputeShader("fft_cooley.comp");
            multiplyProgram = ShaderUtil.CompileAndLinkComputeShader("multiply.comp");
        }
        public int DispatchFFT(
            int inputTex,
            int pingTex,
            int size,
            bool inverse)
        {
            GL.UseProgram(fftProgram);

            int stages = (int)Math.Log2(size);

            GL.Uniform1(GL.GetUniformLocation(fftProgram, "uSize"), size);
            GL.Uniform1(GL.GetUniformLocation(fftProgram, "uInverse"), inverse ? -1 : 1);

            int src = inputTex;
            int dst = pingTex;

            // =========================
            // HORIZONTAL FFT
            // =========================
            GL.Uniform1(GL.GetUniformLocation(fftProgram, "uHorizontal"), 1);

            // Bit-reversal
            GL.Uniform1(GL.GetUniformLocation(fftProgram, "uStage"), -1);
            BindFFTImages(src, dst);
            Dispatch1D(size);
            Swap(ref src, ref dst);

            // FFT stages
            for (int s = 0; s < stages; s++)
            {
                GL.Uniform1(GL.GetUniformLocation(fftProgram, "uStage"), s);
                BindFFTImages(src, dst);
                Dispatch1D(size);
                Swap(ref src, ref dst);
            }

            // =========================
            // VERTICAL FFT
            // =========================
            GL.Uniform1(GL.GetUniformLocation(fftProgram, "uHorizontal"), 0);

            // Bit-reversal
            GL.Uniform1(GL.GetUniformLocation(fftProgram, "uStage"), -1);
            BindFFTImages(src, dst);
            Dispatch1D(size);
            Swap(ref src, ref dst);

            // FFT stages
            for (int s = 0; s < stages; s++)
            {
                GL.Uniform1(GL.GetUniformLocation(fftProgram, "uStage"), s);
                BindFFTImages(src, dst);
                Dispatch1D(size);
                Swap(ref src, ref dst);
            }

            return src;
        }

        // ---------- helpers ----------

        void BindFFTImages(int src, int dst)
        {
            GL.BindImageTexture(0, src, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            GL.BindImageTexture(1, dst, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);
        }

        void Dispatch1D(int size)
        {
            GL.DispatchCompute((size + 255) / 256, 1, size);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }

        void Swap(ref int a, ref int b)
        {
            int t = a; a = b; b = t;
        }

        public void MultiplySpectra(int aTex, int kTex, int outTex, int size)
        {
            GL.UseProgram(multiplyProgram);

            GL.BindImageTexture(0, aTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            GL.BindImageTexture(1, kTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            GL.BindImageTexture(2, outTex, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            GL.DispatchCompute(size / 16, size / 16, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }

        public int ConvolveFFT(
            int fieldTex,
            int kernelFftTex,
            int fftTmpTex,
            int pingTex,
            int size)
        {
            // FFT(field)
            int fieldFft = DispatchFFT(fieldTex, pingTex, size, inverse: false);

            // Multiply in frequency domain
            MultiplySpectra(fieldFft, kernelFftTex, fftTmpTex, size);

            // IFFT
            int convTex = DispatchFFT(fftTmpTex, pingTex, size, inverse: true);

            return convTex; // spatial-domain convolution result
        }
    }
}

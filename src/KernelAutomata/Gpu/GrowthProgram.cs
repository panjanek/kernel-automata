using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KernelAutomata.Utils;
using OpenTK.Graphics.OpenGL;

namespace KernelAutomata.Gpu
{
    public class GrowthProgram
    {
        private int program;
        public GrowthProgram()
        {
            program = ShaderUtil.CompileAndLinkComputeShader("growth.comp");
        }

        public void DispatchGrowth(
            int fieldInTex,
            int myConvTex,
            int competeConvTex,
            float competeWeight,
            int fieldOutTex,
            int size,
            float mu,
            float sigma,
            float dt)
        {
            GL.UseProgram(program);

            GL.Uniform1(GL.GetUniformLocation(program, "uMu"), mu);
            GL.Uniform1(GL.GetUniformLocation(program, "uSigma"), sigma);
            GL.Uniform1(GL.GetUniformLocation(program, "uDt"), dt);
            GL.Uniform1(GL.GetUniformLocation(program, "uSize"), size);
            GL.Uniform1(GL.GetUniformLocation(program, "uCompeteWeight"), competeWeight);

            GL.BindImageTexture(0, fieldInTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);

            GL.BindImageTexture(1, myConvTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            GL.BindImageTexture(2, competeConvTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);

            GL.BindImageTexture(3, fieldOutTex, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            GL.DispatchCompute(
                (size + 15) / 16,
                (size + 15) / 16,
                1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }
    }
}

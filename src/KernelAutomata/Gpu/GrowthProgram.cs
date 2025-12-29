using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            int competeConv2Tex,
            float myWeight,
            float competeWeight,
            float compete2Weight,
            int fieldOutTex,
            int size,
            float mu,
            float sigma,
            float mu2,
            float sigma2,
            float weight2,
            float dt,
            float decay)
        {
            GL.UseProgram(program);

            GL.Uniform1(GL.GetUniformLocation(program, "uMu"), mu);
            GL.Uniform1(GL.GetUniformLocation(program, "uSigma"), sigma);
            GL.Uniform1(GL.GetUniformLocation(program, "uMu2"), mu2);
            GL.Uniform1(GL.GetUniformLocation(program, "uSigma2"), sigma2);
            GL.Uniform1(GL.GetUniformLocation(program, "uSpike2Weight"), weight2);
            GL.Uniform1(GL.GetUniformLocation(program, "uDt"), dt);
            GL.Uniform1(GL.GetUniformLocation(program, "uSize"), size);
            GL.Uniform1(GL.GetUniformLocation(program, "uWeight1"), myWeight);
            GL.Uniform1(GL.GetUniformLocation(program, "uWeight2"), competeWeight);
            GL.Uniform1(GL.GetUniformLocation(program, "uWeight3"), compete2Weight);
            GL.Uniform1(GL.GetUniformLocation(program, "uDecay"), decay);

            GL.BindImageTexture(0, fieldInTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);

            GL.BindImageTexture(1, myConvTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            if (competeConvTex != -1)
                GL.BindImageTexture(2, competeConvTex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            if (competeConv2Tex != -1)
                GL.BindImageTexture(3, competeConv2Tex, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);

            GL.BindImageTexture(4, fieldOutTex, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            GL.DispatchCompute(
                (size + 15) / 16,
                (size + 15) / 16,
                1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }

        public void Destroy()
        {
            if (program != 0) GL.DeleteProgram(program);
        }
    }
}

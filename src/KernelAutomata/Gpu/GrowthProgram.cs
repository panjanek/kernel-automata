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
        public int program;
        public GrowthProgram()
        {
            program = ShaderUtil.CompileAndLinkComputeShader("growth.comp");

        }

        public void DispatchGrowth(
            int program,
            int fieldInTex,
            int convTex,
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

            GL.BindImageTexture(0, fieldInTex, 0, false, 0,
                TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);

            GL.BindImageTexture(1, convTex, 0, false, 0,
                TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);

            GL.BindImageTexture(2, fieldOutTex, 0, false, 0,
                TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            GL.DispatchCompute(
                (size + 15) / 16,
                (size + 15) / 16,
                1);

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }
    }
}

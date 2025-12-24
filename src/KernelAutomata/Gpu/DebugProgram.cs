using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace KernelAutomata.Gpu
{
    public class DebugProgram
    {
        private int program;

        private int stateLocation;

        private int offsetLocation;

        private int sizeLocation;
        public DebugProgram()
        {
            program = ShaderUtil.CompileAndLinkRenderShader("debug.vert", "debug.frag");

            stateLocation = GL.GetUniformLocation(program, "uState");
            if (stateLocation == -1) throw new Exception("Uniform 'uState' not found. Shader optimized it out?");
            offsetLocation = GL.GetUniformLocation(program, "offset");
            if (offsetLocation == -1) throw new Exception("Uniform 'offset' not found. Shader optimized it out?");
            sizeLocation = GL.GetUniformLocation(program, "size");
            if (sizeLocation == -1) throw new Exception("Uniform 'size' not found. Shader optimized it out?");
        }

        public void Run(int texture, Vector2 offset, Vector2 size)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.UseProgram(program);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Uniform1(stateLocation, 0);
            GL.Uniform2(offsetLocation, offset);
            GL.Uniform2(sizeLocation, size);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }
    }
}

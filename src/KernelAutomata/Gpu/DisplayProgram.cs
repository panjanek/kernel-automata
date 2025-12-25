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
    public class DisplayProgram
    {
        private int program;

        private int stateRedLocation;

        private int stateGreenLocation;

        private int offsetLocation;

        private int sizeLocation;

        private Vector2 offset = new Vector2(0, 0);

        private Vector2 size = new Vector2(1, 1);
        public DisplayProgram()
        {
            program = ShaderUtil.CompileAndLinkRenderShader("display.vert", "display.frag");

            stateRedLocation = GL.GetUniformLocation(program, "uStateRed");
            if (stateRedLocation == -1) throw new Exception("Uniform 'uStateGreen' not found. Shader optimized it out?");
            stateGreenLocation = GL.GetUniformLocation(program, "uStateGreen");
            if (stateGreenLocation == -1) throw new Exception("Uniform 'uStateGreen' not found. Shader optimized it out?");
            offsetLocation = GL.GetUniformLocation(program, "offset");
            if (offsetLocation == -1) throw new Exception("Uniform 'offset' not found. Shader optimized it out?");
            sizeLocation = GL.GetUniformLocation(program, "size");
            if (sizeLocation == -1) throw new Exception("Uniform 'size' not found. Shader optimized it out?");
        }

        public void Run(int textureRed, int textureGreen)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.UseProgram(program);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureRed);
            GL.Uniform1(stateRedLocation, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, textureGreen);
            GL.Uniform1(stateGreenLocation, 1);

            GL.Uniform2(offsetLocation, offset);
            GL.Uniform2(sizeLocation, size);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }
    }
}

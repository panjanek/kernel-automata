using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace KernelAutomata.Utils
{
    public static class TextureUtil
    {
        public static int CreateComplexTexture(int size)
        {
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.TexStorage2D(TextureTarget2d.Texture2D, 1,
                SizedInternalFormat.Rgba32f, size, size);

            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            return tex;
        }
        public static int CreateStateTexture(int width, int height)
        {
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba32f,
                width,
                height,
                0,
                PixelFormat.Rgba,
                PixelType.Float,
                nint.Zero
            );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // IMPORTANT: no mipmaps
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);

            return tex;
        }

        public static int CreateFboForTexture(int texture)
        {
            int fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                texture,
                0
            );

            GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 });

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception($"FBO incomplete: {status}");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            return fbo;
        }

        public static void CopyTexture2D(int srcTex, int dstTex, int width, int height)
        {
            GL.CopyImageSubData(
                srcTex, ImageTarget.Texture2D, 0,  // src
                0, 0, 0,
                dstTex, ImageTarget.Texture2D, 0,  // dst
                0, 0, 0,
                width, height, 1);
        }

        public static float[] ReadTexture(int tex, int width, int height)
        {
            float[] data = new float[width * height * 4];
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.GetTexImage(TextureTarget.Texture2D, level: 0,PixelFormat.Rgba, PixelType.Float, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            //normalize for inspection
            for (int i = 0; i < data.Length; i++) data[i] = data[i] / (width * height);
            var min = data.Min();
            var max = data.Max();
            MathUtil.MeanStd(data, out var mean, out var std); //0.0236145761   0.102233931
            return data;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using KernelAutomata.Models;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Panel = System.Windows.Controls.Panel;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace KernelAutomata.Gpu
{
    public class OpenGlRenderer
    {
        public int FrameCounter => frameCounter;

        private Panel placeholder;

        private WindowsFormsHost host;

        private GLControl glControl;

        private Simulation simulation;

        private DebugProgram debugProgram;

        private int frameCounter;

        private int dummyVao;

        private int kernelTex;

        private int ketnelFbo;
        public OpenGlRenderer(Panel placeholder, Simulation simulation)
        {
            this.placeholder = placeholder;
            this.simulation = simulation;
            host = new WindowsFormsHost();
            host.Visibility = Visibility.Visible;
            host.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            host.VerticalAlignment = VerticalAlignment.Stretch;
            glControl = new GLControl(new GLControlSettings
            {
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                APIVersion = new Version(3, 3), // OpenGL 3.3
                Profile = ContextProfile.Compatability,
                Flags = ContextFlags.Default,
                IsEventDriven = false
            });
            glControl.Dock = DockStyle.Fill;
            host.Child = glControl;
            placeholder.Children.Add(host);
            placeholder.SizeChanged += Placeholder_SizeChanged;
            glControl.Paint += GlControl_Paint;

            //setup required features
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            GL.BlendEquation(OpenTK.Graphics.OpenGL.BlendEquationMode.FuncAdd);
            GL.Enable(EnableCap.PointSprite);

            // create dummy vao
            GL.GenVertexArrays(1, out dummyVao);
            GL.BindVertexArray(dummyVao);

            debugProgram = new DebugProgram();

            int kernelSize = 64;
            if (kernelTex > 0)
                GL.DeleteTexture(kernelTex);
            kernelTex = TextureUtil.CreateStateTexture(kernelSize, kernelSize);
            float[,] kernel = KernelUtil.CreateRingKernel(kernelSize, 32, 0.5f, 0.5f);
            float[] initialState = KernelUtil.Flatten4Channels(kernel, 0);
            GL.BindTexture(TextureTarget.Texture2D, kernelTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, kernelSize, kernelSize, PixelFormat.Rgba, PixelType.Float, initialState);
            ketnelFbo = TextureUtil.CreateFboForTexture(kernelTex);
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);


            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();

        }

        private void Placeholder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (glControl.Width <= 0 || glControl.Height <= 0)
                return;

            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }

        public void Draw()
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {

            debugProgram.Run(kernelTex, new Vector2(0, 0), new Vector2(0.3f, 0.3f));

            glControl.SwapBuffers();
            frameCounter++;
        }
    }
}

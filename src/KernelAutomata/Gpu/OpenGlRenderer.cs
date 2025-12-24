using System;
using System.Collections.Generic;
using System.Drawing;
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
using static System.Net.Mime.MediaTypeNames;
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

        private DebugProgram debug;

        private ConvolutionProgram convolution;

        private GrowthProgram growth;

        private int frameCounter;

        private int dummyVao;

        private int kernelTex;

        private int kernelFftTex;

        private int fieldTex;

        private int fieldNextTex;

        private int pingTex;

        private int pongTex;

        private int tmpTex;

        private int fftTmpTex;

        private int fbo;

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

            debug = new DebugProgram();
            convolution = new ConvolutionProgram();
            growth = new GrowthProgram();

            kernelTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            kernelFftTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);

            fieldTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            fieldNextTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            pingTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            pongTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);

            tmpTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            fftTmpTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);



            //float[,] kernel = KernelUtil.CreateRingKernel(simulation.fieldSize, 32, 0.5f, 0.5f);
            //float[,] kernel = KernelUtil.CreateRingKernel2(simulation.fieldSize, 32, 0.5f, 0.5f);
            float[,] kernel = KernelUtil.CreateRingKernel3(simulation.fieldSize, 32, 0.75f, 0.11f);
            float[] kernelFlattened = KernelUtil.Flatten4Channels(kernel, 0);
            GL.BindTexture(TextureTarget.Texture2D, kernelTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, kernelFlattened);


            TextureUtil.CopyTexture2D(kernelTex, tmpTex, simulation.fieldSize, simulation.fieldSize);
            int resTex = convolution.DispatchFFT(
                convolution.fftProgram,
                tmpTex,
                pingTex,
                simulation.fieldSize,
                inverse: false
            );

            /*
            float[] data = new float[simulation.fieldSize * simulation.fieldSize * 4];
            GL.BindTexture(TextureTarget.Texture2D, resTex);
            GL.GetTexImage(TextureTarget.Texture2D, level: 0, PixelFormat.Rgba, PixelType.Float, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            var max = data.Max();
            */

            TextureUtil.CopyTexture2D(resTex, kernelFftTex, simulation.fieldSize, simulation.fieldSize);

            //denormalize original ring, only for debugging;
            var kernelMax = kernelFlattened.Max();
            for (int i = 0; i < kernelFlattened.Length; i++) kernelFlattened[i] /= kernelMax;
            GL.BindTexture(TextureTarget.Texture2D, kernelTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, kernelFlattened);



            var fieldData = FieldUtil.InitRandom(simulation.fieldSize);
            GL.BindTexture(TextureTarget.Texture2D, fieldTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, fieldData);

            GL.ClearTexImage(pingTex, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.ClearTexImage(pongTex, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);



            this.fbo = TextureUtil.CreateFboForTexture(fieldTex);
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
            TextureUtil.CopyTexture2D(fieldTex, tmpTex, simulation.fieldSize, simulation.fieldSize);

            var resTex = convolution.ConvolveFFT(
                convolution.fftProgram,
                convolution.multiplyProgram,
                tmpTex,
                kernelFftTex,
                fftTmpTex,
                pingTex,
                pongTex,
                simulation.fieldSize);
            

            /*
            float[] data = new float[simulation.fieldSize * simulation.fieldSize * 4];
            GL.BindTexture(TextureTarget.Texture2D, resTex);
            GL.GetTexImage(TextureTarget.Texture2D, level: 0,PixelFormat.Rgba, PixelType.Float, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            for (int i = 0; i < data.Length; i++) data[i] = data[i] / (simulation.fieldSize * simulation.fieldSize);
            var min = data.Min();
            var max = data.Max();
            MathUtil.MeanStd(data, out var mean, out var std); //0.0236145761   0.102233931
            */


            growth.DispatchGrowth(growth.program, fieldTex, resTex, fieldNextTex, simulation.fieldSize, 0.35f, 0.6f * 0.102233931f, 0.1f);   //  0.34f, 0.06f, 0.1f);
            //growth.DispatchGrowth(growth.program, fieldTex, resTex, fieldNextTex, simulation.fieldSize, 0.34f, 0.06f, 0.1f);

            TextureUtil.CopyTexture2D(fieldNextTex, fieldTex, simulation.fieldSize, simulation.fieldSize);



            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            //debug.Run(kernelFftTex, new Vector2(0, 0), new Vector2(1.0f, 1.0f));
            //debug.Run(fieldNextTex, new Vector2(0, 0), new Vector2(1.0f, 1.0f));
            debug.Run(fieldNextTex, new Vector2(0, 0), new Vector2(1.0f, 1.0f));

            debug.Run(kernelTex, new Vector2(0, 0), new Vector2(0.3f, 0.3f));

            glControl.SwapBuffers();
            frameCounter++;
        }
    }
}

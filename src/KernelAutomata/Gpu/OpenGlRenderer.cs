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

        private int kernel1Tex;

        private int kernel1FftTex;

        private int kernel2Tex;

        private int kernel2FftTex;

        private int fieldTex;

        private int fieldNextTex;

        private int tmp1Tex;

        private int tmp2Tex;

        private int source1Tex;

        private int source2Tex;

        private int fftTmpTex;

        private int fftTmp2Tex;

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

            //kernel buffers
            kernel1Tex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            kernel1FftTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            kernel2Tex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            kernel2FftTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);

            //field buffers
            fieldTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            fieldNextTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);

            //temporary buffers
            tmp1Tex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            tmp2Tex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            source1Tex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            source2Tex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            fftTmpTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);
            fftTmp2Tex = TextureUtil.CreateComplexTexture(simulation.fieldSize);

            //kernel 1
            float[] kernel1 = KernelUtil.Flatten4Channels(KernelUtil.CreateGausianRing(simulation.fieldSize, 32, 10f, 4f), 0);
            GL.BindTexture(TextureTarget.Texture2D, kernel1Tex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, kernel1);
            TextureUtil.CopyTexture2D(kernel1Tex, source1Tex, simulation.fieldSize, simulation.fieldSize);
            int resTex = convolution.DispatchFFT(
                source1Tex,
                tmp1Tex,
                simulation.fieldSize,
                inverse: false
            );
            TextureUtil.CopyTexture2D(resTex, kernel1FftTex, simulation.fieldSize, simulation.fieldSize);

            //kernel 2
            float[] kernel2 = KernelUtil.Flatten4Channels(KernelUtil.CreateGausianRing(simulation.fieldSize, 32, 24, 7), 0);
            GL.BindTexture(TextureTarget.Texture2D, kernel2Tex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, kernel2);
            TextureUtil.CopyTexture2D(kernel2Tex, source2Tex, simulation.fieldSize, simulation.fieldSize);
            resTex = convolution.DispatchFFT(
                source2Tex,
                tmp2Tex,
                simulation.fieldSize,
                inverse: false
            );
            TextureUtil.CopyTexture2D(resTex, kernel2FftTex, simulation.fieldSize, simulation.fieldSize);

            /*
            float[] data = new float[simulation.fieldSize * simulation.fieldSize * 4];
            GL.BindTexture(TextureTarget.Texture2D, resTex);
            GL.GetTexImage(TextureTarget.Texture2D, level: 0, PixelFormat.Rgba, PixelType.Float, data);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            var max = data.Max();
            */


            //denormalize original ring, only for debugging
            var kernelMax1 = kernel1.Max();
            for (int i = 0; i < kernel1.Length; i++) kernel1[i] /= kernelMax1;
            GL.BindTexture(TextureTarget.Texture2D, kernel1Tex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, kernel1);
            var kernelMax2 = kernel2.Max();
            for (int i = 0; i < kernel2.Length; i++) kernel2[i] /= kernelMax2;
            GL.BindTexture(TextureTarget.Texture2D, kernel2Tex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, kernel2);

            var fieldData = FieldUtil.InitRandom(simulation.fieldSize);
            GL.BindTexture(TextureTarget.Texture2D, fieldTex);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, fieldData);

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

            TextureUtil.CopyTexture2D(fieldTex, source1Tex, simulation.fieldSize, simulation.fieldSize);
            var res1Tex = convolution.ConvolveFFT(
                source1Tex,
                kernel1FftTex,
                fftTmpTex,
                tmp1Tex,
                simulation.fieldSize);

            TextureUtil.CopyTexture2D(fieldTex, source2Tex, simulation.fieldSize, simulation.fieldSize);
            var res2Tex = convolution.ConvolveFFT(
                source2Tex,
                kernel2FftTex,
                fftTmp2Tex,
                tmp2Tex,
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


            growth.DispatchGrowth(fieldTex, res1Tex, res2Tex, fieldNextTex, simulation.fieldSize, 0.1f, 0.015f, 0.1f);
            (fieldTex, fieldNextTex) = (fieldNextTex, fieldTex);

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            debug.Run(fieldNextTex, new Vector2(0, 0), new Vector2(1.0f, 1.0f));

            //debug.Run(kernel1Tex, new Vector2(-1.0f, -1.0f), new Vector2(1.3f, 1.3f));

            //debug.Run(kernel2Tex, new Vector2(-1.2f, -1.2f), new Vector2(1.3f, 1.3f));

            glControl.SwapBuffers();
            frameCounter++;
        }
    }
}

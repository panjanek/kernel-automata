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

        private DisplayProgram display;

        private int frameCounter;

        private int dummyVao;

        private Channel red;

        private Channel green;

        private Kernel kernel;

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

            //shader programs
            debug = new DebugProgram();
            display = new DisplayProgram();
            convolution = new ConvolutionProgram();
            growth = new GrowthProgram();

            // channels
            red = new Channel(simulation, convolution, growth);
            green = new Channel(simulation, convolution, growth);

            // one shared kernel with 2 rings
            kernel = new Kernel(simulation, convolution);
            float[] kernel1 = KernelUtil.Flatten4Channels(KernelUtil.CreateGausianRing(simulation.fieldSize, 32, 10f, 4f), 0);
            float[] kernel2 = KernelUtil.Flatten4Channels(KernelUtil.CreateGausianRing(simulation.fieldSize, 32, 24, 7), 0);
            kernel1 = KernelUtil.SumKernels(kernel1, 1.0f, kernel2, -0.36f);
            kernel.UploadData(kernel1);

            red.UploadData(FieldUtil.RandomRingWithDisk(simulation.fieldSize, new Vector2(0.3f, 0.3f), 250, 25));
            green.UploadData(FieldUtil.RandomRingWithDisk(simulation.fieldSize, new Vector2(0.6f, 0.6f), 350, 100));

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
            red.Convolve(kernel.FftTex);
            red.Grow(1, red.ConvTex, green.ConvTex);

            green.Convolve(kernel.FftTex);
            green.Grow(2, red.ConvTex, green.ConvTex);


            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            display.Run(red.FieldTex, green.FieldTex);

            //debug.Run(kernel1Tex, new Vector2(-1.0f, -1.0f), new Vector2(1.3f, 1.3f));
            //debug.Run(kernel2Tex, new Vector2(-1.2f, -1.2f), new Vector2(1.3f, 1.3f));

            glControl.SwapBuffers();
            frameCounter++;
        }
    }
}

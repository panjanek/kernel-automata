using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using KernelAutomata.Models;
using KernelAutomata.Utils;
using OpenTK.Core;
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
        public const double ZoomingSpeed = 0.0005;
        public int FrameCounter => frameCounter;

        public bool Paused { get; set; }

        private Panel placeholder;

        private WindowsFormsHost host;

        private GLControl glControl;

        private DraggingHandler dragging;

        private Vector2 center = new Vector2(0.5f, 0.5f);

        private float zoom = 1.0f;

        private Simulation simulation;

        private GpuContext gpuContext;

        private int frameCounter;

        private int dummyVao;

        private GpuChannel red;

        private GpuChannel green;

        private GpuKernel redSelf;

        private GpuKernel redOthers;

        private GpuKernel greenSelf;

        private GpuKernel greenOthers;

        private float aspectRatio => (float)(Math.Max(glControl?.ClientSize.Height ?? 1, 1)) / (float)(Math.Max(glControl?.ClientSize.Width ?? 1, 1));

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
            gpuContext = new GpuContext(simulation.fieldSize);

            // channels
            red = new GpuChannel(simulation.fieldSize, simulation.channels, gpuContext.convolutionProgram, gpuContext.growthProgram);
            green = new GpuChannel(simulation.fieldSize, simulation.channels, gpuContext.convolutionProgram, gpuContext.growthProgram);

            //float[] mediumRing = KernelUtil.CreateGausianRing(simulation.fieldSize, 32, 10f, 4f);
            //float[] largeRing = KernelUtil.CreateGausianRing(simulation.fieldSize, 32, 24, 7);
            //float[] smallRing = KernelUtil.CreateGausianRing(simulation.fieldSize, 32, 7, 2f);
            //redSelf.UploadData(KernelUtil.SumKernels(mediumRing, 1.0f, largeRing, -0.36f));
            //greenSelf.UploadData(KernelUtil.SumKernels(KernelUtil.CreateGausianRing(simulation.fieldSize, 32, 4, 2), -0.0f,
            //                                           KernelUtil.CreateGausianRing(simulation.fieldSize, 64, 12, 5), 1.0f,
            //                                           KernelUtil.CreateGausianRing(simulation.fieldSize, 64, 36, 8), -0.35f));

            Kernel redSelfKernel = new Kernel(simulation.fieldSize, gpuContext);
            redSelfKernel.rings[0].Set(32, 10, 4, 1.0f);
            redSelfKernel.rings[1].Set(32, 24, 7, -0.36f);
            redSelfKernel.Recalculate();
            redSelf = new GpuKernel(simulation.fieldSize, gpuContext.convolutionProgram);
            redSelf.UploadData(redSelfKernel.kernelBuffer);

            Kernel smallRingKernel = new Kernel(simulation.fieldSize, gpuContext);
            smallRingKernel.rings[0].Set(32, 7, 2f, 1.0f);
            smallRingKernel.Recalculate();
            redOthers = new GpuKernel(simulation.fieldSize, gpuContext.convolutionProgram);
            redOthers.UploadData(smallRingKernel.kernelBuffer);

            Kernel greenSelfKernel = new Kernel(simulation.fieldSize, gpuContext);
            greenSelfKernel.rings[0].Set(32, 4, 2, 0.0f);
            greenSelfKernel.rings[1].Set(64, 12, 5, 1.0f);
            greenSelfKernel.rings[2].Set(64, 36, 8, -0.35f);
            greenSelfKernel.Recalculate();
            greenSelf = new GpuKernel(simulation.fieldSize, gpuContext.convolutionProgram);
            greenSelf.UploadData(greenSelfKernel.kernelBuffer);

            greenOthers = new GpuKernel(simulation.fieldSize, gpuContext.convolutionProgram);
            greenOthers.UploadData(smallRingKernel.kernelBuffer);

            red.UploadData(FieldUtil.RandomRingWithDisk(simulation.fieldSize, new Vector2(0.3f, 0.3f), 250 * simulation.fieldSize / 512, 25 * simulation.fieldSize / 512));
            green.UploadData(FieldUtil.RandomRingWithDisk(simulation.fieldSize, new Vector2(0.6f, 0.6f), 350 * simulation.fieldSize / 512, 100 * simulation.fieldSize / 512));

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();

            dragging = new DraggingHandler(glControl, (pos, left) => true, (prev, curr) =>
            {
                float aspect = glControl.ClientSize.Width / glControl.ClientSize.Height;
                var delta = prev - curr;
                float screenToTexX = ((float)simulation.fieldSize / aspect) / glControl.ClientSize.Width;
                float screenToTexY = (float)simulation.fieldSize / glControl.ClientSize.Height;
                center.X += delta.X / (simulation.fieldSize * zoom / screenToTexX);
                center.Y -= delta.Y / (simulation.fieldSize * zoom / screenToTexY);
            });

            glControl.MouseWheel += GlControl_MouseWheel;

        }    

        private void GlControl_MouseWheel(object? sender, MouseEventArgs e)
        {
            var pos = new Vector2(e.X, e.Y);
            float zoomRatio = (float)(1.0 + ZoomingSpeed * e.Delta);
            float newZoom = zoom * zoomRatio;
            Vector2 mouseUV = new Vector2(pos.X / glControl.ClientSize.Width, 1.0f - pos.Y / glControl.ClientSize.Height);
            Vector2 mouseTex = new Vector2((mouseUV.X - 0.5f) / (zoom* aspectRatio) + center.X, (mouseUV.Y - 0.5f) / zoom + center.Y);           
            center = new Vector2(mouseTex.X - (mouseUV.X - 0.5f) / (newZoom* aspectRatio), mouseTex.Y - (mouseUV.Y - 0.5f) / newZoom);           
            
            zoom = newZoom;
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
            if (!Paused)
            {
                red.Convolve(redSelf.FftTex, greenOthers.FftTex);
                red.Grow(red.ConvTex[0], green.ConvTex[1], 1.0f, 0.01f, 0.11f, 0.015f, 0, simulation.dt);    //0.11 0.015

                green.Convolve(greenSelf.FftTex, redOthers.FftTex);
                green.Grow(green.ConvTex[0], red.ConvTex[1], 1.0f, 0.5f, 0.108f, 0.015f, 0, simulation.dt);
            }

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            gpuContext.displayProgram.Run(red.FieldTex, green.FieldTex, center, zoom, aspectRatio);

            //debug.Run(kernel1Tex, new Vector2(-1.0f, -1.0f), new Vector2(1.3f, 1.3f));
            //debug.Run(kernel2Tex, new Vector2(-1.2f, -1.2f), new Vector2(1.3f, 1.3f));

            glControl.SwapBuffers();
            frameCounter++;
        }
    }
}

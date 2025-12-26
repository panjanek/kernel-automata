using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
using KernelAutomata.Models;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;

namespace KernelAutomata.Gpu
{
    public class GpuContext
    {
        public static readonly int[] ValidSizes = [128, 256, 512, 1024, 2048, 4096];

        public WindowsFormsHost host;

        public GLControl glControl;

        private int dummyVao;
        public GpuContext(int size, System.Windows.Controls.Panel placeholder)
        {
            if (!ValidSizes.Contains(size))
                throw new Exception($"Invalid field size {fieldSize}");

            fieldSize = size;
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
            

            //setup required features
            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            GL.BlendEquation(OpenTK.Graphics.OpenGL.BlendEquationMode.FuncAdd);
            GL.Enable(EnableCap.PointSprite);

            // create dummy vao
            GL.GenVertexArrays(1, out dummyVao);
            GL.BindVertexArray(dummyVao);

            convolutionProgram = new ConvolutionProgram();
            growthProgram = new GrowthProgram();
            debugProgram = new DebugProgram();
            displayProgram = new DisplayProgram();
        }

        public ConvolutionProgram convolutionProgram;

        public GrowthProgram growthProgram;

        public DisplayProgram displayProgram;

        public DebugProgram debugProgram;

        public int fieldSize;

        public void SetViewportAndInvalidate()
        {
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            glControl.Invalidate();
        }
        private void Placeholder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (glControl.Width <= 0 || glControl.Height <= 0)
                return;

            if (!glControl.Context.IsCurrent)
                glControl.MakeCurrent();

            SetViewportAndInvalidate();
        }
    }
}

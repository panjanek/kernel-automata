using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
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

        private DraggingHandler dragging;

        private Vector2 center = new Vector2(0.5f, 0.5f);

        private float zoom = 1.0f;

        private Simulation simulation;

        private int frameCounter;

        private float aspectRatio => (float)(Math.Max(simulation.gpuContext.glControl?.ClientSize.Height ?? 1, 1)) / (float)(Math.Max(simulation.gpuContext.glControl?.ClientSize.Width ?? 1, 1));

        public OpenGlRenderer(Panel placeholder, Simulation simulation)
        {
            this.placeholder = placeholder;
            this.simulation = simulation;
            simulation.gpuContext.glControl.Paint += GlControl_Paint;

            dragging = new DraggingHandler(simulation.gpuContext.glControl, (pos, left) => true, (prev, curr) =>
            {
                float aspect = simulation.gpuContext.glControl.ClientSize.Width / simulation.gpuContext.glControl.ClientSize.Height;
                var delta = prev - curr;
                float screenToTexX = ((float)simulation.fieldSize / aspect) / simulation.gpuContext.glControl.ClientSize.Width;
                float screenToTexY = (float)simulation.fieldSize / simulation.gpuContext.glControl.ClientSize.Height;
                center.X += delta.X / (simulation.fieldSize * zoom / screenToTexX);
                center.Y -= delta.Y / (simulation.fieldSize * zoom / screenToTexY);
            });

            simulation.gpuContext.glControl.MouseWheel += GlControl_MouseWheel;
            simulation.gpuContext.SetViewportAndInvalidate();
        }    

        private void GlControl_MouseWheel(object? sender, MouseEventArgs e)
        {
            var pos = new Vector2(e.X, e.Y);
            float zoomRatio = (float)(1.0 + ZoomingSpeed * e.Delta);
            float newZoom = zoom * zoomRatio;
            Vector2 mouseUV = new Vector2(pos.X / simulation.gpuContext.glControl.ClientSize.Width, 1.0f - pos.Y / simulation.gpuContext.glControl.ClientSize.Height);
            Vector2 mouseTex = new Vector2((mouseUV.X - 0.5f) / (zoom* aspectRatio) + center.X, (mouseUV.Y - 0.5f) / zoom + center.Y);           
            center = new Vector2(mouseTex.X - (mouseUV.X - 0.5f) / (newZoom* aspectRatio), mouseTex.Y - (mouseUV.Y - 0.5f) / newZoom);           
            zoom = newZoom;
        }

        public void Step()
        {
            if (!Paused)
                simulation.Step();

            simulation.gpuContext.SetViewportAndInvalidate();
        }

        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            var channel1Tex = simulation.channels[0].gpu.FieldTex;
            var channel2Tex = simulation.channels.Length == 2 ? simulation.channels[1].gpu.FieldTex : -1;
            simulation.gpuContext.displayProgram.Run(channel1Tex, channel2Tex, center, zoom, aspectRatio);
            simulation.gpuContext.glControl.SwapBuffers();
            frameCounter++;
        }
    }
}

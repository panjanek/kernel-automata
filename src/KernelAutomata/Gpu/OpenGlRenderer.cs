using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using KernelAutomata.Models;
using OpenTK.Core;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using AppContext = KernelAutomata.Models.AppContext;
using Application = System.Windows.Application;
using Channel = KernelAutomata.Models.Channel;
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

        private AppContext app;

        private int frameCounter;

        private int emptyTex;

        public byte[] captureBuffer;

        private int? recFrameNr;

        private float aspectRatio => (float)(Math.Max(simulation.gpuContext.glControl?.ClientSize.Height ?? 1, 1)) / (float)(Math.Max(simulation.gpuContext.glControl?.ClientSize.Width ?? 1, 1));

        public OpenGlRenderer(Panel placeholder, AppContext appContext)
        {
            this.placeholder = placeholder;
            this.simulation = appContext.simulation;
            this.app = appContext;
            simulation.gpuContext.glControl.Paint += GlControl_Paint;
            emptyTex = TextureUtil.CreateComplexTexture(simulation.fieldSize);

            dragging = new DraggingHandler(simulation.gpuContext.glControl, (pos, left) => left, (prev, curr) =>
            {
                float aspect = simulation.gpuContext.glControl.ClientSize.Width / simulation.gpuContext.glControl.ClientSize.Height;
                var delta = prev - curr;
                float screenToTexX = ((float)simulation.fieldSize / aspect) / simulation.gpuContext.glControl.ClientSize.Width;
                float screenToTexY = (float)simulation.fieldSize / simulation.gpuContext.glControl.ClientSize.Height;
                center.X += delta.X / (simulation.fieldSize * zoom / screenToTexX);
                center.Y -= delta.Y / (simulation.fieldSize * zoom / screenToTexY);
            });

            simulation.gpuContext.glControl.MouseDown += (s, e) => { if (e.Button == MouseButtons.Right) DisturbField(e.X, e.Y, appContext.configWindow.DrawingMode); };
            simulation.gpuContext.glControl.MouseMove += (s, e) => { if (e.Button == MouseButtons.Right) DisturbField(e.X, e.Y, appContext.configWindow.DrawingMode); };
            simulation.gpuContext.glControl.MouseWheel += GlControl_MouseWheel;
            simulation.gpuContext.SetViewportAndInvalidate();
            simulation.gpuContext.glControl.SizeChanged += (s, e) =>
            {
                if (simulation.gpuContext.glControl.Width >= 0 && simulation.gpuContext.glControl.Height >= 0)
                {
                    captureBuffer = new byte[simulation.gpuContext.glControl.Width * simulation.gpuContext.glControl.Height * 4];
                }
            };
        }

        private void DisturbField(float mouseX, float mouseY, int drawingMode)
        {
            if (drawingMode == 0)
                foreach (var channel in simulation.channels)
                    DisturbChannelField(mouseX, mouseY, channel, true);
            else
            {
                var channelIdx = drawingMode - 1;
                if (channelIdx >= simulation.channels.Length)
                    channelIdx = simulation.channels.Length - 1;
                DisturbChannelField(mouseX, mouseY, simulation.channels[channelIdx], false);
            }
        }

        private void DisturbChannelField(float mouseX, float mouseY, Channel channel, bool erase)
        {
            lock (simulation)
            {
                var rnd = new Random(2);
                var tex = channel.gpu.BackBufferTex;
                float[] buffer = new float[simulation.fieldSize * simulation.fieldSize * 4];
                GL.BindTexture(TextureTarget.Texture2D, tex);
                GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.Float, buffer);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                Vector2 mouseUV = new Vector2(mouseX / simulation.gpuContext.glControl.ClientSize.Width, 1.0f - mouseY / simulation.gpuContext.glControl.ClientSize.Height);
                Vector2 mouseTex = new Vector2((mouseUV.X - 0.5f) / (zoom * aspectRatio) + center.X, (mouseUV.Y - 0.5f) / zoom + center.Y);
                int brushSize = (int)((simulation.fieldSize * 0.1) / zoom);
                for (int i = 0; i < brushSize; i++)
                {
                    for (int j = 0; j < brushSize; j++)
                    {
                        if (Math.Sqrt((i - brushSize / 2) * (i - brushSize / 2) + (j - brushSize / 2) * (j - brushSize / 2)) < brushSize / 2)
                        {
                            int bufferX = (int)((mouseTex.X) * simulation.fieldSize);
                            int bufferY = (int)((mouseTex.Y) * simulation.fieldSize);
                            int paintX = bufferX + i - brushSize / 2;
                            int paintY = bufferY + j - brushSize / 2;
                            if (paintX < 0)
                                paintX += simulation.fieldSize;
                            if (paintX >= simulation.fieldSize)
                                paintX -= simulation.fieldSize;
                            if (paintY < 0)
                                paintY += simulation.fieldSize;
                            if (paintY >= simulation.fieldSize)
                                paintY -= simulation.fieldSize;
                            if (paintX >= 0 && paintY >= 0 && paintX < simulation.fieldSize && paintY < simulation.fieldSize)
                            {
                                int idx = (paintY * simulation.fieldSize + paintX) * 4 + 0;
                                if (erase)
                                    buffer[idx] = 0;
                                else
                                {
                                    float value = buffer[idx];
                                    value += (float)rnd.NextDouble() * 0.5f;
                                    if (value > 1.0f)
                                        value = 1.0f;
                                    buffer[idx] = value;
                                }
                            }
                        }
                    }
                }

                GL.BindTexture(TextureTarget.Texture2D, tex);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, simulation.fieldSize, simulation.fieldSize, PixelFormat.Rgba, PixelType.Float, buffer);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            simulation.gpuContext.glControl.Invalidate();
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
            lock (simulation)
            {
                //clear
                GL.Viewport(0, 0, simulation.gpuContext.glControl.Width, simulation.gpuContext.glControl.Height);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.ClearColor(0f, 0f, 0f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                var channel1Tex = simulation.channels[0].gpu.FieldTex;
                var channel2Tex = simulation.channels.Length >= 2 ? simulation.channels[1].gpu.FieldTex : emptyTex;
                var channel3Tex = simulation.channels.Length == 3 ? simulation.channels[2].gpu.FieldTex : emptyTex;
                simulation.gpuContext.displayProgram.Run(channel1Tex, channel2Tex, channel3Tex, center, zoom, aspectRatio);
                simulation.gpuContext.glControl.SwapBuffers();
                frameCounter++;
            }

            Capture();
        }

        private void Capture()
        {
            //combine PNGs into video:
            //mp4: ffmpeg -f image2 -framerate 60 -i rec1/frame_%05d.png -vf "scale=trunc(iw/2)*2:trunc(ih/2)*2" -r 60 -vcodec libx264 -pix_fmt yuv420p out.mp4 -y
            //gif: ffmpeg -framerate 60 -ss2 -i rec1/frame_%05d.png -vf "select='not(mod(n,2))',setpts=N/FRAME_RATE/TB" -t 5 3ch-tissue.gif
            //reduce bitrate:  ffmpeg -i in.mp4 -c:v libx264 -b:v 4236000 -pass 2 -c:a aac -b:a 128k out.mp4
            var recDir = app.configWindow.recordDir?.ToString();
            if (!recFrameNr.HasValue && !string.IsNullOrWhiteSpace(recDir))
            {
                recFrameNr = 0;
            }

            if (recFrameNr.HasValue && string.IsNullOrWhiteSpace(recDir))
                recFrameNr = null;

            if (recFrameNr.HasValue && !string.IsNullOrWhiteSpace(recDir))
            {
                string recFilename = $"{recDir}\\frame_{recFrameNr.Value.ToString("00000")}.png";
                simulation.gpuContext.glControl.MakeCurrent();
                int width = simulation.gpuContext.glControl.Width;
                int height = simulation.gpuContext.glControl.Height;
                int bufferSize = width * height * 4;
                if (captureBuffer == null || bufferSize != captureBuffer.Length)
                    captureBuffer = new byte[bufferSize];
                GL.ReadPixels(
                    0, 0,
                    width, height,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    captureBuffer
                );

                TextureUtil.SaveBufferToFile(captureBuffer, width, height, recFilename);
                recFrameNr = recFrameNr.Value + 1;
            }
        }

        public void Destroy()
        {
            if (emptyTex != 0) GL.DeleteTexture(emptyTex);
        }
    }
}

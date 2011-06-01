using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using Drawing;

namespace ColorModule
{
    public class Coloring
    {
        private class ColorWindow : GameWindow
        {
            protected override void OnLoad(EventArgs e)
            {
                GL.Viewport(0, 0, 1000, 1000);
                GL.ClearColor(Color.White);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0.0, 1000.0, 0.0, 1000.0, -1.0, 1.0);
            }
//            protected override void OnResize(EventArgs e)
//            {
 //               GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
 //           }
            protected override void OnRenderFrame(FrameEventArgs e)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);
                float radiusValue = 100;
                Point p = new Point(100, 100);
                
                DrawCircle draw = new DrawCircle();
                draw.ColorRandom(p, radiusValue);

                this.SwapBuffers();
            }
}

        [STAThread]
        public static void Main()
        {
            using (ColorWindow window = new ColorWindow())
            {
                window.Title = "Colored Draw";
                window.Size = new Size(1028, 500);
                window.X = 50;
                window.Run(1.0, 2.0);
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace HouseParam
{
    public class Diamond
    {

        private class DiamondWindow : GameWindow
        {
            double red = 0.5;
            double blue = 0.5;
            double green = 0.5;

            protected override void OnLoad(EventArgs e)
            {

                GL.Viewport(0, 0, 1000, 1000);
                GL.ClearColor(Color.Black);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0.0, 640.0, 0.0, 640.0, -1.0, 1.0);
            }

            protected override void OnResize(EventArgs e)
            {
                GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
            }

            protected override void OnUpdateFrame(FrameEventArgs e)
            {
 	            //red up
                if (Keyboard[Key.A])
                {
                    red += 0.05;
                    Console.WriteLine("red, green, blue: " + red + ", " + green + " , " + blue);
                }
                //red down
                if (Keyboard[Key.Z])
                {
                    red -= 0.05;
                    Console.WriteLine("red, green, blue: " + red + ", " + green + " , " + blue);
                }
                //blue up
                if (Keyboard[Key.S])
                {
                    blue += 0.05;
                    Console.WriteLine("red, green, blue: " + red + ", " + green + " , " + blue);
                }
                //blue down
                if (Keyboard[Key.X])
                {
                    blue -= 0.05;
                    Console.WriteLine("red, green, blue: " + red + ", " + green + " , " + blue);
                }
                //green up
                if (Keyboard[Key.D])
                {
                    green += 0.05;
                    Console.WriteLine("red, green, blue: " + red + ", " + green + " , " + blue);
                }
                //s
                if (Keyboard[Key.C])
                {
                    green -= 0.05;
                    Console.WriteLine("red, green, blue: " + red + ", " + green + " , " + blue);
                }
}

            protected override void OnRenderFrame(FrameEventArgs e)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Begin(BeginMode.Polygon);
                GL.Color3(red, green, blue);
                GL.Vertex2(50,50);
                GL.Vertex2(50, 400);
                GL.Vertex2(400,400);
                GL.Vertex2(400,50);
                GL.End();

                this.SwapBuffers();
            }

            
        }

        [STAThread]
        public static void Main()
        {
            using (DiamondWindow window = new DiamondWindow())
            {
                window.Title = "Diamond Draw";
                window.Size = new Size(1028, 500);
                window.X = 50;
                window.Run(5.0, 60.0);
            }
        }
    }
}
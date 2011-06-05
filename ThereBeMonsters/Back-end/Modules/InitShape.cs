using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace ThereBeMonsters.Back_end.Modules
{
    [Module("Initial Shape")]
    class InitShape : Module
    {
        [Parameter("Initial Shape")]
        public Vector2[] Shape { private get; set; }

        public static Vector2[] shape = new Vector2[8];
        public override void Run()
        {
            PointInput.Main();
            Shape = shape;
        }

    class PointInput : GameWindow
    {

        private MouseDevice mouse;
        static int numClick = 0;

        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Viewport(0, 0, 1600, 900);
            GL.ClearColor(Color.Black);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, 1600, 900, 0, 0, 100);
            mouse = Mouse;
            mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            draw();
            if (numClick == 8)
            {                
                Exit();
            }

            this.SwapBuffers();
        }

        private void Mouse_ButtonDown(object s, MouseButtonEventArgs e)
        {
            numClick++;
                shape[numClick - 1] = new Vector2(mouse.X, mouse.Y);
                Console.WriteLine(numClick + " " + shape[numClick - 1]);
        }

        public static void draw()
        {
            GL.Begin(BeginMode.LineStrip);
            for (int i = 0; i < numClick; i++)
            {
                GL.Vertex2(shape[i].X, shape[i].Y);
            }
            GL.Vertex2(shape[0].X, shape[0].Y);
            GL.End();
            GL.Flush();
        }

        [STAThread]
        public static void Main()
        {
            using (PointInput window = new PointInput())
            {
                window.Title = "RAWR";
                window.Size = new Size(1600, 900);
                window.X = 2;
                window.Y = 2;
                window.Run(30.0, 0.0);
            }
        }
    }
  }
}

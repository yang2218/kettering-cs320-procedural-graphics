using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject
{
  public class Hello
  {
    /// <summary>
    /// Private nested class for our OpenGL window.
    /// Inherits from <c>OpenTK.GameWindow</c> so we don't have to do a lot of the
    /// initialization work, and platform-specific code for creating a window.
    /// </summary>
    private class HelloWindow : GameWindow
    {
      /// <summary>
      /// Invoked when window is loaded; setup OpenGL and stuff here.
      /// </summary>
      /// <param name="e">Not used.</param>
      protected override void OnLoad(EventArgs e)
      {
        // Had to add this line (not in the original hello.c,
        // probably something initialized by GLUT but not by GameWindow)
        GL.Viewport(0, 0, 250, 250);

        GL.ClearColor(Color.Black);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(0.0, 1.0, 0.0, 1.0, -1.0, 1.0);
      }

      /// <summary>
      /// Invoked when ready to render stuff; add rendering code here.
      /// </summary>
      /// <param name="e">Contains timing information.</param>
      protected override void OnRenderFrame(FrameEventArgs e)
      {
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.Color3(1.0, 1.0, 1.0);
        GL.Begin(BeginMode.Polygon);
          GL.Vertex3(0.25, 0.25, 0.0);
          GL.Vertex3(0.75, 0.25, 0.0);
          GL.Vertex3(0.75, 0.75, 0.0);
          GL.Vertex3(0.25, 0.75, 0.0);
        GL.End();

        GL.Flush();

        // This line wasn't also in hello.c, but was in the OpenTK example.
        // It might be the case that SwapBuffers implies Flush or the Flush
        // causes a SwapBuffers, but since the image never changes I can't
        // tell what effect either have on their own.
        this.SwapBuffers();
      }
    }

    /// <summary>
    /// Main.
    /// </summary>
    [STAThread]
    public static void Main()
    {
      using (HelloWindow window = new HelloWindow())
      {
        window.Title = "hello";
        window.Size = new Size(250, 250);
        window.X = 500;
        window.Y = 500;
        window.Run(30.0, 0.0);
      }
    }

  }
}

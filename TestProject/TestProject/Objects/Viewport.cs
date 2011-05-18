using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public class Viewport
  {
    // public fields can be passed as ref/out parameters
    public Matrix4 projectionMatrix, viewMatrix;
    public Rectangle ViewportRect { get; set; }

    public static Viewport Active { get; private set; }

    public Viewport(Rectangle rect)
    {
      ViewportRect = rect;
    }

    public void Draw()
    {
      // TODO: tell the scene to draw, but, there's no Scene property yet, let alone a Scene class
      GL.Viewport(ViewportRect);
      Viewport.Active = this;
    }
  }
}

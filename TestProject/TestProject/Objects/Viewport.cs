using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public class Viewport
  {
    // public fields can be passed as ref/out parameters
    public Matrix4 projectionMatrix, viewMatrix, vpMatrix;
    public Rectangle ViewportRect { get; set; }
    public uint ActiveLayers { get; set; }

    public static Viewport Active { get; private set; }

    public Viewport()
    {
      // turn all layers on by default
      ActiveLayers = ~(uint)0;
    }

    public Viewport(Rectangle rect)
      : this()
    {
      ViewportRect = rect;
    }

    public void SetActive()
    {
      GL.Viewport(ViewportRect);
      Viewport.Active = this;
      Matrix4.Mult(ref viewMatrix, ref projectionMatrix, out vpMatrix);
    }
  }
}

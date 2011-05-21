using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ThereBeMonsters.Front_end
{
  public class Viewport
  {
    // public fields can be passed as ref/out parameters
    public Matrix4 projectionMatrix, viewMatrix;
    public Rectangle ViewportRect { get; set; }
    public Scene Scene { get; set; }

    public static Viewport Active { get; private set; }

    public Viewport(Rectangle rect)
    {
      ViewportRect = rect;
    }

    public void Draw()
    {
      GL.Viewport(ViewportRect);
      // TODO: premultiply the projection and view matrix
      Viewport.Active = this;
      this.Scene.Draw();
    }
  }
}

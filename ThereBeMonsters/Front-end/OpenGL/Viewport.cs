using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ThereBeMonsters.Front_end
{
  public class Viewport
  {
    public Rectangle ViewportRect { get; set; }

    public event EventHandler<FrameEventArgs> PreRender;

    public event EventHandler<FrameEventArgs> Render;

    public static Viewport Active { get; private set; }

    public Viewport(Rectangle rect)
    {
      ViewportRect = rect;
    }

    public void Draw(FrameEventArgs e)
    {
      GL.Viewport(ViewportRect);
      if (PreRender != null)
      {
        PreRender(this, e);
      }

      if (Render != null)
      {
        Render(this, e);
      }
    }
  }
}

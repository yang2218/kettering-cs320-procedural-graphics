using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ThereBeMonsters.Front_end
{
  public class Viewport
  {
    public Rectangle ViewportRect { get; set; }

    public event EventHandler PreRender;

    public event EventHandler Render;

    public static Viewport Active { get; private set; }

    public Viewport(Rectangle rect)
    {
      ViewportRect = rect;
    }

    public void Draw()
    {
      GL.Viewport(ViewportRect);
      if (PreRender != null)
      {
        PreRender(this, new EventArgs());
      }

      if (Render != null)
      {
        Render(this, new EventArgs());
      }
    }
  }
}

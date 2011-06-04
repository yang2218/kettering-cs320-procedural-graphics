using System;
using System.Collections.Generic;
using OpenTKGUI;

namespace ThereBeMonsters.Front_end
{
  public class ViewportPlaceholder : Control
  {
    public Viewport Viewport { get; set; }

    public ViewportPlaceholder(Viewport v)
    {
      this.Viewport = v;
    }

    public override void Update(GUIControlContext Context, double Time)
    {
      Viewport.ViewportRect = new System.Drawing.Rectangle(
        (int)Context.Offset.X,
        (int)(MainWindow.Active.ClientSize.Height - Context.Offset.Y - this.Size.Y),
        (int)this.Size.X,
        (int)this.Size.Y);
    }
  }
}

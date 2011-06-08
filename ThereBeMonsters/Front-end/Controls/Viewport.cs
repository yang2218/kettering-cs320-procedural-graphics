using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTKGUI;

namespace ThereBeMonsters.Front_end
{
  public class Viewport : Render3DControl
  {
    public class PreRenderEventArgs : EventArgs
    {
      public Point ViewSize { get; private set; }

      public PreRenderEventArgs(Point viewsize)
      {
        this.ViewSize = viewsize;
      }
    }

    public class UpdateEventArgs : EventArgs
    {
      public GUIControlContext Context { get; private set; }
      public float Time { get; private set; }

      public UpdateEventArgs(GUIControlContext context, float time)
      {
        this.Context = context;
        this.Time = time;
      }
    }

    public event EventHandler<PreRenderEventArgs> PreRender;
    public event EventHandler OnRender;
    public event EventHandler PostRender;

    public event EventHandler<UpdateEventArgs> OnUpdate;

    private Point _lastOffset;

    public static Viewport Active { get; private set; }

    public Viewport()
    {
      Viewport.Active = this;
    }

    public override void Update(GUIControlContext Context, double Time)
    {
      _lastOffset = Context.Offset;

      if (OnUpdate != null)
      {
        OnUpdate(this, new UpdateEventArgs(Context, (float)Time));
      }
    }

    public override void SetupProjection(Point viewsize)
    {
      // TODO: save some GL state
      GL.Viewport(
        (int)_lastOffset.X,
        (int)(MainWindow.Active.ClientSize.Height - _lastOffset.Y - this.Size.Y),
        (int)this.Size.X,
        (int)this.Size.Y);
      GL.MatrixMode(MatrixMode.Modelview);
      GL.PushMatrix(); //projection already saved by Render3DControl
      GL.Disable(EnableCap.Texture2D);
      GL.Disable(EnableCap.Blend);
      GL.Disable(EnableCap.ScissorTest);

      if (PreRender != null)
      {
        PreRender(this, new PreRenderEventArgs(viewsize));
      }
    }

    public override void RenderScene()
    {
      if (OnRender != null)
      {
        OnRender(this, new EventArgs());
      }

      if (PostRender != null)
      {
        PostRender(this, new EventArgs());
      }

      // TODO: restore some GL state
      GL.Viewport(MainWindow.Active.ClientRectangle);
      GL.MatrixMode(MatrixMode.Modelview);
      GL.PopMatrix();
      GL.Enable(EnableCap.Blend);
      GL.Enable(EnableCap.ScissorTest);
    }
  }
}

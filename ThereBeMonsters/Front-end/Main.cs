using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTKGUI;

namespace ThereBeMonsters.Front_end
{
  public class MainWindow : HostWindow
  {
    public List<Viewport> Viewports { get; private set; }

    public System.Drawing.Color ClearColor
    {
      set
      {
        GL.ClearColor(value);
      }
    }

    public MainWindow()
      : base(SetupControls, "Don't Blow a Gasket!")
    {
      this.ClientSize = new System.Drawing.Size(1024, 768);

      Viewports = new List<Viewport>();
      ClearColor = System.Drawing.Color.Black;

      Scene s = new Scene();
      s.Entities.AddLast(new TestEntity());

      Viewport v = new Viewport(System.Drawing.Rectangle.Empty);
      v.Scene = s;
      Matrix4.CreateOrthographic(1.1f, 1.1f, -1f, 1f, out v.projectionMatrix);
      v.viewMatrix = Matrix4.Identity;
      Viewports.Add(v);

      vp.Viewport = v;
    }

    // HACK
    private static ViewportPlaceholder vp;

    private static Control SetupControls()
    {
      // TODO: build the control heiarchy here

      // lower part of split pane
      LayerContainer lc = new LayerContainer(null);
      vp = new ViewportPlaceholder(null);
      AlignContainer ac = new AlignContainer(vp, new Point(200, 200), Align.Center, Align.Bottom);
      Form p = new Form(ac, "Test");
      p.ClientSize = new Point(250, 300);
      lc.AddControl(p, new Point(10, 10));

      SplitContainer sc = new SplitContainer(Axis.Vertical, new Button("test"), lc);
      sc.NearSize = 300;
      return sc;
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
      base.OnKeyPress(e);
      if (Keyboard[OpenTK.Input.Key.Escape])
      {
        Exit();
      }
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Clear(ClearBufferMask.ColorBufferBit);

      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadIdentity();

      GUIRenderContext rc = new GUIRenderContext(this.ViewSize);
      rc.Setup();
      this.Control.Render(rc);

      //GL.Disable(EnableCap.ScissorTest);
      //GL.Disable(EnableCap.DepthTest);
      GL.Disable(EnableCap.Texture2D);
      
      foreach (Viewport v in Viewports)
      {
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadMatrix(ref v.projectionMatrix);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadMatrix(ref v.viewMatrix);
        v.Draw();
      }

      this.SwapBuffers();
    }

    public static void Main()
    {
      using (MainWindow w = new MainWindow())
      {
        w.Run();
      }
    }
  }
}

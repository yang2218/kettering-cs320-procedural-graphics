using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTKGUI;

namespace ThereBeMonsters.Front_end
{
  public class MainWindow : HostWindow
  {
    public Viewport Viewport { get; private set; }

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

      Viewport = new Viewport(System.Drawing.Rectangle.Empty);
      ClearColor = System.Drawing.Color.Black;

      vp.Viewport = Viewport;
    }

    // HACK
    private static ViewportPlaceholder vp;

    private static Control SetupControls()
    {
      // TODO: build the control heiarchy here

      // lower part of split pane
      LayerContainer lc = new LayerContainer(null);
      vp = new ViewportPlaceholder(null);
      AlignContainer ac = new AlignContainer(new Button("test"), new Point(200, 200), Align.Center, Align.Bottom);
      Form p = new Form(ac, "Test");
      p.ClientSize = new Point(250, 300);
      lc.AddControl(p, new Point(10, 10));

      SplitContainer sc = new SplitContainer(Axis.Vertical, vp, lc);
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
      
      GL.Disable(EnableCap.Texture2D);
      
      Viewport.Draw();
      
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

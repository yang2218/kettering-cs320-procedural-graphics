using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTKGUI;
using ThereBeMonsters.Front_end.OpenGL;
using ThereBeMonsters.Back_end.Modules;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Front_end
{
  public class MainWindow : HostWindow
  {
    public class ResizeBorder : BorderContainer
    {
      private int _yOffset;

      public ResizeBorder(Control client)
        : base(client)
      {
        this.Set(0, 0, 0, 3);
        this.Color = Color.RGB(0.5, 0.5, 0.5);
      }

      public override void Update(GUIControlContext Context, double Time)
      {
        base.Update(Context, Time);

        MouseState ms = Context.MouseState;
        if (ms == null)
        {
          return;
        }

        if (Context.HasMouse)
        {
          MainWindow.Active.Split.NearSize = MainWindow.Active.Mouse.Y - _yOffset;
          MainWindow.Active.Split.ForceResize(MainWindow.Active.ViewSize);
          if (ms.HasReleasedButton(OpenTK.Input.MouseButton.Left))
          {
            Context.ReleaseMouse();
          }
        }
        else
        {
          if (ms.Position.Y >= this.Size.Y - this.Bottom
            && ms.HasPushedButton(OpenTK.Input.MouseButton.Left))
          {
            _yOffset = MainWindow.Active.Mouse.Y - (int)MainWindow.Active.Split.NearSize;
            Context.CaptureMouse();
          }
        }
      }
    }

    public static MainWindow Active { get; private set; }

    public Viewport Viewport { get; private set; }
    public ModuleGraphControl GraphControl { get; private set; }
    public SplitContainer Split { get; private set; }

    public System.Drawing.Color ClearColor
    {
      set
      {
        GL.ClearColor(value);
      }
    }

    private static IEnumerable<Vector3> CirlceFilter(IEnumerable<Vector3> input)
    {
      foreach (Vector3 v in input)
      {
        if (v.Z > 0f && v.Z < 1f)
        {
          yield return v;
        }
      }
    }

    public static void GenerateTest()
    {
      Gasket cookie = new Gasket();
      cookie.InitialShapePoints = new List<Vector2> {
        new Vector2(0f, 0f),
        new Vector2(1f, 0f),
        new Vector2(1f, 1f),
        new Vector2(0f, 1f),
        new Vector2(0f, 0f)
      };
      cookie.MaxDepth = 7;
      cookie.Run();

      ExtrudeCirclesToHeight extruder = new ExtrudeCirclesToHeight();
      extruder.CapMode = ExtrudeCirclesToHeight.Cap.Hemisphere;
      extruder.Circles = CirlceFilter(cookie.Circles);
      extruder.HeightMap = new byte[64, 64];
      extruder.ScaleMode = ExtrudeCirclesToHeight.Scale.Quadradic;
      extruder.BlendFunc = Blend8bppFunc.Additive;
      extruder.BlendFuncSrcFactor = 1f;
      extruder.BlendFuncDstFactor = 1f;
      extruder.Run();

      CreateTerrain creator = new CreateTerrain();
      creator.HeightMap = extruder.HeightMap;
      creator.Run();

    }

    public MainWindow()
      : base(() => { return null; }, "Don't Blow a Gasket!")
    {
      this.ClientSize = new System.Drawing.Size(1024, 768);
      Active = this;
      Viewport = new Viewport(System.Drawing.Rectangle.Empty);
      ClearColor = System.Drawing.Color.Black;

      this.Control = SetupControls();

      Camera c = new Camera();
      Viewport.PreRender += c.SetupCamera;

      GenerateTest();
    }

    private Control SetupControls()
    {
      GraphControl = new ModuleGraphControl(ModuleGraph.LoadFromXml("save.xml"));
      ModuleGraphWindowControl wc = new ModuleGraphWindowControl(GraphControl);
      wc.FullSize = new Point(double.MaxValue, double.MaxValue);
      wc.Offset = new Point(0, 0);

      ViewportPlaceholder vp = new ViewportPlaceholder(Viewport);
      ResizeBorder rb = new ResizeBorder(vp);

      Split = new SplitContainer(Axis.Vertical, rb, wc);
      Split.NearSize = 300;
      return Split;
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
      base.OnKeyPress(e);
      if (Keyboard[OpenTK.Input.Key.Escape])
      {
        Exit();
      }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      base.OnClosing(e);
      GraphControl.Graph.SaveToXml("save.xml");
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
      GL.Disable(EnableCap.Blend);

      GL.MatrixMode(MatrixMode.Modelview);
      GL.PushMatrix();

      Viewport.Draw(e);
      
      GL.MatrixMode(MatrixMode.Modelview);
      GL.PopMatrix();

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

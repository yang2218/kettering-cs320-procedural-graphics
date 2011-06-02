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
    public static MainWindow hack; 
    public Viewport Viewport { get; private set; }

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
      extruder.BlendFunc = Blend8bppFunctions.Additive;
      extruder.BlendFuncSrcFactor = 1f;
      extruder.BlendFuncDstFactor = 1f;
      extruder.Run();

      CreateTerrain creator = new CreateTerrain();
      creator.HeightMap = extruder.HeightMap;
      creator.Run();

    }

    public MainWindow()
      : base(SetupControls, "Don't Blow a Gasket!")
    {
      this.ClientSize = new System.Drawing.Size(1024, 768);
      hack = this;
      Viewport = new Viewport(System.Drawing.Rectangle.Empty);
      ClearColor = System.Drawing.Color.Black;

      vp.Viewport = Viewport;

      Camera c = new Camera();
      Viewport.PreRender += c.SetupCamera;

      GenerateTest();
    }

    // HACK
    private static ViewportPlaceholder vp;
    private static ModuleGraphControl g;
    private static Control SetupControls()
    {
      // TODO: build the control heiarchy here
      g = new ModuleGraphControl(ModuleGraph.LoadFromXml("save.xml"));

      vp = new ViewportPlaceholder(null);
      
      SplitContainer sc = new SplitContainer(Axis.Vertical, vp, g);
      sc.NearSize = 450;
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

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      base.OnClosing(e);
      //g.Graph.SaveToXml("save.xml");
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

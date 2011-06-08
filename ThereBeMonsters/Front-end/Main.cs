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
      public SplitContainer BottomSplit { get; set; }
      public SplitContainer RightSplit { get; set; }

      private int? _xOffset, _yOffset;

      public ResizeBorder(Control client)
        : base(client)
      {
        this.Set(0, 0, 4, 4);
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
          if (_xOffset.HasValue)
          {
            RightSplit.NearSize = MainWindow.Active.Mouse.X - _xOffset.Value;
            RightSplit.ForceResize(RightSplit.Size);
          }

          if (_yOffset.HasValue)
          {
            BottomSplit.NearSize = MainWindow.Active.Mouse.Y - _yOffset.Value;
            BottomSplit.ForceResize(BottomSplit.Size);
          }

          if (ms.HasReleasedButton(OpenTK.Input.MouseButton.Left))
          {
            Context.ReleaseMouse();
            _xOffset = _yOffset = null;
          }

          return;
        }

        if (ms.HasPushedButton(OpenTK.Input.MouseButton.Left) == false)
        {
          return;
        }

        if (ms.Position.X >= this.Size.X - this.Right)
        {
          _xOffset = MainWindow.Active.Mouse.X - (int)RightSplit.NearSize;
          Context.CaptureMouse();
        }

        if (ms.Position.Y >= this.Size.Y - this.Bottom)
        {
          _yOffset = MainWindow.Active.Mouse.Y - (int)BottomSplit.NearSize;
          Context.CaptureMouse();
        }
      }
    }

    public static MainWindow Active { get; private set; }

    public ModuleGraphControl GraphControl { get; private set; }
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
      extruder.HeightMap = new byte[512, 512];
      extruder.ScaleMode = ExtrudeCirclesToHeight.Scale.Quadradic;
      extruder.BlendFunc = Blend8bppFunc.Additive;
      extruder.BlendFuncSrcFactor = 1f;
      extruder.BlendFuncDstFactor = 1f;
      extruder.Run();

      ColorCircle painter = new ColorCircle();
      painter.Circles = CirlceFilter(cookie.Circles);
      painter.ColorMap = new uint[512, 512];
      painter.BlendFunc = Blend32bppFunc.Additive;
      painter.BlendFuncSrcFactor = 1f;
      painter.BlendFuncDstFactor = 1f;
      //painter.Color = Color.Pink;
      painter.Run();

      CreateTerrain creator = new CreateTerrain();
      creator.HeightMap = extruder.HeightMap;
      creator.ColorMap = painter.ColorMap;
      creator.BaseScale = 10f;
      creator.HeightScale = 5f;
      creator.Run();

    }

    public MainWindow()
      : base(() => { return null; }, "Don't Blow a Gasket!")
    {
      this.ClientSize = new System.Drawing.Size(1024, 768);
      Active = this;
      ClearColor = System.Drawing.Color.Black;

      this.Control = SetupControls();

      Camera c = new Camera();
      Viewport.PreRender += c.SetupCamera;
      Viewport.OnUpdate += c.Update;

      //GenerateTest();
    }

    private Control SetupControls()
    {
      GraphControl = new ModuleGraphControl(ModuleGraph.LoadFromXml("save.xml"));
      ModuleGraphWindowControl wc = new ModuleGraphWindowControl(GraphControl);
      wc.FullSize = new Point(float.MaxValue, float.MaxValue);
      wc.Offset = new Point(0, 0);

      this.Viewport = new Viewport();
      ResizeBorder rb = new ResizeBorder(Viewport);

      Point suggestSize;
      double controlPanelWidth;

      FlowContainer controlPanel = new FlowContainer(5, Axis.Vertical);
      LabelStyle fontStyle = new LabelStyle
      {
        HorizontalAlign = TextAlign.Center,
        Font = new SystemFont("Verdana", 18, true)
      };
      Label l = new Label("\"Don't Blow a Gasket!\"", Color.RGB(1, 0.2, 0.2), fontStyle);
      suggestSize = l.SuggestSize;
      controlPanelWidth = suggestSize.X + 50;
      controlPanel.AddChild(l, suggestSize.Y);

      LogoControl logo = new LogoControl("oblongGasketLogo.jpg");
      controlPanel.AddChild(logo, controlPanelWidth / 2.0);

      CreditScroller scroller = new CreditScroller("Alec \"アィク\" Emmett - Bob \"BCrums\" Crumley - Charlie Welch - Colby \"Cpt. Jack\" Poe - Ethan Koester - Matthew Orlando - Rebeccah \"Javalicious\" MacKinnon");
      controlPanel.AddChild(scroller, scroller.FullSize.Y);

      Button b;
      b = new Button("Run Module Graph");
      b.Click += () =>
        {
          Generator gen = new Generator(GraphControl.Graph);
          gen.RunGraph();
        };
      controlPanel.AddChild(b, 25);

      b = new Button("Load Module Graph");
      b.Click += () =>
        {
          ModuleGraph graph = null;
          try
          {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "XML files (*.xml)|*.xml|All files|*.*";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
              graph = ModuleGraph.LoadFromXml(ofd.FileName);
            }

          }
          catch (Exception) { }

          if (graph != null)
          {
            GraphControl.Graph = graph;
          }
        };
      controlPanel.AddChild(b, 25);

      b = new Button("Save Module Graph");
      b.Click += () =>
        {
          System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
          sfd.AddExtension = true;
          sfd.Filter = "XML files (*.xml)|*.xml|All files|*.*";
          if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
          {
            GraphControl.Graph.SaveToXml(sfd.FileName);
          }
        };
      controlPanel.AddChild(b, 25);

      SplitContainer split1 = new SplitContainer(Axis.Horizontal, rb, controlPanel.WithMargin(3));
      split1.NearSize = 700;
      SplitContainer split2 = new SplitContainer(Axis.Vertical, split1, wc);
      split2.NearSize = 300;

      rb.RightSplit = split1;
      rb.BottomSplit = split2;

      return split2;
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

      this.SwapBuffers();
    }

    [STAThread]
    public static void Main()
    {
      using (MainWindow w = new MainWindow())
      {
        w.Run();
      }
    }

    public class LogoControl : Control
    {
      private int _textureHandle;

      public LogoControl(string logoPath)
      {
        _textureHandle = LoadBitmapTexture(logoPath);
      }

      /// <summary>
      /// Uploads the specified image file as a texture.
      /// Taken from http://www.opentk.com/doc/graphics/textures/loading
      /// </summary>
      /// <param name="filePath">Path to the image file to load.</param>
      /// <returns>The ID of the uploaded texture.</returns>
      public static int LoadBitmapTexture(string filePath)
      {
        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(filePath);

        System.Drawing.Size _bitmapSize = new System.Drawing.Size(bmp.Width, bmp.Height);

        System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
          new System.Drawing.Rectangle(System.Drawing.Point.Empty, _bitmapSize),
          System.Drawing.Imaging.ImageLockMode.ReadOnly,
          System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        // Get the address of the first line.
        IntPtr ptr = bmpData.Scan0;

        // Declare an array to hold the bytes of the bitmap.
        int numPixels = Math.Abs(bmpData.Stride) * bmp.Height / 4;
        int[] _bitmapData = new int[numPixels];

        // Copy the RGB values into the array.
        System.Runtime.InteropServices.Marshal.Copy(ptr, _bitmapData, 0, numPixels);

        bmp.UnlockBits(bmpData);
        bmp.Dispose();

        int id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, id);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0,
            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, _bitmapData);

        // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
        // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
        // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        return id;
      }


      public override void Render(GUIRenderContext Context)
      {
        Context.DrawTexture(_textureHandle, new Rectangle(this.Size));
      }
    }

    public class CreditScroller : WindowContainer
    {
      private float _elapsedCreditScrollTime;

      public CreditScroller(string credits)
        : base(new Label(credits, Color.RGB(1, 1, 1)))
      {
        Label l = (Label)Client;
        this.FullSize = l.SuggestSize;
      }

      public override void Update(GUIControlContext Context, double Time)
      {
        base.Update(Context, Time);

        _elapsedCreditScrollTime += (float)Time;
        if (_elapsedCreditScrollTime > 34f)
        {
          _elapsedCreditScrollTime -= 34f;
        }

        if (_elapsedCreditScrollTime > 19f)
        {
          this.Offset = new Point((1f - (_elapsedCreditScrollTime - 19f) / 15f) * (this.FullSize.X - this.Size.X), 0);
        }
        else if (_elapsedCreditScrollTime > 2f && _elapsedCreditScrollTime < 17f)
        {
          this.Offset = new Point((_elapsedCreditScrollTime - 2f) / 15f * (this.FullSize.X - this.Size.X), 0);
        }
      }
    }
  }
}

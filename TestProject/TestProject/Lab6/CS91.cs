using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using TestProject.Objects;
using TestProject.Objects.Shaders;

namespace TestProject.Lab6
{
  public class CS91 : GameWindow
  {
    private Viewport _viewport;

    public class RgbPixmap
    {
      private int? _textureHandle;
      public Size Size { get; private set; }
      private int[] _bitmapData;

      public bool IsLoaded
      {
        get
        {
          return _textureHandle.HasValue;
        }
      }

      public int TextureHandle
      {
        get
        {
          return _textureHandle.Value;
        }
        private set
        {
          _textureHandle = value;
        }
      }

      public RgbPixmap()
      {
      }

      public RgbPixmap(string filepath)
      {
        ReadBmpFile(filepath);
      }

      public void Draw()
      {

      }

      public void Read()
      {
      }

      public RgbPixmap Copy(Rectangle subrange)
      {
        RgbPixmap ret = new RgbPixmap();
        int ci = subrange.Height < 0 ? -1 : 1,
          cj = subrange.Width< 0 ? -1 : 1;
        ret.Size = new Size(subrange.Width * ci, subrange.Height * cj);
        ret._bitmapData = new int[ret.Size.Width * ret.Size.Height];
        int r = 0, c;
        for (int i = subrange.Top; i != subrange.Bottom; i += ci)
        {
          c = 0;
          for (int j = subrange.Left; j != subrange.Right; j += cj)
          {
            ret._bitmapData[r * ret.Size.Width + c++] =
              this._bitmapData[i * this.Size.Width + j];
          }

          r++;
        }

        return ret;
      }

      /// <summary>
      /// Uploads the specified image data as a texture.
      /// Taken from http://www.opentk.com/doc/graphics/textures/loading and
      /// http://msdn.microsoft.com/en-us/library/system.drawing.imaging.bitmapdata.aspx
      /// </summary>
      /// <param name="image">The bitmap data</param>
      public void ReadBmpFile(Stream image)
      {
        Bitmap bmp = new Bitmap(image);

        Size = new Size(bmp.Width, bmp.Height);

        BitmapData bmpData = bmp.LockBits(new Rectangle(Point.Empty, Size),
          ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        // Get the address of the first line.
        IntPtr ptr = bmpData.Scan0;

        // Declare an array to hold the bytes of the bitmap.
        int numPixels = Math.Abs(bmpData.Stride) * bmp.Height / 4;
        _bitmapData = new int[numPixels];

        // Copy the RGB values into the array.
        System.Runtime.InteropServices.Marshal.Copy(ptr, _bitmapData, 0, numPixels);

        bmp.UnlockBits(bmpData);
        bmp.Dispose();

        TextureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0,
            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, _bitmapData);

        // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
        // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
        // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
      }

      public void ReadBmpFile(string filePath)
      {
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
          ReadBmpFile(fs);
        }
      }

      public void WriteBmpFile()
      {
      }
    }

    public class PixmapEntity : Entity
    {
      public RgbPixmap Pixmap { get; private set; }
      public CS91 Parent { get; private set; }
      public bool IsAnimating { get; private set; }

      public PixmapEntity(CS91 parent, RgbPixmap pixmap)
        : base()
      {
        this.Parent = parent;
        this.Pixmap = pixmap;
        this.VertexData = vertexData;
        this.Material = Material.Cache["VT_Tex"];
        this["textureMap"] = pixmap.TextureHandle;
        this.transform = new Transform();
      }

      public bool TestMouseInside(ref Vector2 result)
      {

        return false;
      }

      public void StartPopAnimation()
      {
        if (IsAnimating)
        {
          return;
        }

        IsAnimating = true;
        Vector3 targetScale = new Vector3(Pixmap.Size.Width, Pixmap.Size.Height, 0f);
        targetScale.Normalize();
        targetScale.Z = 1f;
        int skipFrames = 10;
        transform.Scale = Vector3.Zero;
        float animTime = 0f;
        EventHandler<FrameEventArgs> eh = null;
        eh = (sender, e) =>
        {
          if (skipFrames > 0)
          {
            skipFrames--;
            return;
          }

          float frameTime = (float)e.Time;
          animTime += frameTime;
          if (animTime > 1f)
          {
            Parent.UpdateFrame -= eh;
            transform.Scale = targetScale;
            IsAnimating = false;
            return;
          }

          transform.Scale = Vector3.Lerp(transform.Scale, targetScale, frameTime * 5f);
        };
        Parent.UpdateFrame += eh;
      }

      public void StartFlipAnimation(bool vertical)
      {
        if (IsAnimating)
        {
          return;
        }

        IsAnimating = true;
        float animTime = 0f;
        Quaternion baseRot = transform.Rotation;
        Vector4 rotAngleAxis = new Vector4(vertical ? Vector3.UnitX : Vector3.UnitY, 0f);
        EventHandler<FrameEventArgs> eh = null;
        eh = (sender, e) =>
        {
          float frameTime = (float)e.Time;
          animTime += frameTime;
          if (animTime > 0.5f)
          {
            Parent.UpdateFrame -= eh;
            rotAngleAxis.W = MathHelper.Pi;
            transform.Rotation = baseRot * Quaternion.FromAxisAngle(rotAngleAxis.Xyz, rotAngleAxis.W);
            IsAnimating = false;
            return;
          }

          rotAngleAxis.W += (MathHelper.Pi - rotAngleAxis.W) * frameTime * 10f;
          transform.Rotation = baseRot * Quaternion.FromAxisAngle(rotAngleAxis.Xyz, rotAngleAxis.W);
        };
        Parent.UpdateFrame += eh;
      }

      public void OnMouseDown(object sender, MouseButtonEventArgs e)
      {

      }

      
    }

    public class Camera
    {
      public CS91 Parent { get; set; }
      public Viewport Viewport { get; set; }
      public Vector2 OrthoSize { get; set; }
      public bool UseOrthoProjection { get; set; }
      public Vector3 pos, forward, right, lean;
      public Vector3 velocity;
      public float Speed { get; set; }
      public float SmoothFactor { get; set; }
      public float LookSpeed { get; set; }
      public float ZoomSpeed { get; set; }

      private float _fov, _fovTarget;
      public float Fov
      {
        get
        {
          return _fov;
        }
        set
        {
          _fov = _fovTarget = value;
        }
      }

      private Vector2 _lookThrottle = Vector2.Zero;

      public Camera(CS91 parent, Viewport viewport)
      {
        this.Parent = parent;
        this.Viewport = viewport;
      }

      public void OnRenderFrame(object sender, FrameEventArgs e)
      {
        float frameTime = (float)e.Time;
        GameWindow w = (GameWindow)sender;
        
        // change position based on velocity
        /*
        Vector3 deltaPos = velocity * frameTime;
        Vector3.Add(ref pos, ref deltaPos, out pos);
        */
        // Pitch, yaw rotations based on mouse
        // code "inspired" from http://www.opentk.com/node/952?page=1

        if (Math.Abs(_fov - _fovTarget) > 0.001f)
        {
          _fov += (_fovTarget - _fov) * frameTime * 5f;
          Matrix4.CreatePerspectiveFieldOfView(
            _fov,
            (float)Parent.ClientSize.Width / Parent.ClientSize.Height,
            0.1f,
            75f,
            out Viewport.projectionMatrix);
        }
        
        Vector2 targetLookThrottle = Vector2.Zero;
        if (w.Mouse.X < 150)
        {
          targetLookThrottle.X = (w.Mouse.X - 150) / 100f;
        }
        else if (w.Mouse.X > w.ClientSize.Width - 150)
        {
          targetLookThrottle.X = (w.Mouse.X - w.ClientSize.Width + 150) / 150f;
        }

        if (w.Mouse.Y < 100)
        {
          targetLookThrottle.Y = (w.Mouse.Y - 100) / 100f;
        }
        else if (w.Mouse.Y > w.ClientSize.Height - 100)
        {
          targetLookThrottle.Y = (w.Mouse.Y - w.ClientSize.Height + 100) / 100f;
        }

        Vector2.Lerp(ref _lookThrottle, ref targetLookThrottle, frameTime * 5f, out _lookThrottle);

        if (_lookThrottle.LengthSquared < 0.00001f)
        {
          return;
        }

        float scrollSpeed = -LookSpeed * _fov / 2.4f;

        Vector3 up = Vector3.UnitY;
        Quaternion xrot = Quaternion.FromAxisAngle(right, _lookThrottle.Y * scrollSpeed);
        Quaternion yrot = Quaternion.FromAxisAngle(up, _lookThrottle.X * scrollSpeed);
        Quaternion rot;
        Quaternion.Multiply(ref xrot, ref yrot, out rot);

        Vector3.Transform(ref forward, ref rot, out forward);
        Vector3.Cross(ref forward, ref up, out right);

        Viewport.viewMatrix = Matrix4.LookAt(
          pos,
          pos + forward,
          lean);
      }

      public void OnMouseWheel(object sender, MouseWheelEventArgs e)
      {
        _fovTarget -= ZoomSpeed * e.Delta;
        _fovTarget = _fovTarget.Clamp(0.001f, MathHelper.Pi - 0.001f);
      }
    }

    private Camera _camera;

    public static VTiData vertexData;
    private List<PixmapEntity> _loadedPixmaps = new List<PixmapEntity>();

    // half-sphere scaled by total image area
    // potential fields to self-arrange images?

    protected override void OnLoad(EventArgs e)
    {
      GL.ClearColor(Color.Black);

      vertexData = new VTiData();
      vertexData.Load("Square");
      vertexData.Update();

      _viewport = new Viewport(ClientRectangle);
      
      _camera = new Camera(this, _viewport)
      {
        Fov = (MathHelper.Pi / 2.4f), // 75 degrees
        OrthoSize = new Vector2(1f, 1f),
        pos = new Vector3(0f, 0f, -1f),
        forward = Vector3.UnitZ,
        lean = Vector3.UnitY,
        Speed = 5f,
        SmoothFactor = 10f,
        LookSpeed = 0.05f,
        ZoomSpeed = 0.07f
      };

      _viewport.viewMatrix = Matrix4.LookAt(
        _camera.pos,
        _camera.pos + _camera.forward,
        _camera.lean);

      RenderFrame += _camera.OnRenderFrame;
      Mouse.WheelChanged += _camera.OnMouseWheel;

      // enable bits: depth test, etc.
      GL.Enable(EnableCap.DepthTest);
      GL.DepthFunc(DepthFunction.Less);

      ResetCursorPos();
    }

    private void ResetCursorPos()
    {
      System.Windows.Forms.Cursor.Position = new Point(
        (Bounds.Left + Bounds.Right) / 2,
        (Bounds.Top + Bounds.Bottom) / 2);
    }

    protected override void OnResize(EventArgs e)
    {
      base.OnResize(e);

      Matrix4.CreatePerspectiveFieldOfView(
        _camera.Fov,
        (float)ClientSize.Width / ClientSize.Height,
        0.1f,
        75f,
        out _viewport.projectionMatrix);
    }

    protected override void OnKeyPress(OpenTK.KeyPressEventArgs e)
    {
      base.OnKeyPress(e);

      switch (e.KeyChar)
      {
        case 'o':
          {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Bitmap files (*.bmp)|*.bmp|All files|*.*";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
              foreach (string filepath in ofd.FileNames)
              {
                PixmapEntity pe = new PixmapEntity(this, new RgbPixmap(filepath));
                _loadedPixmaps.Add(pe);
                pe.StartPopAnimation();
              }
            }

            ResetCursorPos();
          }
          break;
        case 'q':
          Exit();
          break;
        case 'e':
          _camera.forward = Vector3.UnitZ;
          _viewport.viewMatrix = Matrix4.LookAt(
            _camera.pos,
            _camera.pos + _camera.forward,
            _camera.lean);
          break;
        case 't':
          _loadedPixmaps[0]["minBox"] = new Vector2(0.4f, 0.4f);
          _loadedPixmaps[0]["maxBox"] = new Vector2(0.6f, 0.6f);
          _loadedPixmaps[0]["boxColor"] = new Vector4(0f, 1f, 1f, 0.5f);
          break;
        case 'c':
          // unload image
          _loadedPixmaps[0].VertexData = null;
          _loadedPixmaps.RemoveAt(0);
          break;
        case 'r':
          _loadedPixmaps[0].transform.ResetRotation();
          break;
        case 'f':
          // horizontal flip of active image
          _loadedPixmaps[0].StartFlipAnimation(false);
          break;
        case 'v':
          // vertical flip of active image
          _loadedPixmaps[0].StartFlipAnimation(true);
          break;
        default:
          break;
      }
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);

      if (Keyboard[Key.Escape])
      {
        Exit();
      }

      float frameTime = (float)e.Time;
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);

      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

      _viewport.SetActive();
      
      VertexData.DrawAllVertexData();

      this.SwapBuffers();
    }

    [STAThread]
    public static void Main()
    {
      using (CS91 win = new CS91())
      {
        win.ClientSize = new Size(1024, 768);
        win.Title = "Case Study 9.1";
        win.Run(30, 0);
      }
    }
  }
}

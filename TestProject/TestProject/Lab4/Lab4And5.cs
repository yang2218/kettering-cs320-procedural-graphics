using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using TestProject.Objects;
using TestProject.Objects.Shaders;

namespace TestProject.Lab4
{
  public class Lab4And5 : GameWindow
  {
    /* units = inches
     * 15x15x72 viewer (invisible)
     * 120x120 room
     *  procedurally animated floor texture
     *  different textures on each wall
     *  texture on ceiling
     * light blub hanging off of ceiling
     * 48x24x36 table
     *  6x6 teapot
     *   environment mapping
     *  desklamp (light source)
     *  cube
     *  cylinder
     *  laptop w/ security camera feed
     * security camera in upper corner of room, panning
     *  cube, cylinder, small glowing red cylinder
     */

    private Viewport _viewport;

    public class Camera
    {
      public float fov;
      public Vector3 pos, forward, right, lean;
      public Vector3 velocity;
      public float speed, smoothFactor;
      public float lookSpeed;

      public void OnRenderFrame(object sender, FrameEventArgs e)
      {
        float frameTime = (float)e.Time;
        GameWindow w = (GameWindow)sender;

        // change position based on velocity

        Vector3 deltaPos = velocity * frameTime;
        Vector3.Add(ref pos, ref deltaPos, out pos);

        // Pitch, yaw rotations based on mouse
        // code "inspired" from http://www.opentk.com/node/952?page=1

        Vector2 mouseDelta = new Vector2(
          System.Windows.Forms.Cursor.Position.X - (w.Bounds.Left + w.Bounds.Right) / 2,
          System.Windows.Forms.Cursor.Position.Y - (w.Bounds.Top + w.Bounds.Bottom) / 2);

        System.Windows.Forms.Cursor.Position = new Point(
          (w.Bounds.Left + w.Bounds.Right) / 2,
          (w.Bounds.Top + w.Bounds.Bottom) / 2);

        Vector3 up = Vector3.UnitY;
        Quaternion xrot = Quaternion.FromAxisAngle(right, mouseDelta.Y * -lookSpeed);
        Quaternion yrot = Quaternion.FromAxisAngle(up, mouseDelta.X * -lookSpeed);
        Quaternion rot;
        Quaternion.Multiply(ref xrot, ref yrot, out rot);

        Vector3.Transform(ref forward, ref rot, out forward);
        Vector3.Cross(ref forward, ref up, out right);
      }

      public void OnUpdateFrame(object sender, FrameEventArgs e)
      {
        float frameTime = (float)e.Time;
        GameWindow w = (GameWindow)sender;

        // forward, side motion based on wasd and current orientation

        Vector3 targetVelocity = Vector3.Zero;

        if (w.Keyboard[Key.W])
        {
          targetVelocity += speed * forward;
        }

        if (w.Keyboard[Key.S])
        {
          targetVelocity -= speed * forward;
        }

        if (w.Keyboard[Key.A])
        {
          targetVelocity -= speed * right;
        }

        if (w.Keyboard[Key.D])
        {
          targetVelocity += speed * right;
        }

        // TODO: rotate by curent orientation

        Vector3.Lerp(ref velocity, ref targetVelocity,
          smoothFactor * frameTime, out velocity);

        // Lean rotation based on r + mouse button

        Vector3 targetLean = Vector3.UnitY;

        if (w.Keyboard[Key.R] && w.Mouse[MouseButton.Left])
        {
          targetLean = -Vector3.UnitX;
        }

        if (w.Keyboard[Key.R] && w.Mouse[MouseButton.Right])
        {
          targetLean = Vector3.UnitX;
        }

        // FIXME: bad slerp function?
        // TODO: use quaternion slerp
        /*Slerp(ref lean, ref targetLean,
          smoothFactor * frameTime, out lean);*/
      }
    }

    private Camera _camera = new Camera
      {
        fov = (float)(Math.PI / 2.4), // 75 degrees
        pos = new Vector3(0f, 0f, -5f),
        forward = Vector3.UnitZ,
        lean = Vector3.UnitY,
        speed = 15f,
        smoothFactor = 10f,
        lookSpeed = 0.001f
      };

    private Entity cube1, cube2;

    private static void Slerp(ref Vector3 a, ref Vector3 b, float t, out Vector3 result)
    {
      float omega, sinOmega;
      Vector3.Dot(ref a, ref b, out omega);
      sinOmega = (float)Math.Sin(omega);

      result = (float)Math.Sin((1 - t) * omega) / sinOmega * a
        + (float)Math.Sin(t * omega) / sinOmega * b;
    }

    protected override void OnLoad(EventArgs e)
    {
      GL.ClearColor(Color.Black);
      
      _viewport = new Viewport(ClientRectangle);
      Matrix4.CreatePerspectiveFieldOfView(
        _camera.fov,
        ClientSize.Width / ClientSize.Height,
        1f,
        75f,
        out _viewport.projectionMatrix);

      _viewport.viewMatrix = Matrix4.LookAt(
        _camera.pos,
        _camera.pos + _camera.forward,
        _camera.lean);

      RenderFrame += _camera.OnRenderFrame;
      UpdateFrame += _camera.OnUpdateFrame;

      // generate scene
      VNTiData cubeData = new VNTiData();
      cubeData.Load("Cube");
      cubeData.Update();

      int textureId = LoadBitmapTexture("Data/bulldog.png");

      // our flat color shading material
      // TOOD: move this out to the material code or laod from files, when I have the time
      Material diffspec = new Material();
      Simple1MVPShaderModifier mod = new Simple1MVPShaderModifier();
      diffspec.Program = mod.DeriveProgram(ShaderProgram.Cache["VNT_DiffSpec"]);
      diffspec.Program.Compile();

      // TODO: encapsulate light parameters into a Light class
      // TODO: separate the lighting parts of the shader into shadermodifiers

      //diffspec["textureMap"] = textureId; no idea why this causes a crash
      diffspec["npInterp"] = false;
      diffspec["matCol"] = new Vector3(1f, 1f, 1f);
      diffspec["matShiny"] = 10f;
      diffspec["ambLight"] = new Vector3(0.3f, 0f, 0f);
      diffspec["pointLightPos"] = new Vector3(1f, 1f, 1f);
      diffspec["pointLightCol"] = new Vector3(0.5f, 0.9f, 0.5f);
      diffspec["pointLightRange"] = 20f;

      cube1 = new Entity();
      cube1.VertexData = cubeData;
      cube1.Material = diffspec;
      cube1.transform = new Transform(Matrix4.CreateTranslation(-1.5f, 0f, 0f));

      cube2 = new Entity();
      cube2.VertexData = cubeData;
      cube2.Material = diffspec;
      cube2.transform = new Transform(Matrix4.CreateTranslation(1.5f, 0f, 0f));
      
      // enable bits: depth test, etc.
      GL.Enable(EnableCap.DepthTest);
      GL.DepthFunc(DepthFunction.Less);

      // TODO: generate environment maps

      System.Windows.Forms.Cursor.Position = new Point(
        (Bounds.Left + Bounds.Right) / 2,
        (Bounds.Top + Bounds.Bottom) / 2);
    }

    /// <summary>
    /// Uploads the specified image file as a texture.
    /// Taken from http://www.opentk.com/doc/graphics/textures/loading
    /// </summary>
    /// <param name="filePath">Path to the image file to load.</param>
    /// <returns>The ID of the uploaded texture.</returns>
    private int LoadBitmapTexture(string filePath)
    {
      if (String.IsNullOrEmpty(filePath))
        throw new ArgumentException(filePath);

      int id = GL.GenTexture();
      GL.BindTexture(TextureTarget.Texture2D, id);

      Bitmap bmp = new Bitmap(filePath);
      BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
          OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

      bmp.UnlockBits(bmp_data);

      // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
      // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
      // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

      return id;
    }

    protected override void OnResize(EventArgs e)
    {
      base.OnResize(e);

      Matrix4.CreatePerspectiveFieldOfView(
        _camera.fov,
        ClientSize.Width / ClientSize.Height,
        1f,
        75f,
        out _viewport.projectionMatrix);
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
      base.OnKeyPress(e);
      if (e.KeyChar == 't')
      {
        cube1.VertexData.PrimitiveType =
          cube1.VertexData.PrimitiveType == BeginMode.Triangles
          ? BeginMode.LineStrip
          : BeginMode.Triangles;
      }
      else if (e.KeyChar == 'p')
      {
        cube1.Material["npInterp"] = !(bool)cube1.Material["npInterp"];
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
      float rotSpeed = 2f;

      Quaternion rot = Quaternion.FromAxisAngle(Vector3.UnitY, rotSpeed * frameTime);
      cube1.transform.Rotate(rot);

      rot *= Quaternion.FromAxisAngle(Vector3.UnitX, 1.5f * rotSpeed * frameTime);
      cube2.transform.Rotate(rot);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);

      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

      _viewport.viewMatrix = Matrix4.LookAt(
        _camera.pos,
        _camera.pos + _camera.forward,
        _camera.lean);

      _viewport.SetActive();
      
      VertexData.DrawAllVertexData();

      /*
      // for now:
      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadMatrix(ref _viewport.projectionMatrix);
      GL.MatrixMode(MatrixMode.Modelview);
      GL.LoadMatrix(ref _viewport.viewMatrix);
      Teapot.DrawSolidTeapot(5f);
      //*/

      this.SwapBuffers();
    }

    public static void Main()
    {
      using (Lab4And5 win = new Lab4And5())
      {
        win.ClientSize = new Size(1024, 768);
        win.Title = "Combined Case Studies 5.4, 8.1, 8.2";
        win.Run(30, 0);
      }
    }
  }
}

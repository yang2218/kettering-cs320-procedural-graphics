using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject
{
  public class Hello2 : GameWindow
  {
    // The handle for the bulldog texture.
    private int bulldogTextureID;

    // Transform matrix for the camera.
    Matrix4 lookMatrix;
    
    // Transform matrix for the bulldog (which is just a textured square)
    Matrix4 bulldogTransformMatrix;

    // Some extra variables for keeping track of the state of things
    Vector3 eyePos, lookTarget;
    Vector3 bulldogRotation;

    /// <summary>
    /// Invoked when window is loaded; setup OpenGL and stuff here.
    /// </summary>
    /// <param name="e">Not used.</param>
    protected override void OnLoad(EventArgs e)
    {
      GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height);

      GL.ClearColor(Color.Black);

      GL.MatrixMode(MatrixMode.Projection);
      GL.Ortho(-0.6, 0.6, -0.6, 0.6, -10, 10);

      // Initialize the camera's transform and some state variables
      eyePos = new Vector3(0f, 0f, 2f);
      lookTarget = new Vector3(0f, 0f, 0f);
      lookMatrix = Matrix4.LookAt(eyePos, lookTarget, Vector3.UnitY);

      // Initialize the bulldog's transform and some state variables
      bulldogTransformMatrix = Matrix4.Identity;
      bulldogRotation = Vector3.Zero;
      
      // It's a good idea to use an image that's square with a side length a power of 2
      // e.g. 128x128, 256x256, 1024x1024, etc.
      // This image is a PNG, which also has an alpha channel. This channel is included
      // in the texture, and can be used for various things (blending, cutouts, etc.)
      bulldogTextureID = LoadBitmapTexture("Data/bulldog.png");
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

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);

      float frameTime = (float)e.Time;

      if (Keyboard[OpenTK.Input.Key.Escape])
      {
        Exit();
      }

      //
      // Process input for moving camera
      //
      const float panSpeed = 1f;
      bool updated = false;

      if (Keyboard[OpenTK.Input.Key.Up])
      {
        lookTarget.Y += panSpeed * frameTime;
        updated = true;
      }
      if (Keyboard[OpenTK.Input.Key.Down])
      {
        lookTarget.Y -= panSpeed * frameTime;
        updated = true;
      }
      if (Keyboard[OpenTK.Input.Key.Left])
      {
        lookTarget.X -= panSpeed * frameTime;
        updated = true;
      }
      if (Keyboard[OpenTK.Input.Key.Right])
      {
        lookTarget.X += panSpeed * frameTime;
        updated = true;
      }

      if (updated)
      {
        lookMatrix = Matrix4.LookAt(eyePos, lookTarget, Vector3.UnitY);
      }

      //
      // Process input for moving the square
      //
      const float rotSpeed = 5f;
      updated = false;

      if (Keyboard[OpenTK.Input.Key.W])
      {
        bulldogRotation.X += rotSpeed * frameTime;
        updated = true;
      }
      if (Keyboard[OpenTK.Input.Key.S])
      {
        bulldogRotation.X -= rotSpeed * frameTime;
        updated = true;
      }
      if (Keyboard[OpenTK.Input.Key.A])
      {
        bulldogRotation.Y -= rotSpeed * frameTime;
        updated = true;
      }
      if (Keyboard[OpenTK.Input.Key.D])
      {
        bulldogRotation.Y += rotSpeed * frameTime;
        updated = true;
      }

      if (updated)
      {
        Matrix4 xrotMatrix, yrotMatrix;
        Matrix4.CreateRotationX(bulldogRotation.X, out xrotMatrix);
        Matrix4.CreateRotationY(bulldogRotation.Y, out yrotMatrix);

        // The multiplication operation can also be written as:
        // projectionMatrix = lookMatrix * orthoMatrix;
        // However, structs, such as Matrix4, are value types, and are normally passed by value,
        // in method parameters or in operator overloads. Using the static method Matrix4.Mult
        // with the ref and out parameters passes the parameters by reference, which is more
        // efficient.
        Matrix4.Mult(ref xrotMatrix, ref yrotMatrix, out bulldogTransformMatrix);
      }
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
      GL.Enable(EnableCap.DepthTest);

      GL.MatrixMode(MatrixMode.Modelview);

      SetupCamera();

      GL.PushMatrix();
      GL.MultMatrix(ref bulldogTransformMatrix);
      DrawBulldog();
      GL.PopMatrix();

      // This just draws a second bulldog just behind the first one.
      // If you uncomment this code, you'll see as the first bulldog rotates,
      // the parts that go behind this one clipped and removed.
      // However, if you also disable DepthTest, you'll notice the second
      // bulldog is always drawn on top.
      /*GL.PushMatrix();
      GL.Translate(0f, 0f, -0.1f);
      DrawBulldog();
      GL.PopMatrix();*/
      
      this.SwapBuffers();
    }

    /// <summary>
    /// Load's the camera's transform matrix; MatrixMode should be set to ModelView
    /// before calling this method.
    /// </summary>
    private void SetupCamera()
    {
      GL.LoadMatrix(ref lookMatrix);
    }

    /// <summary>
    /// Draws a square with the bulldog texture.
    /// </summary>
    private void DrawBulldog()
    {
      GL.Enable(EnableCap.Texture2D);
      GL.BindTexture(TextureTarget.Texture2D, bulldogTextureID);

      // Doesn't render pixels with alpha < 0.5
      // If you comment out this line, the texture's alpha channel is ignored,
      // and you'll see a white square background (and maybe something else)
      GL.Enable(EnableCap.AlphaTest);
      GL.AlphaFunc(AlphaFunction.Greater, 0.5f);

      GL.Color3(1.0, 1.0, 1.0);
      GL.Begin(BeginMode.Polygon);
        GL.TexCoord2(0.0, 1.0); GL.Vertex3(-0.5f, -0.5f, 0.0);
        GL.TexCoord2(1.0, 1.0); GL.Vertex3(0.5f, -0.5f, 0.0);
        GL.TexCoord2(1.0, 0.0); GL.Vertex3(0.5f, 0.5f, 0.0);
        GL.TexCoord2(0.0, 0.0); GL.Vertex3(-0.5f, 0.5f, 0.0);
      GL.End();
    }

    /// <summary>
    /// Main.
    /// </summary>
    [STAThread]
    public static void Main()
    {
      using (Hello2 window = new Hello2())
      {
        window.Title = "Hello Texture and Camera Control";
        window.ClientSize = new Size(250, 250);
        window.X = 500;
        window.Y = 500;
        window.Run(30.0, 30.0);
      }
    }
  }
}

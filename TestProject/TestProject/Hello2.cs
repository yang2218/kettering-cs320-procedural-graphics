using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject
{
  public class Hello2 : GameWindow
  {
    private int textureID;
    Matrix4 orthoMatrix, lookMatrix, projectionMatrix;
    Vector3 eyePos, lookTarget;
    Vector3 objRotation;

    /// <summary>
    /// Invoked when window is loaded; setup OpenGL and stuff here.
    /// </summary>
    /// <param name="e">Not used.</param>
    protected override void OnLoad(EventArgs e)
    {
      GL.Viewport(0, 0, 250, 250);

      GL.ClearColor(Color.Black);

      eyePos = new Vector3(0f, 0f, 2f);
      lookTarget = new Vector3(0f, 0f, 0f);
      Matrix4.CreateOrthographic(2f, 2f, -10f, 10f, out orthoMatrix);
      lookMatrix = Matrix4.LookAt(eyePos, lookTarget, Vector3.UnitY);
      
      Matrix4.Mult(ref lookMatrix, ref orthoMatrix, out projectionMatrix);
      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadMatrix(ref projectionMatrix);
      
      textureID = LoadBitmapTexture("Data/bulldog.png");
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

      if (Keyboard[OpenTK.Input.Key.Escape])
      {
        Exit();
      }

      float panSpeed = 1f;

      if (Keyboard[OpenTK.Input.Key.Up])
      {
        lookTarget.Y += panSpeed * (float)e.Time;
      }
      if (Keyboard[OpenTK.Input.Key.Down])
      {
        lookTarget.Y -= panSpeed * (float)e.Time;
      }
      if (Keyboard[OpenTK.Input.Key.Left])
      {
        lookTarget.X -= panSpeed * (float)e.Time;
      }
      if (Keyboard[OpenTK.Input.Key.Right])
      {
        lookTarget.X += panSpeed * (float)e.Time;
      }

      lookMatrix = Matrix4.LookAt(eyePos, lookTarget, Vector3.UnitY);

      Matrix4.Mult(ref lookMatrix, ref orthoMatrix, out projectionMatrix);
      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadMatrix(ref projectionMatrix);


      float rotSpeed = 5f;

      if (Keyboard[OpenTK.Input.Key.W])
      {
        objRotation.X += rotSpeed * (float)e.Time;
      }
      if (Keyboard[OpenTK.Input.Key.S])
      {
        objRotation.X -= rotSpeed * (float)e.Time;
      }
      if (Keyboard[OpenTK.Input.Key.A])
      {
        objRotation.Y -= rotSpeed * (float)e.Time;
      }
      if (Keyboard[OpenTK.Input.Key.D])
      {
        objRotation.Y += rotSpeed * (float)e.Time;
      }

      Matrix4 xrotMatrix, yrotMatrix, modelViewMatrix;
      Matrix4.CreateRotationX(objRotation.X, out xrotMatrix);
      Matrix4.CreateRotationY(objRotation.Y, out yrotMatrix);
      Matrix4.Mult(ref xrotMatrix, ref yrotMatrix, out modelViewMatrix);
      GL.MatrixMode(MatrixMode.Modelview);
      GL.LoadMatrix(ref modelViewMatrix);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

      GL.Enable(EnableCap.DepthTest);

      // Doesn't render pixels with alpha < 0.5
      GL.Enable(EnableCap.AlphaTest);
      GL.AlphaFunc(AlphaFunction.Greater, 0.5f);

      GL.Enable(EnableCap.Texture2D);
      GL.BindTexture(TextureTarget.Texture2D, textureID);

      GL.Color3(1.0, 1.0, 1.0);
      GL.Begin(BeginMode.Polygon);
        GL.TexCoord2(0.0, 1.0); GL.Vertex3(-0.5f, -0.5f, 0.0);
        GL.TexCoord2(1.0, 1.0); GL.Vertex3(0.5f, -0.5f, 0.0);
        GL.TexCoord2(1.0, 0.0); GL.Vertex3(0.5f, 0.5f, 0.0);
        GL.TexCoord2(0.0, 0.0); GL.Vertex3(-0.5f, 0.5f, 0.0);
      GL.End();

      // This just draws a second bulldog just behind the first one.
      // If you uncomment this code, you'll see as the first bulldog rotates,
      // the parts that go behind this one clipped and removed.
      // However, if you also disable DepthTest, you'll notice the second
      // bulldog is always drawn on top.
      /*
      GL.MatrixMode(MatrixMode.Modelview);
      GL.PushMatrix();
      GL.LoadIdentity();
      GL.Begin(BeginMode.Polygon);
        GL.TexCoord2(0.0, 1.0); GL.Vertex3(-0.5f, -0.5f, 0.1);
        GL.TexCoord2(1.0, 1.0); GL.Vertex3(0.5f, -0.5f, 0.1);
        GL.TexCoord2(1.0, 0.0); GL.Vertex3(0.5f, 0.5f, 0.1);
        GL.TexCoord2(0.0, 0.0); GL.Vertex3(-0.5f, 0.5f, 0.1);
      GL.End();
      GL.PopMatrix();
      */

      this.SwapBuffers();
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
        window.Size = new Size(250, 250);
        window.X = 500;
        window.Y = 500;
        window.Run(30.0, 0.0);
      }
    }
  }
}

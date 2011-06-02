using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using TestProject.Objects;

namespace TestProject
{
  public class TextTest : GameWindow
  {
    private Viewport viewport;
    private Entity testEnt;
    public static int texId;

    protected override void OnLoad(EventArgs e)
    {
      
      Exit();
      return;

      GL.ClearColor(Color.Black);
      //GL.Enable(EnableCap.DepthTest);

      viewport = new Viewport(this.ClientRectangle);
      Matrix4.CreateOrthographic(3f, 3f, -1f, 1f, out viewport.projectionMatrix);
      viewport.viewMatrix = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);

      VertexPosUVData.Setup();

      VertexPosUVData vertexData = new VertexPosUVData();
      vertexData.LoadTestModel();
      vertexData.Update();

      texId = LoadBitmapTexture("Data/LiberationSans-Regular.ttf_sdf.png");
      //texId = LoadBitmapTexture("Data/bulldog.png");

      Material material = Material.Cache["DefaultTextured"];
      material["color"] = Vector3.One;
      material["textureMap"] = texId;

      testEnt = new Entity();
      testEnt.VertexData = vertexData;
      testEnt.Material = material;
      testEnt.transform = new Transform();
    }

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

      ProcessInput(frameTime);
    }

    private void ProcessInput(float frameTime)
    {
      if (Keyboard[Key.Escape])
      {
        Exit();
      }
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);

      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

      viewport.SetActive(); // right now just makes the viewport active

      testEnt.Draw();

      this.SwapBuffers();
    }

    /// <summary>
    /// Main.
    /// </summary>
    [STAThread]
    public static void Main()
    {
      using (TextTest win = new TextTest())
      {
        win.ClientSize = new Size(640, 480);
        win.Title = "Text Rendering Test";
        win.Run(30);
      }
    }
  }
}

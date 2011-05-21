using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using ThereBeMonsters.Front_end;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ThereBeMonsters.Back_end.Modules
{
  public class TexturePreview : Module
  {
    public class PreviewControl : EditorControl
    {
      public override double PreferredHeight
      {
        get { return 128.0; }
      }

      public PreviewControl(ModuleNodeControl parentNode, string paramName)
        : base(parentNode, paramName)
      {
      }

      public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
      {
      }

      public override void Render(OpenTKGUI.GUIRenderContext Context)
      {
        // TODO: code for in-line preview
        base.Render(Context);
      }
    }

    public class PreviewWindow : GameWindow
    {
      private int textureId;
      private uint[,] map;

      public PreviewWindow(uint[,] map)
      {
        Title = "Texture Preview";
        ClientSize = new Size(map.GetLength(0), map.GetLength(1));
        this.map = map;
      }

      protected override void OnLoad(EventArgs e)
      {
        GL.Viewport(ClientRectangle);

        GL.ClearColor(Color.Black);

        GL.MatrixMode(MatrixMode.Projection);
        GL.Ortho(-0.5, 0.5, -0.5, 0.5, -1, 1);

        textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureId);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, map.GetLength(0),
          map.GetLength(1), 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte,
          map);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
      }

      protected override void OnUpdateFrame(FrameEventArgs e)
      {
        if (Keyboard[OpenTK.Input.Key.Escape])
        {
          Exit();
        }
      }

      protected override void OnRenderFrame(FrameEventArgs e)
      {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        GL.Enable(EnableCap.Texture2D);
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        
        GL.Color3(1.0, 1.0, 1.0);
        GL.Begin(BeginMode.Polygon);
          GL.TexCoord2(0.0, 1.0); GL.Vertex3(-0.5f, -0.5f, 0.0);
          GL.TexCoord2(1.0, 1.0); GL.Vertex3(0.5f, -0.5f, 0.0);
          GL.TexCoord2(1.0, 0.0); GL.Vertex3(0.5f, 0.5f, 0.0);
          GL.TexCoord2(0.0, 0.0); GL.Vertex3(-0.5f, 0.5f, 0.0);
        GL.End();

        this.SwapBuffers();
      }
    }

    [Parameter(Editor = typeof(PreviewControl))]
    public bool PreviewInline { private get; set; }

    [Parameter(Optional = true)]
    public byte[,] HeightMap { private get; set; }

    [Parameter(Optional = true)]
    public uint[,] ColorMap { private get; set; }

    public int TextureID { get; private set; }

    public override void Run()
    {
      uint[,] map;

      if (HeightMap != null)
      {
        map = new uint[HeightMap.GetLength(0), HeightMap.GetLength(1)];
        for (int i = 0; i < map.GetLength(0); i++)
        {
          for (int j = 0; j < map.GetLength(1); j++)
          {
            map[i, j] = (uint)0xff000000
              | (uint)(HeightMap[i, j] << 16)
              | (uint)(HeightMap[i, j] << 8)
              | (uint)(HeightMap[i, j]);
          }
        }
      }
      else if (ColorMap != null)
      {
        map = ColorMap;
      }
      else
      {
        throw new ArgumentNullException(
          "One of ColorMap or HeightMap must not be null.",
          (Exception)null);
      }

      if (PreviewInline)
      {
        // TODO
        return;
      }

      using (PreviewWindow win = new PreviewWindow(map))
      {
        win.Run();
      }
    }
  }
}

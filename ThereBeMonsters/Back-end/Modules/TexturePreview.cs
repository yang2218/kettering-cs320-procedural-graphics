using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using ThereBeMonsters.Front_end;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ThereBeMonsters.Front_end.Controls;
using OpenTKGUI;

namespace ThereBeMonsters.Back_end.Modules
{
  public class TexturePreview : Module
  {
    public class PreviewControl : EditorControl
    {
      private uint[,] _map;
      public uint[,] Map
      {
        get
        {
          return _map;
        }
        set
        {
          _map = value;

          if (_textureHandle.HasValue)
          {
            GL.DeleteTexture(_textureHandle.Value);
          }

          _textureHandle = GL.GenTexture();
          GL.BindTexture(TextureTarget.Texture2D, _textureHandle.Value);

          GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Map.GetLength(0),
            Map.GetLength(1), 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte,
            Map);

          GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
          GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }
      }

      private int? _textureHandle;

      public override double PreferredHeight
      {
        get { return 150.0; }
      }

      public PreviewControl(ModuleNodeControl parentNode, string paramName)
        : base(parentNode, paramName)
      {
        ModuleParameterValue = this;
      }

      public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
      {
      }

      public override void Update(GUIControlContext Context, double Time)
      {
        // TODO: on click, open up a modal dialog with a bigger preview
      }

      public override void Render(GUIRenderContext Context)
      {
        if (_textureHandle.HasValue == false)
        {
          return;
        }

        GL.Enable(EnableCap.Texture2D);
        GL.BindTexture(TextureTarget.Texture2D, _textureHandle.Value);

        // TODO: perserve aspect ratio of map
        GL.Begin(BeginMode.Polygon);
        GL.TexCoord2(0.0, 1.0); GL.Vertex3(0f, 0f, 0.0);
        GL.TexCoord2(1.0, 1.0); GL.Vertex3(Size.X, 0f, 0.0);
        GL.TexCoord2(1.0, 0.0); GL.Vertex3(Size.X, Size.Y, 0.0);
        GL.TexCoord2(0.0, 0.0); GL.Vertex3(0f, Size.Y, 0.0);
        GL.End();
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

        GL.ClearColor(System.Drawing.Color.Black);

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

    [Parameter(Editor = typeof(PreviewControl),
      Direction = Module.Parameter.IODirection.NOWIREUP)]
    public PreviewControl PreviewInline { private get; set; }

    [Parameter(Optional = true)]
    public byte[,] HeightMap { private get; set; }

    [Parameter(Optional = true)]
    public uint[,] ColorMap { private get; set; }

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

      if (PreviewInline != null)
      {
        PreviewInline.Map = map;
        return;
      }

      using (PreviewWindow win = new PreviewWindow(map))
      {
        win.Y = 10;
        win.Run();
      }
    }
  }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public class Texture
  {
    public int TextureID { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    // format, etc.

    private Texture() { }

    // TODO: texture cache

    // TODO: move texture uploading to a separate method
    //   support texture unloading
    public static Texture GetTexture(string filePath)
    {
      Texture tex = new Texture();
      byte[] aValues, argbValues;

      using (Bitmap bmp = new Bitmap(filePath))
      {
        tex.Width = bmp.Width;
        tex.Height = bmp.Height;

        BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        int bytes = Math.Abs(bd.Stride) * bmp.Height;
        argbValues = new byte[bytes];
        aValues = new byte[bytes / 4];

        System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, argbValues, 0, bytes);

        for (int i = 3; i < argbValues.Length; i += 4)
        {
          aValues[i >> 2] = argbValues[i];
        }

        bmp.UnlockBits(bd);
      }

      tex.TextureID = GL.GenTexture();
      GL.BindTexture(TextureTarget.Texture2D, tex.TextureID);
      //GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, PixelInternalFormat.Alpha8, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.Byte, aValues);
      GL.TexImage2D<byte>(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, tex.Width, tex.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, argbValues);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

      return tex;
    }


  }
}

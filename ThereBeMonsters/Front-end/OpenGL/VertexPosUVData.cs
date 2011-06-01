using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ThereBeMonsters.Front_end
{
  public class VertexPosUVData : VertexData
  {
    public struct VertexLayout
    {
      public Vector3 position;
      public Vector2 uv;
      public float padding; // not sure if any video cards would complain without

      public VertexLayout(Vector3 pos, Vector2 uv)
      {
        this.position = pos;
        this.uv = uv;
        this.padding = 0f;
      }
    }

    private VertexLayout[] data;
    private bool uploaded;
    private int offset, count;
    private int[] firsts, counts;
    private static int lastOffset;

    private static int vertexArrayHandle;
    private static int vertexBufferHandle;

    public static void Setup()
    {
      // Create the vertex array object (defines how to read various vertex attributes from the data buffer(s))
      GL.GenVertexArrays(1, out vertexArrayHandle);
      GL.BindVertexArray(vertexArrayHandle);
      
      // Create the vertex buffer object (holds data for all models with this layout)
      GL.GenBuffers(1, out vertexBufferHandle);
      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
      // allocate 0.25MB vertex buffer. TODO: expand when needed.
      GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(262144), IntPtr.Zero, BufferUsageHint.StaticDraw);
      
      // Associate a buffer and instructions for reading it to an attribute index
      // vertex attribute 0 = position
      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6, 0);
      GL.EnableVertexAttribArray(0);
      GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 6, 3);
      GL.EnableVertexAttribArray(1);

      // TODO: also setup an element array, but too much work for drawing a... square

      GL.BindVertexArray(0);
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void LoadTestModel()
    {
      data = new VertexLayout[] {
        new VertexLayout(new Vector3( 1f, -1f,  0.0f), new Vector2(1f, 0f)),
        new VertexLayout(new Vector3(-1f, -1f,  0.0f), new Vector2(0f, 0f)),
        new VertexLayout(new Vector3( 1f,  1f,  0.0f), new Vector2(1f, 1f)),
        new VertexLayout(new Vector3(-1f,  1f,  0.0f), new Vector2(0f, 1f))
      };

      PrimitiveType = BeginMode.TriangleStrip;
      firsts = new int[] { 0 };
      counts = new int[] { 4 };
    }

    public void Load(VertexLayout[] data, BeginMode primitiveType, int[] firsts, int[] counts)
    {
      this.data = data;
      PrimitiveType = primitiveType;
      this.firsts = firsts;
      this.counts = counts;
    }

    public override void Load(string fromFile)
    {
      throw new NotImplementedException();
    }

    public override void Update()
    {
      if (uploaded == false)
      {
        offset = lastOffset;
        count = data.Length;
        lastOffset += count;
        for (int i = 0; i < firsts.Length; i++)
        {
          firsts[i] += offset;
        }

        // TODO: memory management
        // - make sure lastOffset is not > capcity
        uploaded = true;
      }

      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
      GL.BufferSubData<VertexLayout>(BufferTarget.ArrayBuffer, SizeofMult<VertexLayout>(offset), SizeofMult<VertexLayout>(data.Length), data);
      // error checking if data got uploaded?
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public override void Unload()
    {
      throw new NotImplementedException();
    }

    public override void Draw()
    {
      GL.BindVertexArray(vertexArrayHandle);
      GL.MultiDrawArrays(PrimitiveType, firsts, counts, firsts.Length);
      GL.BindVertexArray(0);
    }
  }
}

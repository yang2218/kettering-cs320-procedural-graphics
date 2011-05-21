using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public class VertexTweenData : VertexData
  {
    public struct VertexLayout
    {
      public Vector3 position1, position2;

      public VertexLayout(Vector3 pos1, Vector3 pos2)
      {
        this.position1 = pos1;
        this.position2 = pos2;
      }

      public VertexLayout(ref Vector3 pos1, ref Vector3 pos2)
      {
        this.position1 = pos1;
        this.position2 = pos2;
      }
    }

    private VertexLayout[] data;
    private float[] tweenFactors;
    private bool uploaded;
    private int offset, count;
    private int[] firsts, counts;
    private static int lastOffset;

    private static int vertexArrayHandle;
    private static int vertexBufferHandle;
    private static int vertexBufferTweenHandle;

    public override string DefaultVertexShader
    {
      get { throw new NotImplementedException(); }
    }

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
      GL.GenBuffers(1, out vertexBufferTweenHandle);
      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferTweenHandle);
      // allocate 0.03125MB vertex buffer for the tweening factors
      GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(32768), IntPtr.Zero, BufferUsageHint.DynamicDraw);

      // Associate a buffer and instructions for reading it to an attribute index
      // vertex attribute 0 = position
      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes * 2, 0);
      GL.EnableVertexAttribArray(0);
      GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes * 2, Vector3.SizeInBytes);
      GL.EnableVertexAttribArray(1);

      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferTweenHandle);
      GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, 0, 0);
      GL.EnableVertexAttribArray(2);

      GL.BindVertexArray(0);
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public float[] TweenFactors { get { return tweenFactors; } }
    public VertexLayout[] Data { get { return data; } }

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

    public bool LoadFromLineDrawFiles(string file1, string file2)
    {
      string[] lines1, lines2;
      using (StreamReader sr1 = new StreamReader(file1), sr2 = new StreamReader(file2))
      {
        lines1 = sr1.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        lines2 = sr2.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
      }

      int numPrims = lines1.Length;
      firsts = new int[numPrims];
      counts = new int[numPrims];
      List<VertexLayout> vertices = new List<VertexLayout>();
      string[] tokens1, tokens2;
      Vector3 initPos1, curPos1;
      Vector3 initPos2, curPos2;
      int i1, i2;
      float x1 = 0f, y1 = 0f;
      float x2 = 0f, y2 = 0f;
      for (int prim = 0; prim < numPrims; prim++)
      {
        firsts[prim] = vertices.Count;
        tokens1 = lines1[prim].Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        tokens2 = lines2[prim].Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        i1 = i2 = 0;
        if (((tokens1[i1++] == "m"
          && float.TryParse(tokens1[i1++], out x1)
          && float.TryParse(tokens1[i1++], out y1)) == false)
          || ((tokens2[i2++] == "m"
          && float.TryParse(tokens2[i2++], out x2)
          && float.TryParse(tokens2[i2++], out y2)) == false))
        {
          return false;
        }

        initPos1 = curPos1 = new Vector3(x1, y1, 0f);
        initPos2 = curPos2 = new Vector3(x2, y2, 0f);
        vertices.Add(new VertexLayout(ref initPos1, ref initPos2));
        while (i1 < tokens1.Length)
        {
          if (tokens1[i1] == "z")
          {
            vertices.Add(new VertexLayout(ref initPos1, ref initPos2));
            break;
          }

          if (((float.TryParse(tokens1[i1++], out x1)
            && float.TryParse(tokens1[i1++], out y1)) == false)
            || ((float.TryParse(tokens2[i2++], out x2)
            && float.TryParse(tokens2[i2++], out y2)) == false))
          {
            return false;
          }

          curPos1.X += x1;
          curPos1.Y += y1;
          curPos2.X += x2;
          curPos2.Y += y2;
          vertices.Add(new VertexLayout(ref curPos1, ref curPos2));
        }

        counts[prim] = vertices.Count - firsts[prim];
      }

      data = vertices.ToArray();
      tweenFactors = new float[data.Length];
      PrimitiveType = BeginMode.LineStrip;

      return true;
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

        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
        GL.BufferSubData<VertexLayout>(BufferTarget.ArrayBuffer, SizeofMult<VertexLayout>(offset), SizeofMult<VertexLayout>(data.Length), data);
        // error checking if data got uploaded?

        uploaded = true;
      }

      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferTweenHandle);
      GL.BufferSubData<float>(BufferTarget.ArrayBuffer, SizeofMult<float>(offset), SizeofMult<float>(tweenFactors.Length), tweenFactors);
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public override void Unload()
    {
      throw new NotImplementedException();
    }

    public override void BindVAO()
    {
      GL.BindVertexArray(vertexArrayHandle);
    }

    public override void Draw()
    {
      GL.MultiDrawArrays(PrimitiveType, firsts, counts, firsts.Length);
    }
  }
}

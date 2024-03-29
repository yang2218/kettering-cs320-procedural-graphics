﻿using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public class VertexPosData : VertexData
  {
    public struct VertexLayout
    {
      public Vector3 position;

      public VertexLayout(Vector3 pos)
      {
        this.position = pos;
      }

      public VertexLayout(ref Vector3 pos)
      {
        this.position = pos;
      }
    }

    private VertexLayout[] data;
    private bool uploaded;
    private int offset, count;
    private int[] firsts, counts;
    private static int lastOffset;

    private static int vertexArrayHandle;
    private static int vertexBufferHandle;
    private static bool isSetup = false;

    public override string DefaultVertexShader
    {
      get { return "Pos"; }
    }

    public static void Setup()
    {
      if (isSetup)
      {
        return;
      }

      // Create the vertex array object (defines how to read various vertex attributes from the data buffer(s))
      GL.GenVertexArrays(1, out vertexArrayHandle);
      GL.BindVertexArray(vertexArrayHandle);
      
      // Create the vertex buffer object (holds data for all models with this layout)
      GL.GenBuffers(1, out vertexBufferHandle);
      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
      // allocate 0.25MB vertex buffer. TODO: expand when needed.
      GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(262144), IntPtr.Zero, BufferUsageHint.StaticDraw);
      
      // Associate a buffer and instructions for reading it to an attribute index
      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
      BindAttribute(Attribute.Position, 3, VertexAttribPointerType.Float, false, 0, 0);
      
      GL.BindVertexArray(0);
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

      isSetup = true;
    }

    public void LoadTestModel()
    {
      data = new VertexLayout[] {
        new VertexLayout(new Vector3( 1f, -1f,  0.0f)),
        new VertexLayout(new Vector3(-1f, -1f,  0.0f)),
        new VertexLayout(new Vector3( 1f,  1f,  0.0f)),
        new VertexLayout(new Vector3(-1f,  1f,  0.0f))
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

    public bool LoadFromPolylineFile(string filePath)
    {
      string[] tokens;
      using (StreamReader sr = new StreamReader(filePath))
      {
        tokens = sr.ReadToEnd().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
      }

      int i = 0;
      int numPrims;
      List<VertexLayout> vertices = new List<VertexLayout>();
      if (int.TryParse(tokens[i++], out numPrims) == false)
      {
        return false;
      }

      firsts = new int[numPrims];
      counts = new int[numPrims];
      int numVerts;
      float x, y = 0f; // C# is smart enough to recognize the short-circuit before Y gets parsed,
      // but not smart enough to realize that if the short-circuit happens Y will never be read.
      for (int prim = 0; prim < numPrims; prim++)
      {
        if (int.TryParse(tokens[i++], out numVerts) == false)
        {
          return false;
        }

        firsts[prim] = vertices.Count;
        counts[prim] = numVerts;

        for (int vert = 0; vert < numVerts; vert++)
        {
          if ((float.TryParse(tokens[i++], out x)
            && float.TryParse(tokens[i++], out y)) == false)
          {
            return false;
          }

          vertices.Add(new VertexLayout(new Vector3(x, y, 0f)));
        }
      }

      data = vertices.ToArray();
      PrimitiveType = BeginMode.LineStrip;

      return true;
    }

    public bool LoadFromLineDrawFile(string filePath)
    {
      string[] lines;
      using (StreamReader sr = new StreamReader(filePath))
      {
        lines = sr.ReadToEnd().Split(new char[]{'\n','\r'}, StringSplitOptions.RemoveEmptyEntries);
      }

      int numPrims = lines.Length;
      firsts = new int[numPrims];
      counts = new int[numPrims];
      List<VertexLayout> vertices = new List<VertexLayout>();
      string[] tokens;
      Vector3 initPos, curPos;
      int i;
      float x = 0f, y = 0f;
      for (int prim = 0; prim < numPrims; prim++)
      {
        firsts[prim] = vertices.Count;
        tokens = lines[prim].Split(new char[]{' ', ','}, StringSplitOptions.RemoveEmptyEntries);
        i = 0;
        if ((tokens[i++] == "m"
          && float.TryParse(tokens[i++], out x)
          && float.TryParse(tokens[i++], out y)) == false)
        {
          return false;
        }

        initPos = curPos = new Vector3(x, y, 0f);
        vertices.Add(new VertexLayout(ref initPos));
        while (i < tokens.Length)
        {
          if (tokens[i] == "z")
          {
            vertices.Add(new VertexLayout(ref initPos));
            break;
          }

          if ((float.TryParse(tokens[i++], out x)
            && float.TryParse(tokens[i++], out y)) == false)
          {
            return false;
          }

          curPos.X += x;
          curPos.Y += y;
          vertices.Add(new VertexLayout(ref curPos));
        }

        counts[prim] = vertices.Count - firsts[prim];
      }

      data = vertices.ToArray();
      PrimitiveType = BeginMode.LineStrip;

      return true;
    }

    public override void Update()
    {
      if (isSetup == false)
      {
        Setup();
      }

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

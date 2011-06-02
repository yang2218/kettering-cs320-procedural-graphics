using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public class VTiData : VertexData
  {
    public struct VertexLayout
    {
      public Vector3 position;
      public Vector2 texcoord;

      public VertexLayout(float px, float py, float pz, float s, float t)
      {
        this.position = new Vector3(px, py, px);
        this.texcoord = new Vector2(s, t);
      }

      public VertexLayout(Vector3 pos, Vector2 tex)
      {
        this.position = pos;
        this.texcoord = tex;
      }

      public VertexLayout(ref Vector3 pos, ref Vector2 tex)
      {
        this.position = pos;
        this.texcoord = tex;
      }
    }

    public class VertexList : List<VertexLayout>
    {
      public void Add(float px, float py, float pz, float s, float t)
      {
        base.Add(new VertexLayout(
          new Vector3(px, py, pz),
          new Vector2(s, t)));
      }
    }

    private VertexList data;
    private ushort[] elements;
    
    private bool uploaded;
    private int vboOffset, elementOffset;

    private static int lastVboOffset, lastElementOffset;
    private static uint vertexArrayHandle, vertexBufferHandle, elementBufferHandle;
    private static bool isSetup = false;

    // TODO: define a primitive restart constant

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
      // allocate 256KB vertex buffer. TODO: expand when needed.
      GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(262144), IntPtr.Zero, BufferUsageHint.StaticDraw);

      // Create the element buffer (holds offsets intot he vertex buffer)
      GL.GenBuffers(1, out elementBufferHandle);
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferHandle);
      // allocate 5KB element buffer TODO: expand when needed
      GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(5120), IntPtr.Zero, BufferUsageHint.StaticDraw);

      // Associate a buffer and instructions for reading it to an attribute index
      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
      BindAttribute<VertexLayout>(Attribute.Position, 3, VertexAttribPointerType.Float, false, 0);
      BindAttribute<VertexLayout>(Attribute.TexCoord, 2, VertexAttribPointerType.Float, false, Vector3.SizeInBytes);
      
      GL.BindVertexArray(0);
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

      isSetup = true;
    }

    public void Load(VertexList data, BeginMode primitiveType, ushort[] elements)
    {
      this.data = data;
      PrimitiveType = primitiveType;
      this.elements = elements;
    }

    public override void Load(string fromFile)
    {
      switch (fromFile)
      {
        case "Square":
          data = new VertexList {
            { 0.5f, -0.5f, 0f, 0f, 1f},
            {-0.5f, -0.5f, 0f, 1f, 1f},
            { 0.5f,  0.5f, 0f, 0f, 0f},
            {-0.5f,  0.5f, 0f, 1f, 0f}
          };

          elements = new ushort[] { 0, 1, 2, 3 };
          PrimitiveType = BeginMode.TriangleStrip;
          break;
        case "Cube":
          data = new VertexList {
            // front
            { 0.5f, -0.5f, -0.1f, 0f, 1f},
            {-0.5f, -0.5f, -0.1f, 1f, 1f},
            { 0.5f,  0.5f, -0.1f, 0f, 0f},
            {-0.5f,  0.5f, -0.1f, 1f, 0f},
            // right
            { 0.5f, -0.5f,  0.5f, 1f, 1f},
            { 0.5f, -0.5f, -0.5f, 1f, 1f},
            { 0.5f,  0.5f,  0.5f, 1f, 0f},
            { 0.5f,  0.5f, -0.5f, 1f, 0f},
            // top
            { 0.5f,  0.5f, -0.5f, 1f, 1f},
            {-0.5f,  0.5f, -0.5f, 0f, 1f},
            { 0.5f,  0.5f,  0.5f, 1f, 1f},
            {-0.5f,  0.5f,  0.5f, 0f, 1f},
            // back
            { 0.5f, -0.5f,  0.5f, 1f, 1f},
            {-0.5f, -0.5f,  0.5f, 0f, 1f},
            { 0.5f,  0.5f,  0.5f, 1f, 0f},
            {-0.5f,  0.5f,  0.5f, 0f, 0f},
            // left
            {-0.5f,  0.5f, -0.5f, 0f, 0f},
            {-0.5f, -0.5f, -0.5f, 0f, 1f},
            {-0.5f,  0.5f,  0.5f, 0f, 0f},
            {-0.5f, -0.5f,  0.5f, 0f, 1f},
            // bottom
            { 0.5f, -0.5f, -0.5f, 1f, 0f},
            {-0.5f, -0.5f, -0.5f, 0f, 0f},
            { 0.5f, -0.5f,  0.5f, 1f, 0f},
            {-0.5f, -0.5f,  0.5f, 0f, 0f}
          };

          PrimitiveType = BeginMode.Triangles;
          elements = new ushort[] {
            0,1,2,1,2,3,        // front
            4,5,6,5,6,7,        // right
            8,9,10,9,10,11,     // top
            12,13,14,13,14,15,  // back
            16,17,18,17,18,19,  // left
            20,21,22,21,22,23   // bottom
          };

          break;
        default:
          throw new NotImplementedException();
      }
    }

    public override void Update()
    {
      if (isSetup == false)
      {
        Setup();
      }

      if (uploaded == false)
      {
        vboOffset = lastVboOffset;
        lastVboOffset += data.Count;
        elementOffset = lastElementOffset;
        lastElementOffset += elements.Length;

        // TODO: memory management
        // - make sure lastOffset is not > capcity
        uploaded = true;
      }

      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
      GL.BufferSubData<VertexLayout>(BufferTarget.ArrayBuffer, SizeofMult<VertexLayout>(vboOffset), SizeofMult<VertexLayout>(data.Count), data.ToArray());
      // error checking if data got uploaded?

      GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferHandle);
      GL.BufferSubData<ushort>(BufferTarget.ElementArrayBuffer, SizeofMult<ushort>(elementOffset), SizeofMult<ushort>(elements.Length), elements);
      // error checking if data got uploaded?

      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
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
      if (uploaded == false)
      {
        throw new InvalidOperationException("Vertex data has not be uploaded yet");
      }

      GL.DrawRangeElementsBaseVertex(
        PrimitiveType,
        vboOffset,              // range in which this object's
        vboOffset + data.Count, // data resides in the vbo TODO: subtract 1?
        elements.Length, // how many offsets will we read from the element buffer
        DrawElementsType.UnsignedShort, // format of our elements buffer
        new IntPtr(elementOffset), // where to start reading from the element buffer
        vboOffset); // where this object's data starts in the vbo starts
    }
  }
}

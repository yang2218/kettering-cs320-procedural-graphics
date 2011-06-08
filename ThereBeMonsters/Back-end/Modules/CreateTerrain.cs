using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThereBeMonsters.Front_end;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace ThereBeMonsters.Back_end.Modules
{
  public class CreateTerrain : Module
  {
    private static int _vertexArrayHandle;
    private static int _vertexBufferHandle;
    private static int _elementBufferHandle;

    public byte[,] HeightMap { private get; set; }
    public uint[,] ColorMap { private get; set; }

    [Parameter("Length of the sides.", Default = 10f)]
    public float BaseScale { private get; set; }
    [Parameter("Maximum height.", Default = 5f)]
    public float HeightScale { private get; set; }

    public struct VertexData
    {
      public Vector3 position;
      public Vector3 normal;
      public Vector3 color;

      public VertexData(Vector3 position, Vector3 normal, Vector3 color)
      {
        this.position = position;
        this.normal = normal;
        this.color = color;
      }

      public VertexData(Vector3 position, Vector3 color)
      {
        this.position = position;
        this.normal = Vector3.Zero;
        this.color = color;
      }
    }

    static CreateTerrain()
    {
      int ver;
      GL.GetInteger(GetPName.MajorVersion, out ver);
      if (ver >= 3)  // turns out Vertex Arrays are opengl 3.0,
      { // and it's not difficult to keep backwards-compatible with 2.1
        GL.GenVertexArrays(1, out _vertexArrayHandle);
        GL.BindVertexArray(_vertexArrayHandle);
      }

      GL.GenBuffers(1, out _vertexBufferHandle);
      GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferHandle);

      GL.GenBuffers(1, out _elementBufferHandle);
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferHandle);

      GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes * 3, 0);
      GL.EnableClientState(ArrayCap.VertexArray);
      GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes * 3, Vector3.SizeInBytes * 1);
      GL.EnableClientState(ArrayCap.NormalArray);
      GL.ColorPointer(3, ColorPointerType.Float, Vector3.SizeInBytes * 3, Vector3.SizeInBytes * 2);

      if (ver >= 3)
      {
        GL.BindVertexArray(0);
      }
    }

    public override void Run()
    {
      //width and height of heightmap
      int mapLength = HeightMap.GetLength(0);
      int mapHeight = HeightMap.GetLength(1);

      //values used in calculating position
      float x, y, z;

      float xScale = BaseScale / mapLength;
      float yScale = BaseScale / mapHeight;
      float zScale = HeightScale / (float)byte.MaxValue;
      float xOffSet = -BaseScale / 2f;
      float yOffSet = -BaseScale / 2f;

      VertexData[] vertices = new VertexData[mapLength * mapHeight];

      //position and color computation and storing
      for (int i = 0; i < mapLength; i++)
      {
        for (int j = 0; j < mapHeight; j++)
        {
          //calculate x, y, z values of each position
          x = i * xScale + xOffSet;
          z = j * yScale + yOffSet;

          // holes on the side don't look good and we're not sitching together multiple terrains
          if (i == 0 || i == mapLength - 1
            || j == 0 || j == mapHeight - 1)
          {
            y = 0;
          }
          else
          {
            y = this.HeightMap[i, j] * zScale;
          }

          //calculate color of each position
          float red = (ColorMap[i, j] & 0xFF) / 255f;
          float green = ((ColorMap[i, j] >> 8) & 0xFF) / 255f;
          float blue = ((ColorMap[i, j] >> 16) & 0xFF) / 255f;
          Vector3 color = new Vector3(red, green, blue);

          //add position and color
          vertices[i * mapLength + j] = new VertexData(new Vector3(x, y, z), color);
        }
      }

      #region normal calculation
      Vector3 normal;
      //normal computation and storing
      for (int i = 0; i < mapLength; i++)
      {
        for (int j = 0; j < mapHeight; j++)
        {
          //calculate the normals
          //first sum all adjacent cross products

           normal = Vector3.Zero;

          //topleft cross topcenter
          if (i != 0 && j != 0)
            normal += Vector3.Cross(vertices[(i - 1) * mapLength + (j - 1)].position, vertices[(i - 1) * mapLength + j].position);

          //topcenter cross topright
          if (i != 0 && j != mapLength - 1)
            normal += Vector3.Cross(vertices[(i - 1) * mapLength + j].position, vertices[(i - 1) * mapLength + (j + 1)].position);

          //topright cross midright
          if (i != 0 && j != mapLength - 1)
            normal += Vector3.Cross(vertices[(i - 1) * mapLength + (j + 1)].position, vertices[i * mapLength + (j + 1)].position);

          //midright cross bottomright
          if (i != mapHeight - 1 && j != mapLength - 1)
            normal += Vector3.Cross(vertices[i * mapLength + (j + 1)].position, vertices[(i + 1) * mapLength + (j + 1)].position);

          //bottomright cross bottomcenter
          if (i != mapHeight - 1 && j != mapLength - 1)
            normal += Vector3.Cross(vertices[(i + 1) * mapLength + (j + 1)].position, vertices[(i + 1) * mapLength + j].position);

          //bottomcenter cross bottomleft
          if (i != mapHeight - 1 && j != 0)
            normal += Vector3.Cross(vertices[(i + 1) * mapLength + j].position, vertices[(i + 1) * mapLength + (j - 1)].position);

          //bottomleft cross midleft
          if (i != mapHeight - 1 && j != 0)
            normal += Vector3.Cross(vertices[(i + 1) * mapLength + (j - 1)].position, vertices[i * mapLength + (j - 1)].position);

          //midleft cross topleft
          if (i != 0 && j != 0)
            normal += Vector3.Cross(vertices[i * mapLength + (j - 1)].position, vertices[(i - 1) * mapLength + (j - 1)].position);
         
          //normalize, and store the vector
          vertices[i * mapLength + j].normal = Vector3.Normalize(normal);
        }
      }
      #endregion

      //array for triangles created from verticies
      int[] triangles = new int[(2 * mapLength) * (mapHeight - 1)];
      int[] counts = new int[mapHeight - 1];
      int[] offsets = new int[mapHeight - 1];

      int store = 0;

      //fill the triangle array
      for (int j = 0; j < mapHeight - 1; j++)
      {
        offsets[j] = store * 4; //offset in bytes
        counts[j] = mapLength * 2;

        for (int i = 0; i < mapLength; i++)
        {
          triangles[store++] = j * mapLength + i;
          triangles[store++] = (j + 1) * mapLength + i;
        }
      }
      
      int ver;
      GL.GetInteger(GetPName.MajorVersion, out ver);
      if (ver >= 3)
      {
        GL.BindVertexArray(CreateTerrain._vertexArrayHandle);
      }

      GL.BindBuffer(BufferTarget.ArrayBuffer, CreateTerrain._vertexBufferHandle);
      GL.BufferData(
        BufferTarget.ArrayBuffer,
        new IntPtr(Vector3.SizeInBytes * vertices.Length * 3),
        vertices,
        BufferUsageHint.StaticDraw);

      GL.BindBuffer(BufferTarget.ElementArrayBuffer, CreateTerrain._elementBufferHandle);
      GL.BufferData(
        BufferTarget.ElementArrayBuffer,
        new IntPtr(4 * triangles.Length),
        triangles,
        BufferUsageHint.StaticDraw);

      GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes * 3, 0);
      GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes * 3, Vector3.SizeInBytes * 1);
      GL.ColorPointer(3, ColorPointerType.Float, Vector3.SizeInBytes * 3, Vector3.SizeInBytes * 2);
      
      if (ver >= 3)
      {
        GL.BindVertexArray(0);
      }

      MainWindow.Active.Viewport.OnRender += (sender, e) =>
      {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);

        if (ver >= 3)
        {
          GL.BindVertexArray(CreateTerrain._vertexArrayHandle);
        }
        else
        {
          GL.BindBuffer(BufferTarget.ArrayBuffer, CreateTerrain._vertexBufferHandle);
          GL.BindBuffer(BufferTarget.ElementArrayBuffer, CreateTerrain._elementBufferHandle);
        }

        GL.EnableClientState(ArrayCap.VertexArray);
        GL.EnableClientState(ArrayCap.NormalArray);
        GL.EnableClientState(ArrayCap.ColorArray);

        GL.MultiDrawElements(
          BeginMode.TriangleStrip,
          counts,
          DrawElementsType.UnsignedInt,
          offsets,
          counts.Length);

        GL.BindVertexArray(0);
        GL.Disable(EnableCap.DepthTest);
      };
    }
  }
}

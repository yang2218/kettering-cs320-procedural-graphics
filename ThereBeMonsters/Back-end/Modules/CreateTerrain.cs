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
    public byte[,] HeightMap { private get; set; }
    public uint[,] ColorMap { private get; set; }

    public struct terrainVal
    {
      public Vector3 position;
      public Vector3 normal;
      public Vector3 color;

      public terrainVal(Vector3 position, Vector3 normal, Vector3 color)
      {
        this.position = position;
        this.normal = normal;
        this.color = color;
      }

      public terrainVal(Vector3 position, Vector3 color)
      {
        this.position = position;
        this.normal = Vector3.Zero;
        this.color = color;
      }
    }



    public override void Run()
    {
      int vertexArrayHandle;
      int vertexBufferHandle;
      int elementBufferHandle;
      //width and height of heightmap
      int mapLength = HeightMap.GetLength(0);
      int mapHeight = HeightMap.GetLength(1);

      

      //values used in calculating position
      float x, y, z;

      float xScale = 10f / mapLength;
      float yScale = 10f / mapHeight;
      float zScale = 5f / byte.MaxValue;
      float xOffSet = -5f;
      float yOffSet = -5f;

      terrainVal[] vertices = new terrainVal[mapLength * mapHeight];


      //position and color computation and storing
      for (int i = 0; i < mapLength; i++)
      {
        for (int j = 0; j < mapHeight; j++)
        {
          //calculate x, y, z values of each position
          x = i * xScale + xOffSet;
          z = j * yScale + yOffSet;
          y = this.HeightMap[i, j] * zScale;

          //calculate color of each position
          float red = (ColorMap[i, j] & 0xFF) / 255f;
          float green = ((ColorMap[i, j] >> 8) & 0xFF) / 255f;
          float blue = ((ColorMap[i, j] >> 16) & 0xFF) / 255f;
          Vector3 color = new Vector3(red, green, blue);

          //add position and color
          vertices[i * mapLength + j] = new terrainVal(new Vector3(x, y, z), color);
        }
      }
      #region normal calculation
      Vector3 normal = Vector3.Zero;
      //normal computation and storing
      for (int i = 0; i < mapLength; i++)
      {
        for (int j = 0; j < mapHeight; j++)
        {
          //calculate the normals
          //first sum all adjacent cross products
          
          //cross poducts \/
          //topleft cross topcenter
          if(i != 0 && j != 0)
            normal = normal + Vector3.Cross(vertices[(i - 1) * mapLength + (j - 1)].position, vertices[(i - 1) * mapLength + j].position);
          
          //topcenter cross topright
          if(i != 0 && j != mapLength - 1)
            normal = normal + Vector3.Cross(vertices[(i - 1) * mapLength + j].position, vertices[(i - 1) * mapLength + (j + 1)].position);
         
          //topright cross midright
          if(i != 0 && j != mapLength -1)
            normal = normal + Vector3.Cross(vertices[(i - 1) * mapLength + (j + 1)].position, vertices[i * mapLength + (j + 1)].position);
          
          //midright cross bottomright
          if (i != mapHeight - 1 && j != mapLength - 1)
            normal = normal + Vector3.Cross(vertices[i * mapLength + (j + 1)].position, vertices[(i + 1) * mapLength + (j + 1)].position);
          
          //bottomright cross bottomcenter
          if (i != mapHeight - 1 && j != mapLength - 1)
            normal = normal + Vector3.Cross(vertices[(i + 1) * mapLength + (j + 1)].position, vertices[(i + 1) * mapLength + j].position);
          
          //bottomcenter cross bottomleft
          if (i != mapHeight - 1 && j != 0)
            normal = normal + Vector3.Cross(vertices[(i + 1) * mapLength + j].position, vertices[(i + 1) * mapLength + (j - 1)].position);
         
          //bottomleft cross midleft
          if (i != mapHeight - 1 && j != 0)
            normal = normal + Vector3.Cross(vertices[(i + 1) * mapLength + (j - 1)].position, vertices[i * mapLength + (j - 1)].position);
          
          //midleft cross topleft
          if (i != 0 && j != 0)
            normal = normal + Vector3.Cross(vertices[i * mapLength + (j - 1)].position, vertices[(i - 1) * mapLength + (j - 1)].position);
          //cross products /\


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
        for (int i = 0; i < mapLength; i++)
        {
          triangles[store++] = j * mapLength + i;
          triangles[store++] = (j + 1) * mapLength + i;
        }
        offsets[j] = j * mapLength * 8; // offset in bytes
        counts[j] = mapLength * 2;
      }




      GL.GenVertexArrays(1, out vertexArrayHandle);
      GL.BindVertexArray(vertexArrayHandle);

      GL.GenBuffers(1, out elementBufferHandle);
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferHandle);
      
      GL.GenBuffers(1, out vertexBufferHandle);
      GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);

      GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(Vector3.SizeInBytes * vertices.Length * 3), vertices, BufferUsageHint.StaticDraw);
      GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(4 * triangles.Length), triangles, BufferUsageHint.StaticDraw);
     
      GL.EnableClientState(ArrayCap.VertexArray);
      GL.EnableClientState(ArrayCap.NormalArray);
      GL.EnableClientState(ArrayCap.ColorArray);
      
      

      GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes * 3, 0);
      GL.ColorPointer(3, ColorPointerType.Float, Vector3.SizeInBytes * 3, Vector3.SizeInBytes * 2);
      GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes * 3, Vector3.SizeInBytes * 1);

      GL.BindVertexArray(0);

      MainWindow.Active.Viewport.Render += (object sender, FrameEventArgs e) =>
      {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.BindVertexArray(vertexArrayHandle);
        GL.MultiDrawElements(BeginMode.TriangleStrip, counts, DrawElementsType.UnsignedInt, offsets, counts.Length);
        GL.BindVertexArray(0);
        GL.Disable(EnableCap.DepthTest);
      };
    }
  }
}

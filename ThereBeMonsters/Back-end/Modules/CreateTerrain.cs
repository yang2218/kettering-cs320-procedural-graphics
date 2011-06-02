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

      Vector3[] positions = new Vector3[mapLength * mapHeight];


      //calculate x, y, z values of each position
      for (int i = 0; i < mapLength; i++)
      {
        for (int j = 0; j < mapHeight; j++)
        {
          x = i * xScale + xOffSet;
          y = j * yScale + yOffSet;
          z = this.HeightMap[i, j] * zScale;

          //store each vector into the array
          positions[i * mapLength + j] = new Vector3(x, y, z);
        }
      }

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

      GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(Vector3.SizeInBytes * positions.Length), positions, BufferUsageHint.StaticDraw);
      GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(4 * triangles.Length), triangles, BufferUsageHint.StaticDraw);

      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
      GL.EnableVertexAttribArray(0);

      GL.BindVertexArray(0);

      MainWindow.hack.Viewport.Render += (object sender, FrameEventArgs e) =>
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

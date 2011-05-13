using System;
using OpenTK.Graphics.OpenGL;

namespace ThereBeMonsters.Front_end
{
  public class TestEntity : Entity
  {
    public override void Draw()
    {
      GL.Begin(BeginMode.TriangleStrip);
      GL.Color3(1f, 1f, 1f);
      GL.Vertex3( 1f, -1f,  0.0f);
      GL.Vertex3(-1f, -1f,  0.0f);
      GL.Vertex3( 1f,  1f,  0.0f);
      GL.Vertex3(-1f,  1f,  0.0f);
      GL.End();
    }
  }
}

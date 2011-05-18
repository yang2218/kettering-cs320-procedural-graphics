using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public class Entity
  {
    public VertexData VertexData { get; set; }
    public Material Material { get; set; }

    // public fields can be passd as ref parameters
    public Transform transform;

    public object this[string uniform]
    {
      get
      {
        return Material[uniform];
      }
      set
      {
        // TODO: clone the material if it's a global material
        Material[uniform] = value;
      }
    }

    public void Draw()
    {
      Material.Use(ref transform.matrix);
      VertexData.Draw();
      GL.UseProgram(0);
    }
  }
}

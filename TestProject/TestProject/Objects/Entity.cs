using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public class Entity
  {
    // TODO:
    // add heiarchy mechanism
    // design for inheritance?
    // use event model for... stuff

    #region Properties and Fields

    // public fields can be passd as ref parameters
    public Transform transform;

    protected VertexData _vertexData;
    public VertexData VertexData
    {
      get
      {
        return _vertexData;
      }
      set
      {
        if (_vertexData != null)
        {
          _vertexData.RemoveEntity(this);
        }

        if (value != null)
        {
          value.AddEntity(this);
        }

        _vertexData = value;
      }
    }

    public Material Material { get; set; }

    public bool Visible { get; set; }

    public uint Layers { get; set; }

    public object this[string uniform]
    {
      get
      {
        return Material[uniform];
      }
      set
      {
        if (Material.IsShared)
        {
          Material = Material.CloneInstance();
        }

        Material[uniform] = value;
      }
    }

    #endregion

    public Entity()
    {
      Visible = true;
      Layers = 1; // default layer
    }

    public void Draw()
    {
      Material.Use(ref transform.matrix);
      VertexData.Draw();
      GL.UseProgram(0);
    }
  }
}

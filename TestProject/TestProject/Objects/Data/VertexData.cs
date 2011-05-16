using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public abstract class VertexData
  {
    // TODO: metadata, per-vertex or per-patch, for such things as rendering different parts with different materials
    // TODO: create attributes on the struct defintion and enumerate through those to set attributes
    // TODO: buffer management functions
    //  - creation, loading, unloading, compacting?, expansion

    public BeginMode PrimitiveType { get; set; }
    public abstract string DefaultVertexShader { get; }
    
    public abstract void Load(string fromFile);
    public abstract void Update();
    public abstract void Unload();
    public abstract void BindVAO();
    public abstract void Draw();

    protected LinkedList<Entity> EntityList { get; set; }

    public virtual void AddEntity(Entity entity)
    {
      EntityList.AddLast(entity);
    }

    public virtual void RemoveEntity(Entity entity)
    {
      EntityList.Remove(entity);
    }

    public virtual void DrawEntities()
    {
      Dictionary<Material, LinkedList<Entity>> drawList = new Dictionary<Material, LinkedList<Entity>>();
      uint activeLayers = Viewport.Active.ActiveLayers;
      LinkedList<Entity> list;
      bool checkVAO = false;
      foreach (Entity e in EntityList)
      {
        if (e.Visible == false
          || (e.Layers & activeLayers) == 0)
        {
          continue;
        }

        if (drawList.TryGetValue(e.Material, out list) == false)
        {
          drawList[e.Material] = list = new LinkedList<Entity>();
        }

        list.AddLast(e);
        checkVAO = true;
      }

      if (checkVAO)
      {
        BindVAO();
      }

      foreach (KeyValuePair<Material, LinkedList<Entity>> entry in drawList)
      {
        // material.DrawEntities(entityList)
        entry.Key.DrawEntities(entry.Value);
      }
    }

    public VertexData()
    {
      EntityList = new LinkedList<Entity>();

      Type type = GetType();
      List<VertexData> list;
      if (_vertexDataTypeInstances.TryGetValue(type, out list) == false)
      {
        _vertexDataTypeInstances[type] = list = new List<VertexData>();
      }

      list.Add(this);
    }

    /// <summary>
    /// Defines explicit attribute index numbering for better VAO/shader interop.
    /// </summary>
    public enum Attribute
    {
      Position,
      Normal,
      TexCoord,
      TexCoord2,
      Color,
      Position2,
      Tangent,
      
      // always keep these two at the bottom
      NumAttributes,
      None = -1
    }

    protected static uint ActiveVAO { get; set; }

    private static Dictionary<Type, List<VertexData>> _vertexDataTypeInstances
      = new Dictionary<Type, List<VertexData>>();

    public static void DrawAllVertexData()
    {
      foreach (List<VertexData> list in _vertexDataTypeInstances.Values)
      {
        if (list.Count == 0)
        {
          continue;
        }

        foreach (VertexData dataObj in list)
        {
          dataObj.DrawEntities();
        }
      }
    }

    protected static void BindAttribute(Attribute attrib, int size,
      VertexAttribPointerType type, bool normalized, int stride, int offset)
    {
      GL.VertexAttribPointer((int)attrib, size, type, normalized, stride, offset);
      GL.EnableVertexAttribArray((int)attrib);
    }

    public static IntPtr SizeofMult<T>(int count) where T : struct
    {
      return new IntPtr(Marshal.SizeOf(typeof(T)) * count);
    }
  }
}

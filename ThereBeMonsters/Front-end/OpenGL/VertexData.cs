using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ThereBeMonsters.Front_end
{
  public abstract class VertexData
  {
    // represents one or more vertex buffers (maybe perhaps a data sharing scheme somewhere?),
    // as well as the vertex data layout. For now, buffer data is static, but the design could be changed to allow updates to data

    // it could be adventagous to use a standardized vertex format, so models in the same vbo with the same material and uniforms
    // can be drawn in batch. Yeah, right, like I'm gonna work that much to figure out how to batch entities like that.

    // Shader objects need to be able to figure out how to link attributes to their inputs; provide that interface
    // maybe an enum of common data: position, normal, color, etc. Or maybe these are hardcoded properties, and subclasses
    // override to provide the correct attribute info?
    // - vertex attribute info: buffer handle, type (stored in buffer), size, stride, offset... ?: normalized, integral, enabled
    // renderer also needs to know how many vertices to draw (in case the buffer has multiple models)

    // Load/constructor: loads to system memory
    // Update: uploads
    // Unload: unloads buffers from gpu memeory

    // TODO (although probably overkill until the need comes up): specify ranges which can be drawn with separate materials

    public BeginMode PrimitiveType { get; set; }

    public abstract void Load(string fromFile);
    public abstract void Update();
    public abstract void Unload();
    public abstract void Draw();

    public IntPtr SizeofMult<T>(int count) where T : struct
    {
      return new IntPtr(Marshal.SizeOf(typeof(T)) * count);
    }
  }
}

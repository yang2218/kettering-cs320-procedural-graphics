using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public class ShaderProgram
  {
    public sealed class ProgramCache
    {
      private Dictionary<string, ShaderProgram> programCache = new Dictionary<string, ShaderProgram>();

      public ShaderProgram this[string name]
      {
        get
        {
          ShaderProgram prog;
          if (programCache.TryGetValue(name, out prog))
          {
            return prog;
          }

          return LoadShaderProgram(name);
        }
        set
        {
          programCache[name] = value;
        }
      }

      public bool TryGet(string name, out ShaderProgram sp)
      {
        return programCache.TryGetValue(name, out sp);
      }
    }

    private static ProgramCache _cache = new ProgramCache();
    public static ProgramCache Cache { get { return _cache; } }

    private static ShaderProgram LoadShaderProgram(string name)
    {
      // TODO: load from disk
      throw new NotImplementedException();
      //Cache[name] = prog;
    }

    public string Name { get; protected set; }
    public Shader VertexShader { get; protected set; }
    public Shader GeometryShader { get; protected set; }
    public Shader FragmentShader { get; protected set; }
    public bool IsCompiled { get { return this._handle.HasValue; } }
    public int Handle
    {
      get
      {
        return _handle.Value;
      }
      protected set
      {
        this._handle = value;
      }
    }

    private int? _handle;

    public ShaderProgram(string name, Shader vertex, Shader fragment)
      : this(name, vertex, null, fragment)
    {
    }

    public ShaderProgram(string name, Shader vertex, Shader geometry, Shader fragment)
    {
      this.Name = name;
      this.VertexShader = vertex;
      this.GeometryShader = geometry;
      this.FragmentShader = fragment;
    }

    public ShaderProgram(string name, string vertexShaderName, string fragmentShaderName)
      : this(name, Shader.Cache[vertexShaderName], null, Shader.Cache[fragmentShaderName])
    {
    }

    public ShaderProgram(string name, string vertexShaderName, string geometryShaderName,
      string fragmentShaderName)
      : this(name, Shader.Cache[vertexShaderName], Shader.Cache[geometryShaderName],
      Shader.Cache[fragmentShaderName])
    {
    }

    public void Compile()
    {
      if (this.IsCompiled)
      {
        return;
      }

      VertexShader.Compile();
      FragmentShader.Compile();
      if (GeometryShader != null)
      {
        GeometryShader.Compile();
      }

      Handle = GL.CreateProgram();
      GL.AttachShader(Handle, this.VertexShader.Handle);
      GL.AttachShader(Handle, this.FragmentShader.Handle);
      if (this.GeometryShader != null)
      {
        GL.AttachShader(Handle, this.GeometryShader.Handle);
      }

      BindAttributes();

      GL.LinkProgram(Handle);

      // Validate
      int status;
      GL.GetProgram(Handle, ProgramParameter.LinkStatus, out status);
      if (status == 0)
      {
        string message = GL.GetShaderInfoLog(Handle);
        throw new ApplicationException(string.Format(
          "Error linking shader program \"{0}\":\n{1}",
          this.Name,
          message));
      }
    }

    protected virtual void BindAttributes()
    {
      // we need to make sure the indices map to the same number across all VertexData subclasses
      // for more exotic data, I need to add in custom vertex array support
      GL.BindAttribLocation(Handle, 0, "position");
      GL.BindAttribLocation(Handle, 1, "uv");
    }

    public void Use()
    {
      GL.UseProgram(Handle);
    }

    public void Delete()
    {
      if (this.IsCompiled == false)
      {
        return;
      }

      GL.UseProgram(0);
      GL.DetachShader(this.Handle, VertexShader.Handle);
      GL.DetachShader(this.Handle, FragmentShader.Handle);
      if (GeometryShader != null)
      {
        GL.DetachShader(this.Handle, GeometryShader.Handle);
      }

      GL.DeleteProgram(this.Handle);

      this._handle = null;
    }
  }
}

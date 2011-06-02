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
      // for now, use this switch
      ShaderProgram sp;
      switch (name)
      {
        case "Pos_FlatCol":
          sp = new ShaderProgram(name, "Pos", "FlatCol");
          break;
        case "VNT_DiffSpec":
          sp = new ShaderProgram(name, "VNT", "DiffSpec");
          break;
        case "VT_Tex":
          sp = new ShaderProgram(name, "VT", "Tex");
          break;
        default:
          throw new NotImplementedException();
      }

      Cache[name] = sp;
      return sp;
    }

    public string Name { get; protected set; }
    public Shader VertexShader { get; protected set; }
    public Shader GeometryShader { get; protected set; }
    public Shader FragmentShader { get; protected set; }
    public bool IsCompiled { get { return this._handle.HasValue; } }

    public Dictionary<string, Shader.Parameter> Uniforms { get; private set; }

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

    #region Constructors

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

      Uniforms = new Dictionary<string, Shader.Parameter>();
      GatherUniforms();
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

    #endregion

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

      foreach (Shader.Parameter p in VertexShader.InputAttributes)
      {
        GL.BindAttribLocation(Handle, (int)p.attribute, p.name);
      }

      // TODO: bind output attributes

      GL.LinkProgram(Handle);

      // Validate
      int status;
      GL.GetProgram(Handle, ProgramParameter.LinkStatus, out status);
      if (status == 0)
      {
        string message = GL.GetShaderInfoLog(Handle);
        throw new ApplicationException(string.Format(
          "Error linking shader program '{0}':\n{1}",
          this.Name,
          message));
      }
    }

    private void GatherUniforms()
    {
      Uniforms.Clear();
      Shader[] shaders = (GeometryShader == null
        ? new Shader[] { VertexShader, FragmentShader }
        : new Shader[] { VertexShader, GeometryShader, FragmentShader });
      foreach (Shader shader in shaders)
      {
        foreach (Shader.Parameter uniform in shader.Uniforms)
        {
          Uniforms[uniform.name] = uniform;
        }
      }
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

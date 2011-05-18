using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public class Shader
  {
    public sealed class ShaderCache
    {
      private Dictionary<string, Shader> shaderCache = new Dictionary<string, Shader>();
      
      public Shader this[string name]
      {
        get
        {
          Shader shader;
          if (shaderCache.TryGetValue(name, out shader))
          {
            return shader;
          }

          return LoadShader(name);
        }
        set
        {
          shaderCache[name] = value;
        }
      }

      public bool TryGet(string name, out Shader s)
      {
        return shaderCache.TryGetValue(name, out s);
      }

      public void Remove(string name)
      {
        shaderCache.Remove(name);
      }
    }

    private static ShaderCache _cache = new ShaderCache();
    public static ShaderCache Cache { get { return _cache; } }

    public struct Parameter
    {
      public enum InterpolationQualifier
      {
        None,
        Smooth,
        Flat,
        NoPerspective
      }

      public enum StorageQualifier
      {
        None,
        Const,
        In,
        CentroidIn,
        SampleIn,
        Out,
        CentroidOut,
        SampleOut,
        Uniform,
        PatchIn,
        PatchOut
      }

      public enum GLSLType
      {
        Void,
        Bool,
        Int, Uint,
        Float,
        Double,
        Vec2, Dvec2, Bvec2, Ivec2, Uvec2,
        Vec3, Dvec3, Bvec3, Ivec3, Uvec3,
        Vec4, Dvec4, Bvec4, Ivec4, Uvec4,
        Mat2, Mat3, Mat4,
        Mat2x2, Mat3x2, Mat4x2,
        Mat2x3, Mat3x3, Mat4x3,
        Mat2x4, Mat3x4, Mat4x4,
        Dmat2, Dmat3, Dmat4,
        Dmat2x2, Dmat3x2, Dmat4x2,
        Dmat2x3, Dmat3x3, Dmat4x3,
        Dmat2x4, Dmat3x4, Dmat4x4,
        Sampler1D, Sampler2D, Sampler3D,
        SamplerCube,
        Sampler2DRect,
        Sampler1DShadow, Sampler2DShadow,
        Sampler2DRectShadow,
        Sampler1DArray, Sampler2DArray,
        SamplerBuffer,
        Sampler2DMS, Sampler2DMSArray,
        SamplerCubeArray, SamplerCubeArrayShadow,
        Isampler1D, Isampler2D, Isampler3D,
        IsamplerCube,
        Isampler2DRect,
        Isampler1DArray, Isampler2DArray,
        IsamplerBuffer,
        Isampler2DMS, Isampler2DMSArray,
        IsamplerCubeArray,
        Usampler1D, Usampler2D, Usampler3D,
        UsamplerCube,
        Usampler2DRect,
        Usampler1DArray, Usampler2DArray,
        UsamplerBuffer,
        Usampler2DMS, Usampler2DMSArray,
        UsamplerCubeArray
      }

      public InterpolationQualifier interpolation;
      public StorageQualifier storage;
      public GLSLType type;
      public string name;

      public Parameter(
        InterpolationQualifier interp,
        StorageQualifier stor,
        GLSLType type,
        string name)
      {
        this.interpolation = interp;
        this.storage = stor;
        this.type = type;
        this.name = name;
      }

      public string TypeToString()
      {
        string type = this.type.ToString();
        return string.Format("{0}{1}", Char.ToLower(type[0]), type.Substring(1));
      }

      public override string ToString()
      {
        StringBuilder sb = new StringBuilder();
        if (interpolation != InterpolationQualifier.None)
        {
          sb.Append(interpolation.ToString().ToLower());
          sb.Append(" ");
        }

        if (storage != StorageQualifier.None)
        {
          sb.Append(Regex.Replace(storage.ToString(), @"(.)([A-Z])", "$1 $2").ToLower());
          sb.Append(" ");
        }

        string type = TypeToString();
        sb.Append(" ");
        sb.Append(name);

        return sb.ToString();
      }
    }

    public struct Function
    {
      public Parameter returnType;
      public string name, body;
      public Parameter[] parameters;

      public Function(Parameter returnType, string name, Parameter[] parameters, string body)
      {
        this.returnType = returnType;
        this.name = name;
        this.parameters = parameters;
        this.body = body;
      }

      public string ToString()
      {
        return ToString(false);
      }

      public string ToString(bool asMain)
      {
        StringBuilder sb = new StringBuilder();
        sb.Append(returnType.TypeToString());
        sb.Append(" ");
        sb.Append(asMain ? "main" : name);
        sb.Append("(");
        if (parameters.Length == 0)
        {
          sb.Append("void");
        }
        else
        {
          for (int i = 0; i < parameters.Length; i++)
          {
            sb.Append(parameters[i].ToString());
            if (i != parameters.Length - 1)
            {
              sb.Append(", ");
            }
          }
        }

        sb.Append(")\n{\n");
        sb.Append(body);
        sb.Append("\n}");
        return sb.ToString();
      }
    }

    private static Shader LoadShader(string name)
    {
      // TODO: load shader from disk
      throw new ApplicationException(string.Format("Could not load shader {0}", name));
      //Cache[name] = shader;
    }

    // TODO: setup for xml deserialization
    // TDOO: better way to list/store uniforms, in/outs?
    // TODO: better variation system, maybe vertex/fragment should be subclassed?
    //  - might make the parameter stuff simplier
    
    public string Name { get; protected set; }
    public ShaderType Type { get; protected set; }
    // TODO: directives?
    public Dictionary<string, Parameter> Parameters { get; protected set; }
    public Dictionary<string, Function> Functions { get; protected set; }
    public string EntryFunction { get; set; }

    public bool IsCompiled { get { return _handle.HasValue; } }
    public int Handle
    {
      get
      {
        return _handle.Value;
      }
      protected set
      {
        _handle = value;
      }
    }

    private int? _handle;

    public Shader(string name, ShaderType type,
      Dictionary<string, Parameter> parameters,
      Dictionary<string, Function> functions)
    {
      this.Name = name;
      this.Type = type;
      this.Parameters = parameters;
      this.Functions = functions;
    }

    public string GenerateSource()
    {
      StringBuilder sb = new StringBuilder();
      
      foreach (Parameter p in Parameters.Values)
      {
        sb.AppendLine(p.ToString());
      }

      foreach (Function f in Functions.Values)
      {
        sb.AppendLine(f.ToString(f.name == EntryFunction));
      }

      return sb.ToString();
    }

    public void Compile()
    {
      Handle = GL.CreateShader(this.Type);
      GL.ShaderSource(Handle, GenerateSource());
      GL.CompileShader(Handle);

      // Validate
      int status;
      GL.GetShader(Handle, ShaderParameter.CompileStatus, out status);
      if (status == 0)
      {
        string message = GL.GetShaderInfoLog(Handle);
        throw new ApplicationException(string.Format(
          "Error compiling {0} shader \"{1}\":\n{2}",
          this.Type,
          this.Name,
          message));
      }
    }

    public void Delete()
    {
      GL.DeleteShader(this.Handle);
    }
  }
}

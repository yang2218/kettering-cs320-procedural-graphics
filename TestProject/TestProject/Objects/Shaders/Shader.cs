﻿using System;
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

      public class List : Dictionary<string, Parameter>
      {
        public void Add(string name, Parameter.InterpolationQualifier interp,
          Parameter.StorageQualifier stor, Parameter.GLSLType type)
        {
          base.Add(name, new Parameter(interp, stor, type, name));
        }

        public void Add(string name, Parameter.GLSLType type)
        {
          base.Add(name, new Parameter(type, name));
        }

        public void Add(string name, VertexData.Attribute attribute,
          GLSLType type)
        {
          base.Add(name, new Parameter(attribute, type, name));
        }
      }

      public InterpolationQualifier interpolation;
      public StorageQualifier storage;
      public GLSLType type;
      public VertexData.Attribute attribute;
      public string name;

      public bool IsInputAttribute
      {
        get
        {
          return attribute != VertexData.Attribute.None;
          /*
          return storage == StorageQualifier.In
              || storage == StorageQualifier.CentroidIn
              || storage == StorageQualifier.PatchIn
              || storage == StorageQualifier.SampleIn;
          */
        }
      }

      public bool IsUniform
      {
        get
        {
          return storage == StorageQualifier.Uniform;
        }
      }

      public static Parameter Void = new Parameter()
      {
        interpolation = InterpolationQualifier.None,
        storage = StorageQualifier.None,
        type = GLSLType.Void,
        name = string.Empty,
        attribute = VertexData.Attribute.None
      };

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
        this.attribute = VertexData.Attribute.None;
      }

      public Parameter(GLSLType type, string name)
      {
        this.interpolation = InterpolationQualifier.None;
        this.storage = StorageQualifier.Uniform;
        this.type = type;
        this.name = name;
        this.attribute = VertexData.Attribute.None;
      }

      public Parameter(
        VertexData.Attribute attribute,
        GLSLType type,
        string name)
      {
        this.interpolation = InterpolationQualifier.None;
        this.storage = StorageQualifier.In;
        this.type = type;
        this.name = name;
        this.attribute = attribute;
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

        sb.Append(TypeToString());
        sb.Append(" ");
        sb.Append(name);
        sb.Append(";");

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
            sb.Append(parameters[i].TypeToString());
            sb.Append(" ");
            sb.Append(parameters[i].name);
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
      // for now, just use this switch stmt
      Shader s;
      switch (name)
      {
        #region Pos
        case "Pos":
          s = new Shader(
            "Pos",
            ShaderType.VertexShader,
            new Dictionary<string, Parameter>()
            {
              {"position", new Parameter(
                VertexData.Attribute.Position,
                Parameter.GLSLType.Vec3,
                "position")}
            },
            new Dictionary<string, Function>()
            {
              {"pos_main", new Function(
                Parameter.Void,
                "pos_main",
                new Parameter[]{},
                "gl_Position = vec4(position, 1.0);")}
            },
            "pos_main");
          break;
        #endregion
        #region VT
        case "VT":
          s = new Shader(
            "VT",
            ShaderType.VertexShader,
            new Parameter.List
            {
              {"position", VertexData.Attribute.Position, Parameter.GLSLType.Vec3},
              {"in_texcoord", VertexData.Attribute.TexCoord, Parameter.GLSLType.Vec2},
              {"fragTexCoord", 
                Parameter.InterpolationQualifier.Smooth,
                Parameter.StorageQualifier.Out,
                Parameter.GLSLType.Vec2}
            },
            new Dictionary<string, Function>()
            {
              {"vt_main", new Function(
                Parameter.Void,
                "vt_main",
                new Parameter[]{},
                @"
gl_Position = vec4(position, 1.0);
fragTexCoord = in_texcoord;
")}
            },
            "vt_main");
          break;
        #endregion
        #region VNT
        case "VNT":
            s = new Shader(
              "PosNorm",
              ShaderType.VertexShader,
              new Parameter.List
              {
                {"position", VertexData.Attribute.Position, Parameter.GLSLType.Vec3},
                {"in_normal", VertexData.Attribute.Normal, Parameter.GLSLType.Vec3},
                {"in_texcoord", VertexData.Attribute.TexCoord, Parameter.GLSLType.Vec2},
                {"mv_matrix", Parameter.GLSLType.Mat4},
                {"norm_matrix", Parameter.GLSLType.Mat4},
                {"fragNormal",
                  Parameter.InterpolationQualifier.Smooth,
                  Parameter.StorageQualifier.Out,
                  Parameter.GLSLType.Vec3},
                {"fragTexCoord", 
                  Parameter.InterpolationQualifier.Smooth,
                  Parameter.StorageQualifier.Out,
                  Parameter.GLSLType.Vec2},
                {"fragTexCoordNP",
                  Parameter.InterpolationQualifier.NoPerspective,
                  Parameter.StorageQualifier.Out,
                  Parameter.GLSLType.Vec2},
                {"v",
                  Parameter.InterpolationQualifier.Smooth,
                  Parameter.StorageQualifier.Out,
                  Parameter.GLSLType.Vec3}
              },
              new Dictionary<string, Function>()
              {
                {"vnt_main", new Function(
                  Parameter.Void,
                  "vnt_main",
                  new Parameter[]{},
                  @"
  gl_Position = vec4(position, 1.0);
  v = vec3(mv_matrix * gl_Position);
  fragNormal = normalize(mat3(norm_matrix) * in_normal);
  fragTexCoord = in_texcoord;
  fragTexCoordNP = in_texcoord;
  ")}
              },
              "vnt_main");
          break;
        #endregion
        #region FlatCol
        case "FlatCol":
          s = new Shader(
            "FlatCol",
            ShaderType.FragmentShader,
            new Dictionary<string, Parameter>()
            {
              {"color", new Parameter(
                Parameter.InterpolationQualifier.None,
                Parameter.StorageQualifier.Uniform,
                Parameter.GLSLType.Vec3,
                "color")},
              {"out_Color", new Parameter(
                Parameter.InterpolationQualifier.None,
                Parameter.StorageQualifier.Out,
                Parameter.GLSLType.Vec4,
                "out_Color")}
            },
            new Dictionary<string, Function>()
            {
              {"flatcol_main", new Function(
                Parameter.Void,
                "flatcol_main",
                new Parameter[]{},
                "out_Color = vec4(color, 1.0);")}
            },
            "flatcol_main");
          break;
        #endregion
        #region DiffSpec
        case "DiffSpec":
          s = new Shader(
            "DiffSpec",
            ShaderType.FragmentShader,
            new Parameter.List
            {
              {"out_Color",
                Parameter.InterpolationQualifier.None,
                Parameter.StorageQualifier.Out,
                Parameter.GLSLType.Vec4
              },
              {"v",
                Parameter.InterpolationQualifier.Smooth,
                Parameter.StorageQualifier.In,
                Parameter.GLSLType.Vec3},
              {"fragNormal", 
                Parameter.InterpolationQualifier.Smooth,
                Parameter.StorageQualifier.In,
                Parameter.GLSLType.Vec3},
              {"fragTexCoord",
                Parameter.InterpolationQualifier.Smooth,
                Parameter.StorageQualifier.In,
                Parameter.GLSLType.Vec2},
              {"fragTexCoordNP", 
                Parameter.InterpolationQualifier.NoPerspective,
                Parameter.StorageQualifier.In,
                Parameter.GLSLType.Vec2},
              {"textureMap", Parameter.GLSLType.Sampler2D},
              {"npInterp", Parameter.GLSLType.Bool},
              {"matCol", Parameter.GLSLType.Vec3},
              {"matShiny", Parameter.GLSLType.Float},
              {"ambLight", Parameter.GLSLType.Vec3},
              {"pointLightPos", Parameter.GLSLType.Vec3}, // window space!
              {"pointLightCol", Parameter.GLSLType.Vec3},
              {"pointLightRange", Parameter.GLSLType.Float}
            },
            new Dictionary<string, Function>()
            {
              {"lengthSq", new Function(
                new Parameter(Parameter.GLSLType.Float, string.Empty),
                "lengthSq",
                new Parameter[] { new Parameter(Parameter.GLSLType.Vec3, "v") },
                "return v.x*v.x + v.y*v.y + v.z*v.z;")},
              {"diffspec_main", new Function(
                Parameter.Void,
                "diffspec_main",
                new Parameter[]{},
                @"
vec3 dir;
float dist, distSq;

dir = pointLightPos - v;
distSq = lengthSq(dir);
dist = sqrt(distSq);
dir = dir / dist;

vec3 col;
col = ambLight;

float surfDot;
surfDot = max(dot(fragNormal, dir), 0);
col += pointLightCol * (surfDot /* pointLightRange / distSq*/);

vec3 e = normalize(-v);
vec3 r = normalize(-reflect(e, dir));
surfDot = pow(max(dot(r, e), 0), matShiny);
col += pointLightCol * (surfDot /* pointLightRange / distSq*/);

vec4 texCol;
if (npInterp)
  texCol = texture(textureMap, fragTexCoordNP);
else
  texCol = texture(textureMap, fragTexCoord);

col *= texCol.rgb * matCol;

out_Color = vec4(col, texCol.a);
")}
            },
            "diffspec_main");
          break;
        #endregion
        #region Tex
        case "Tex":
          s = new Shader(
            "Tex",
            ShaderType.FragmentShader,
            new Parameter.List
            {
              {"textureMap", Parameter.GLSLType.Sampler2D},
              {"minBox", Parameter.GLSLType.Vec2},
              {"maxBox", Parameter.GLSLType.Vec2},
              {"boxColor", Parameter.GLSLType.Vec4},
              {"fragTexCoord",
                Parameter.InterpolationQualifier.Smooth,
                Parameter.StorageQualifier.In,
                Parameter.GLSLType.Vec2},
              {"out_Color",
                Parameter.InterpolationQualifier.None,
                Parameter.StorageQualifier.Out,
                Parameter.GLSLType.Vec4
              }
            },
            new Dictionary<string, Function>()
            {
              {"tex_main", new Function(
                Parameter.Void,
                "tex_main",
                new Parameter[]{},
                @"
out_Color = texture(textureMap, fragTexCoord);
if (fragTexCoord.x > minBox.x
  && fragTexCoord.x < maxBox.x
  && fragTexCoord.y > minBox.y
  && fragTexCoord.y < maxBox.y)
{
  out_Color.rgb = out_Color.rgb * (1.0 - boxColor.a) + boxColor.a * boxColor.rgb;
}
")}
            },
            "tex_main");
          break;
        #endregion
        default:
          throw new ApplicationException(string.Format("Could not load shader {0}", name));
      }

      Cache[name] = s;
      return s;
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
      Dictionary<string, Function> functions,
      string entryFunction)
    {
      this.Name = name;
      this.Type = type;
      this.Parameters = parameters;
      this.Functions = functions;
      this.EntryFunction = entryFunction;
    }

    public IEnumerable<Parameter> InputAttributes
    {
      get
      {
        foreach (Parameter p in Parameters.Values)
        {
          if (p.IsInputAttribute)
          {
            yield return p;
          }
        }
      }
    }

    public IEnumerable<Parameter> Uniforms
    {
      get
      {
        foreach (Parameter p in Parameters.Values)
        {
          if (p.IsUniform)
          {
            yield return p;
          }
        }
      }
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
      this._handle = null;
    }
  }
}

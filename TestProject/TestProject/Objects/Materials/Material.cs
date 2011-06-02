using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TestProject.Objects.Shaders;

namespace TestProject.Objects
{
  /// <summary>
  /// Represents a way to draw objects; usually, a shader program and a set
  /// of uniform values, as well as potentially other GL state (blending, etc.).
  /// </summary>
  public class Material
  {
    public sealed class MaterialCache
    {
      private Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

      public Material this[string name]
      {
        get
        {
          Material shader;
          if (materialCache.TryGetValue(name, out shader))
          {
            return shader;
          }

          return LoadMaterial(name);
        }
        set
        {
          materialCache[name] = value;
        }
      }

      public bool TryGet(string name, out Material mat)
      {
        return materialCache.TryGetValue(name, out mat);
      }
    }

    private static MaterialCache _cache = new MaterialCache();
    public static MaterialCache Cache { get { return _cache; } }

    public string Name { get; set; }
    public ShaderProgram Program { get; set; }

    public bool IsShared {  get; private set; }
    
    private Dictionary<string, object> UniformParameters = new Dictionary<string, object>();
    
    public Material()
    {
      IsShared = true;
    }
    
    private static Material LoadMaterial(string name)
    {
      switch (name)
      {
        case "Default":
          return DefaultMaterial();
        case "DefaultTextured":
          return DefaultTexturedMaterial();
        case "TweenLines":
          return TweenLinesMaterial();
        case "VT_Tex":
          Material mat = new Material();
          mat.Name = "VT_Tex";
          Simple1MVPShaderModifier mod = new Simple1MVPShaderModifier();
          mat.Program = mod.DeriveProgram(ShaderProgram.Cache["VT_Tex"]);
          mat.Program.Compile();
          Cache[mat.Name] = mat;
          return mat;
        default:
          // load from file
          throw new NotImplementedException();
      }
    }

    private static Material TweenLinesMaterial()
    {
      Material mat;
      if (Cache.TryGet("TweenLines", out mat))
      {
        return mat;
      }

      mat = new Material();
      // TODO: replace three matrixes with one MVP (do multiplication on client-side)
      string vertShaderSource = @"
#version 140

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

in vec3 position1;
in vec3 position2;
in float tween;

void main(void)
{
  gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(position1 + tween * (position2 - position1), 1.0);
}
";
      string fragShaderSource = @"
#version 140

uniform vec3 color;

out vec4 out_Color;

void main(void)
{
  out_Color = vec4(color, 1.0);
}
";

      mat.Name = "Test Material";

      // TODO: move to a compile function
      // TODO: check the cache for these shaders first
      /*
      mat.vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
      mat.fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

      GL.ShaderSource(mat.vertexShaderHandle, vertShaderSource);
      GL.CompileShader(mat.vertexShaderHandle);
      ValidateShader(mat.vertexShaderHandle, "test vertex shader");

      GL.ShaderSource(mat.fragmentShaderHandle, fragShaderSource);
      GL.CompileShader(mat.fragmentShaderHandle);
      ValidateShader(mat.fragmentShaderHandle, "test fragmenmt shader");

      // TODO: optionally compile a geometry shader


      mat.shaderProgramHandle = GL.CreateProgram();
      GL.AttachShader(mat.shaderProgramHandle, mat.vertexShaderHandle);
      GL.AttachShader(mat.shaderProgramHandle, mat.fragmentShaderHandle);
      // TODO: optionally attach geometry shader

      // we need to make sure the indices map to the same number across all VertexData subclasses
      // for more exotic data, I need to add in custom vertex array support
      GL.BindAttribLocation(mat.shaderProgramHandle, 0, "position1");
      GL.BindAttribLocation(mat.shaderProgramHandle, 1, "position2");
      GL.BindAttribLocation(mat.shaderProgramHandle, 2, "tween");

      GL.LinkProgram(mat.shaderProgramHandle);
      ValidateProgram(mat.shaderProgramHandle);

      materialCache["TweenLines"] = mat;*/
      return mat;
    }

    private static Material DefaultMaterial()
    {
      Material mat;
      if (Cache.TryGet("Default", out mat))
      {
        return mat;
      }
      
      mat = new Material();
      mat.Name = "Default";

      // TODO: this part, now that I actually use it, seems really kludgy
      // redesign somehow?
      Simple1MVPShaderModifier mod = new Simple1MVPShaderModifier();
      mat.Program = mod.DeriveProgram(ShaderProgram.Cache["Pos_FlatCol"]);

      mat.Program.Compile();

      Cache["Default"] = mat;
      return mat;
    }



    private static Material DefaultTexturedMaterial()
    {
      Material mat;
      if (Cache.TryGet("DefaultTextured", out mat))
      {
        return mat;
      }

      mat = new Material();
      string vertShaderSource = @"
#version 330

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

in vec3 position;
in vec2 uv;

smooth out vec2 texCoord;

void main(void)
{
  texCoord = uv;
  gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(position, 1.0);
}
";
      string fragShaderSource = @"
#version 330

uniform sampler2D textureMap; 

smooth in vec2 texCoord;

out vec4 out_Color;

void main(void)
{
  out_Color = texture(textureMap, texCoord);
  if (out_Color.a < 0.5)
  {
    discard;
  }
}
";

      mat.Name = "Test Material";
      /*
      // TODO: move to a compile function
      // TODO: check the cache for these shaders first

      mat.vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
      mat.fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

      GL.ShaderSource(mat.vertexShaderHandle, vertShaderSource);
      GL.CompileShader(mat.vertexShaderHandle);
      ValidateShader(mat.vertexShaderHandle, "test vertex shader 2");

      GL.ShaderSource(mat.fragmentShaderHandle, fragShaderSource);
      GL.CompileShader(mat.fragmentShaderHandle);
      ValidateShader(mat.fragmentShaderHandle, "test fragmenmt shader 2");

      // TODO: optionally compile a geometry shader


      mat.shaderProgramHandle = GL.CreateProgram();
      GL.AttachShader(mat.shaderProgramHandle, mat.vertexShaderHandle);
      GL.AttachShader(mat.shaderProgramHandle, mat.fragmentShaderHandle);
      // TODO: optionally attach geometry shader

      // we need to make sure the indices map to the same number across all VertexData subclasses
      // for more exotic data, I need to add in custom vertex array support
      GL.BindAttribLocation(mat.shaderProgramHandle, 0, "position");
      GL.BindAttribLocation(mat.shaderProgramHandle, 1, "uv");

      GL.LinkProgram(mat.shaderProgramHandle);
      ValidateProgram(mat.shaderProgramHandle);

      materialCache["DefaultTextured"] = mat;*/
      return mat;
    }

    private static Material UberTextMaterial()
    {
      Material mat;
      if (Cache.TryGet("DefaultTextured", out mat))
      {
        return mat;
      }

      mat = new Material();
      string vertShaderSource = @"
#version 330

uniform mat4 MVPMatrix;

in vec3 position;
in uvec2 uv;

smooth out uvec2 TextureCoord;

void main(void)
{
  TextureCoord = uv;
  gl_Position = MVPMatrix * vec4(position, 1.0);
}
";
      string fragShaderSource = @"
in uvec2 TextureCoord;

uniform sampler2D DistanceField;
uniform vec3 OutlineColor;
uniform vec3 GlyphColor;
uniform vec3 GlowColor;

uniform bool Outline;
uniform bool Glow;
uniform bool Shadow;

const vec2 ShadowOffset = vec2(0.005, 0.01);2
const vec3 ShadowColor = vec3(0.0, 0.0, 0.125);3
const float SmoothCenter = 0.5;4
const float OutlineCenter = 0.4;5
const float GlowBoundary = 1.0;6

void main(void)
{
  vec4 color = utexture2D(DistanceField, TextureCoord);
  float distance = color.a;
  float smoothWidth = fwidth(distance);
  float alpha;
  vec3 rgb;

  if (Outline) {
      float mu = smoothstep(OutlineCenter - smoothWidth,
                                    OutlineCenter + smoothWidth,
                                    distance);
      alpha = smoothstep(SmoothCenter - smoothWidth,
                         SmoothCenter + smoothWidth, distance)
      rgb = mix(GlyphColor, OutlineColor, mu);
  }

  if (Glow) {
    float mu = smoothstep(SmoothCenter - smoothWidth,
                                  SmoothCenter + smoothWidth, 
                                  distance);
    rgb = mix(GlyphColor, GlowColor, mu);
    alpha = smoothstep(SmoothCenter, GlowBoundary, sqrt(distance));
  }

  if (Shadow) {
      float distance2 = texture2D(DistanceField, 
                                          TextureCoord + ShadowOffset).a;
      float s = smoothstep(SmoothCenter - smoothWidth,
                                   SmoothCenter + smoothWidth, 
                                   distance2);
      float v = smoothstep(SmoothCenter - smoothWidth,
                                   SmoothCenter + smoothWidth, 
                                   distance);
      
      // If s is 0 then we're inside the shadow; 
      // if it's 1 then we're outside the shadow.
      //
      // If v is 0 then we're inside the vector; 
      // if it's 1 then we're outside the vector.
      
      // Totally inside the vector (i.e., inside the glyph):
      if (v == 0.0) {
          rgb = GlyphColor;
          alpha = 0.0;
      }
      
      // On a non-shadowed vector edge:
      else if (s == 1.0 && v != 1.0) {
          rgb = GlyphColor;
          alpha = v;
      }

      // Totally inside the shadow:
      else if (s == 0.0 && v == 1.0) {
          rgb = ShadowColor;
          alpha = 0.0;
      }

      // On a shadowed vector edge:
      else if (s == 0.0) {
          rgb = mix(GlyphColor, ShadowColor, v);
          alpha = 0.0;
      }

      // On the shadow's outside edge:
      else {
          rgb = mix(GlyphColor, ShadowColor, v);
          alpha = s;
      }
  }

  gl_FragColor = vec4(rgb, alpha);
}
"; // TODO: look up code from the original whitepaper

      // check if GL version?

      mat.Name = "Test Material";

      // TODO: move to a compile function
      // TODO: check the cache for these shaders first

      Cache["DefaultTextured"] = mat;
      return mat;
    }

    
    public void Use(ref Matrix4 modelMatrix) // TODO: maybe a better way to assign the model matrix?
    {
      /*
      GL.UseProgram(Program.Handle);

      int location;
      location = GL.GetUniformLocation(Program.Handle, "projectionMatrix");
      GL.UniformMatrix4(location, false, ref Viewport.Active.projectionMatrix);
      location = GL.GetUniformLocation(Program.Handle, "viewMatrix");
      GL.UniformMatrix4(location, false, ref Viewport.Active.viewMatrix);
      location = GL.GetUniformLocation(Program.Handle, "modelMatrix");
      GL.UniformMatrix4(location, false, ref modelMatrix);

      // temporary code until I get a more programmatic uniform setting worked out
      object value;
      if (UniformParameters.TryGetValue("color", out value))
      {
        location = GL.GetUniformLocation(Program.Handle, "color");
        GL.Uniform3(location, (Vector3)value);
      }
      if (UniformParameters.TryGetValue("texture", out value))
      {
        int texid = (int)value; // ((Texture)value).TextureID;
        location = GL.GetUniformLocation(Program.Handle, "textureMap");
        GL.Uniform1(location, (int)TextureUnit.Texture0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texid);
      }
      if (UniformParameters.TryGetValue("tween", out value))
      {
        location = GL.GetUniformLocation(Program.Handle, "tween");
        GL.Uniform1(location, (float)value);
      }
      //*/
      /*
      // TODO: set global uniforms
      int location;
      location = GL.GetUniformLocation(shaderProgramHandle, "projectionMatrix");
      GL.UniformMatrix4(location, false, ref projectionMatrix);
      location = GL.GetUniformLocation(shaderProgramHandle, "viewMatrix");
      GL.UniformMatrix4(location, false, ref viewMatrix);
      location = GL.GetUniformLocation(shaderProgramHandle, "modelMatrix");
      GL.UniformMatrix4(location, false, ref entity.transform);
      // time

      // use custom uniforms
      // TODO: get count of uniforms, get names/indices, get types, assign
      // maybe cache this process
      string[] names;
      int[] indices;
      
      GL.GetUniformIndices(shaderProgramHandle, 99, names, indices) 
      GL.GetActiveUniform(shaderProgramHandle, index, out bufSize, out length, out size, out type, out name);
      */

      // TOOD: mapping output parameters... if I ever use those
    }

    
    
    public virtual Material CloneInstance()
    {
      Material clone = (Material)MemberwiseClone();
      clone.IsShared = false;
      clone.UniformParameters = new Dictionary<string, object>(this.UniformParameters);
      return clone;
    }

    public object this[string uniform]
    {
      get
      {
        return UniformParameters[uniform];
      }
      set
      {
        UniformParameters[uniform] = value;
      }
    }


    public void DrawEntities(LinkedList<Entity> list)
    {
      // TODO: these entity lists should go to the shared instance of the material,
      // so instacing across any varying uniforms can be performed

      Program.Use();
      SetMaterialUniforms();

      foreach (Entity e in list)
      {
        SetEntityTransform(e);
        e.VertexData.Draw();
      }
    }

    private void SetEntityTransform(Entity e)
    {
      // TODO: figure out how we're gonna figure out how to get transform data to the shader
      // for now, just assume there's one transform matrix
      Matrix4 transform;
      e.transform.RegenerateMatrix();
      Matrix4.Mult(ref e.transform.matrix, ref Viewport.Active.vpMatrix, out transform);
      int location = GL.GetUniformLocation(Program.Handle, "in_mvpMatrix");
      GL.UniformMatrix4(location, false, ref transform);
      /*
      Matrix4.Mult(ref e.transform.matrix, ref Viewport.Active.viewMatrix, out transform);
      location = GL.GetUniformLocation(Program.Handle, "mv_matrix");
      GL.UniformMatrix4(location, false, ref transform);

      transform.Invert();
      transform.Transpose();
      location = GL.GetUniformLocation(Program.Handle, "norm_matrix");
      GL.UniformMatrix4(location, false, ref transform);
      */
    }

    protected void SetMaterialUniforms()
    {
      Dictionary<string, Shader.Parameter> uniforms = Program.Uniforms;
      Shader.Parameter parameter;
      int programHandle = Program.Handle;
      int location;
      foreach (string paramName in UniformParameters.Keys)
      {
        if (uniforms.TryGetValue(paramName, out parameter) == false)
        {
          continue;
        }

        location = GL.GetUniformLocation(programHandle, paramName);
        // TODO: do we need to test if location = -1 (for inactive uniforms)?
        object value = UniformParameters[paramName];

        #region Ungodly switch block
        switch (parameter.type)
        {
          case Shader.Parameter.GLSLType.Void:
            break;
          case Shader.Parameter.GLSLType.Bool:
            GL.Uniform1(location, (bool)value ? 1 : 0);
            break;
          case Shader.Parameter.GLSLType.Int:
            GL.Uniform1(location, (int)value);
            break;
          case Shader.Parameter.GLSLType.Uint:
            GL.Uniform1(location, (uint)value);
            break;
          case Shader.Parameter.GLSLType.Float:
          case Shader.Parameter.GLSLType.Double:
            GL.Uniform1(location, (float)value);
            break;
          case Shader.Parameter.GLSLType.Vec2:
          case Shader.Parameter.GLSLType.Dvec2:
            GL.Uniform2(location, (Vector2)value);
            break;
          case Shader.Parameter.GLSLType.Bvec2:
          case Shader.Parameter.GLSLType.Ivec2:
            GL.Uniform2(location, (int)((Vector2)value).X, (int)((Vector2)value).Y);
            break;
          case Shader.Parameter.GLSLType.Uvec2:
            GL.Uniform2(location, (uint)((Vector2)value).X, (uint)((Vector2)value).Y);
            break;
          case Shader.Parameter.GLSLType.Vec3:
          case Shader.Parameter.GLSLType.Dvec3:
            GL.Uniform3(location, (Vector3)value);
            break;
          case Shader.Parameter.GLSLType.Bvec3:
          case Shader.Parameter.GLSLType.Ivec3:
            GL.Uniform3(location, (int)((Vector3)value).X,
              (int)((Vector3)value).Y, (int)((Vector3)value).Z);
            break;
          case Shader.Parameter.GLSLType.Uvec3:
            GL.Uniform3(location, (uint)((Vector3)value).X,
              (uint)((Vector3)value).Y, (uint)((Vector3)value).Z);
            break;
          case Shader.Parameter.GLSLType.Vec4:
          case Shader.Parameter.GLSLType.Dvec4:
            GL.Uniform4(location, (Vector4)value);
            break;
          case Shader.Parameter.GLSLType.Bvec4:
          case Shader.Parameter.GLSLType.Ivec4:
            GL.Uniform4(location, (int)((Vector4)value).X,
              (int)((Vector4)value).Y, (int)((Vector4)value).Z,
              (int)((Vector4)value).W);
            break;
          case Shader.Parameter.GLSLType.Uvec4:
            GL.Uniform4(location, (uint)((Vector4)value).X,
              (uint)((Vector4)value).Y, (uint)((Vector4)value).Z,
              (uint)((Vector4)value).W);
            break;
          case Shader.Parameter.GLSLType.Mat2:
            break;
          case Shader.Parameter.GLSLType.Mat3:
            break;
          case Shader.Parameter.GLSLType.Mat4:
            {
              Matrix4 mat = (Matrix4)value;
              GL.UniformMatrix4(location, false, ref mat);
            }
            break;
          case Shader.Parameter.GLSLType.Mat2x2:
            break;
          case Shader.Parameter.GLSLType.Mat3x2:
            break;
          case Shader.Parameter.GLSLType.Mat4x2:
            break;
          case Shader.Parameter.GLSLType.Mat2x3:
            break;
          case Shader.Parameter.GLSLType.Mat3x3:
            break;
          case Shader.Parameter.GLSLType.Mat4x3:
            break;
          case Shader.Parameter.GLSLType.Mat2x4:
            break;
          case Shader.Parameter.GLSLType.Mat3x4:
            break;
          case Shader.Parameter.GLSLType.Mat4x4:
            break;
          case Shader.Parameter.GLSLType.Dmat2:
            break;
          case Shader.Parameter.GLSLType.Dmat3:
            break;
          case Shader.Parameter.GLSLType.Dmat4:
            break;
          case Shader.Parameter.GLSLType.Dmat2x2:
            break;
          case Shader.Parameter.GLSLType.Dmat3x2:
            break;
          case Shader.Parameter.GLSLType.Dmat4x2:
            break;
          case Shader.Parameter.GLSLType.Dmat2x3:
            break;
          case Shader.Parameter.GLSLType.Dmat3x3:
            break;
          case Shader.Parameter.GLSLType.Dmat4x3:
            break;
          case Shader.Parameter.GLSLType.Dmat2x4:
            break;
          case Shader.Parameter.GLSLType.Dmat3x4:
            break;
          case Shader.Parameter.GLSLType.Dmat4x4:
            break;
          case Shader.Parameter.GLSLType.Sampler1D:
            break;
          case Shader.Parameter.GLSLType.Sampler2D:
            GL.Uniform1(location, 0); // texture unit 0
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, (int)value);
            break;
          case Shader.Parameter.GLSLType.Sampler3D:
            break;
          case Shader.Parameter.GLSLType.SamplerCube:
            break;
          case Shader.Parameter.GLSLType.Sampler2DRect:
            break;
          case Shader.Parameter.GLSLType.Sampler1DShadow:
            break;
          case Shader.Parameter.GLSLType.Sampler2DShadow:
            break;
          case Shader.Parameter.GLSLType.Sampler2DRectShadow:
            break;
          case Shader.Parameter.GLSLType.Sampler1DArray:
            break;
          case Shader.Parameter.GLSLType.Sampler2DArray:
            break;
          case Shader.Parameter.GLSLType.SamplerBuffer:
            break;
          case Shader.Parameter.GLSLType.Sampler2DMS:
            break;
          case Shader.Parameter.GLSLType.Sampler2DMSArray:
            break;
          case Shader.Parameter.GLSLType.SamplerCubeArray:
            break;
          case Shader.Parameter.GLSLType.SamplerCubeArrayShadow:
            break;
          case Shader.Parameter.GLSLType.Isampler1D:
            break;
          case Shader.Parameter.GLSLType.Isampler2D:
            break;
          case Shader.Parameter.GLSLType.Isampler3D:
            break;
          case Shader.Parameter.GLSLType.IsamplerCube:
            break;
          case Shader.Parameter.GLSLType.Isampler2DRect:
            break;
          case Shader.Parameter.GLSLType.Isampler1DArray:
            break;
          case Shader.Parameter.GLSLType.Isampler2DArray:
            break;
          case Shader.Parameter.GLSLType.IsamplerBuffer:
            break;
          case Shader.Parameter.GLSLType.Isampler2DMS:
            break;
          case Shader.Parameter.GLSLType.Isampler2DMSArray:
            break;
          case Shader.Parameter.GLSLType.IsamplerCubeArray:
            break;
          case Shader.Parameter.GLSLType.Usampler1D:
            break;
          case Shader.Parameter.GLSLType.Usampler2D:
            break;
          case Shader.Parameter.GLSLType.Usampler3D:
            break;
          case Shader.Parameter.GLSLType.UsamplerCube:
            break;
          case Shader.Parameter.GLSLType.Usampler2DRect:
            break;
          case Shader.Parameter.GLSLType.Usampler1DArray:
            break;
          case Shader.Parameter.GLSLType.Usampler2DArray:
            break;
          case Shader.Parameter.GLSLType.UsamplerBuffer:
            break;
          case Shader.Parameter.GLSLType.Usampler2DMS:
            break;
          case Shader.Parameter.GLSLType.Usampler2DMSArray:
            break;
          case Shader.Parameter.GLSLType.UsamplerCubeArray:
            break;
          default:
            break;
        }
        #endregion
      }
    }
  }
}

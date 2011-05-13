using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects
{
  public class Material
  {
    // represents a shader program, some gl state flags (blending, etc.) and a set of uniforms.
    // cache shader handles so programs can be shared by several materials
    // changing a material parameter via an entity will clone the material (if not already a dedicated instace),
    // so the change will not affect other entities sharing the same material.

    // properties: corespond to the shader uniforms/samplers
    // an indexer to assign values directly to the parameter by name
    // TODO: how to map vertex attributes to data? specifically, how to cache/manage vertex array objects

    // compile: compiles shaders  and upload if needed
    // BindVertexAttributes: bind given vao or manually bind using the given vertexdata
    // GetVAO: generates or retrieves from cache a vao for binding with the given vertexdata
    // Use: use for next draw call(s)
    // Unload: detaches, deletes shaders&program
    
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

    public string Name { get; private set; }
    public ShaderProgram Program { get; protected set; }

    private bool loaded, global;
    
    private Dictionary<string, object> UniformParameters = new Dictionary<string, object>();
    // any global uniforms like projection, view, time are set at runtime.

    // TODO: cache any compiled shaders, as well as programs

    protected Material()
    {
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
      // TODO: replace three matrixes with one MVP (do multiplication on client-side)
      string vertShaderSource = @"
#version 140

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

in vec3 position;

void main(void)
{
  gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(position, 1.0);
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
      GL.BindAttribLocation(mat.shaderProgramHandle, 0, "position");

      GL.LinkProgram(mat.shaderProgramHandle);
      ValidateProgram(mat.shaderProgramHandle);

      materialCache["Default"] = mat;*/
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
      GL.UseProgram(shaderProgramHandle);

      int location;
      location = GL.GetUniformLocation(shaderProgramHandle, "projectionMatrix");
      GL.UniformMatrix4(location, false, ref Viewport.Active.projectionMatrix);
      location = GL.GetUniformLocation(shaderProgramHandle, "viewMatrix");
      GL.UniformMatrix4(location, false, ref Viewport.Active.viewMatrix);
      location = GL.GetUniformLocation(shaderProgramHandle, "modelMatrix");
      GL.UniformMatrix4(location, false, ref modelMatrix);

      // temporary code until I get a more programmatic uniform setting worked out
      object value;
      if (UniformParameters.TryGetValue("color", out value))
      {
        location = GL.GetUniformLocation(shaderProgramHandle, "color");
        GL.Uniform3(location, (Vector3)value);
      }
      if (UniformParameters.TryGetValue("texture", out value))
      {
        int texid = (int)value; // ((Texture)value).TextureID;
        location = GL.GetUniformLocation(shaderProgramHandle, "textureMap");
        GL.Uniform1(location, (int)TextureUnit.Texture0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texid);
      }
      if (UniformParameters.TryGetValue("tween", out value))
      {
        location = GL.GetUniformLocation(shaderProgramHandle, "tween");
        GL.Uniform1(location, (float)value);
      }
      */
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
      clone.global = false;
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

  }
}

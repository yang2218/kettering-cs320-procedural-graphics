using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ThereBeMonsters.Front_end
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

    public string MaterialName { get; private set; }

    private bool loaded, global;
    private int shaderProgramHandle, vertexShaderHandle, geometryShaderHandle, fragmentShaderHandle;
    
    private Dictionary<string, object> UniformParameters = new Dictionary<string, object>();
    // any global uniforms like projection, view, time are set at runtime.

    // TODO: cache any compiled shaders, as well as programs

    private Material()
    {
    }

    private static Dictionary<string, Material> materialCache = new Dictionary<string,Material>();

    public static Material GetMaterial(string name)
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
      if (materialCache.TryGetValue("TweenLines", out mat))
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

      mat.MaterialName = "Test Material";

      // TODO: move to a compile function
      // TODO: check the cache for these shaders first

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

      materialCache["TweenLines"] = mat;
      return mat;
    }

    private static Material DefaultMaterial()
    {
      Material mat;
      if (materialCache.TryGetValue("Default", out mat))
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

      mat.MaterialName = "Test Material";

      // TODO: move to a compile function
      // TODO: check the cache for these shaders first

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

      materialCache["Default"] = mat;
      return mat;
    }

    private static Material DefaultTexturedMaterial()
    {
      Material mat;
      if (materialCache.TryGetValue("DefaultTextured", out mat))
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

uniform vec3 color;
uniform sampler2D textureMap;

smooth in vec2 texCoord;

out vec4 out_Color;

void main(void)
{
  //out_Color = vec4(color, 1.0) * texture(textureMap, texCoord);
  out_Color = texture(textureMap, texCoord);
}
";

      mat.MaterialName = "Test Material";

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

      materialCache["DefaultTextured"] = mat;
      return mat;
    }

    public void Use(ref Matrix4 modelMatrix) // TODO: maybe a better way to assign the model matrix?
    {
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
        int texid = ((Texture)value).TextureID;
        location = GL.GetUniformLocation(shaderProgramHandle, "textureMap");
        GL.Uniform1(location, 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texid);
      }
      if (UniformParameters.TryGetValue("tween", out value))
      {
        location = GL.GetUniformLocation(shaderProgramHandle, "tween");
        GL.Uniform1(location, (float)value);
      }
      
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

    public Material CloneInstance()
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

    // TODO: use the validation instructins from the opengl wiki?

    /// [Copied from swiftless tutorials: http://www.swiftless.com/]
    /// Given a shader and the filename associated with it, validateShader will
	  /// then get information from OpenGl on whether or not the shader was compiled successfully
	  /// and if it wasn't, it will output the file with the problem, as well as the problem.
    private static void ValidateShader(int handle, string shaderFile)
    {
      string message;
      GL.GetProgramInfoLog(handle, out message);
      if (string.IsNullOrEmpty(message) == false)
      {
        // TODO: proper error handling
        System.Console.WriteLine(string.Format("Error compiling shader {0}:\n{1}", shaderFile, message));
      }
    }

    /// [Copied from swiftless tutorials: http://www.swiftless.com/]
    ///Given a shader program, validateProgram will request from OpenGL, any information
	  ///related to the validation or linking of the program with it's attached shaders. It will
	  ///then output any issues that have occurred.
    private static void ValidateProgram(int handle)
    {
      string message;
      GL.GetProgramInfoLog(handle, out message);
      if (string.IsNullOrEmpty(message) == false)
      {
        // TODO: proper error handling
        System.Console.WriteLine(string.Format("Message linking shader:\n{0}", message));
      }

      GL.ValidateProgram(handle);
      int status;
      GL.GetProgram(handle, ProgramParameter.ValidateStatus, out status);
      if (status == 0)
      {
        System.Console.WriteLine("Error validating shader");
      }
    }

  }
}

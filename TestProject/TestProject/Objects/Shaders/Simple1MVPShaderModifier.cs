using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects.Shaders
{
  public class Simple1MVPShaderModifier : ShaderModifier
  {
    public override string Name
    {
      get { return "Simple1MVP"; }
    }

    public override ShaderProgram DeriveShaders(ShaderProgram sp)
    {
      ShaderProgram ret;
      if (ShaderProgram.Cache.TryGet(sp.Name + this.Name, out ret))
      {
        return ret;
      }

      Shader vertShader = ModifyShader(sp.VertexShader);

    }

    private Shader ModifyShader(Shader s)
    {
      string newName = string.Format("{0}_{1}", s.Name, this.Name);
      Shader ret;
      if (Shader.Cache.TryGet(newName, out ret))
      {
        return ret;
      }

      ret = new Shader(newName, ShaderType.VertexShader);
    }

    public override void Compile()
    {
      if (this.IsCompiled)
      {
        return;
      }

      StringBuilder parameterList = new StringBuilder();
      foreach (Parameter p in Parameters.Values)
      {
        parameterList.AppendLine(p.ToString());
      }

      string source = string.Format(@"
// version declaration?

{0}

void shaderMain(void)
{
{1}
}

void main(void)
{
  shaderMain();
  gl_Position = mvpMatrix * gl_Position;
}",
  parameterList,
  this.MainBodyCode);

      Handle = GL.CreateShader(this.Type);
      GL.ShaderSource(Handle, source);
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
  }
}

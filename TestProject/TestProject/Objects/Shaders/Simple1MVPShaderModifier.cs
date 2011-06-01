using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace TestProject.Objects.Shaders
{
  public class Simple1MVPShaderModifier : ProgramModifier
  {
    public override string Name
    {
      get { return "Simple1MVP"; }
    }

    public override ShaderProgram DeriveProgram(ShaderProgram sp)
    {
      string newName = string.Format("{0}_{1}", sp.Name, this.Name);
      ShaderProgram ret;
      if (ShaderProgram.Cache.TryGet(newName, out ret))
      {
        return ret;
      }

      Shader vertShader = ModifyShader(sp.VertexShader);
      ret = new ShaderProgram(newName, vertShader, sp.GeometryShader, sp.FragmentShader);

      ShaderProgram.Cache[ret.Name] = ret;

      return ret;
    }

    private Shader ModifyShader(Shader s)
    {
      string newName = string.Format("{0}_{1}", s.Name, this.Name);
      Shader ret;
      if (Shader.Cache.TryGet(newName, out ret))
      {
        return ret;
      }

      ret = new Shader(
        newName,
        ShaderType.VertexShader,
        new Dictionary<string, Shader.Parameter>(s.Parameters),
        new Dictionary<string, Shader.Function>(s.Functions),
        "simple1MVP_main");
      
      // add parameter
      ret.Parameters.Add("in_mvpMatrix", new Shader.Parameter(
        Shader.Parameter.InterpolationQualifier.None,
        Shader.Parameter.StorageQualifier.Uniform,
        Shader.Parameter.GLSLType.Mat4,
        "in_mvpMatrix"));

      // add function
      ret.Functions.Add("simple1MVP_main", new Shader.Function(
        new Shader.Parameter(
          Shader.Parameter.InterpolationQualifier.None,
          Shader.Parameter.StorageQualifier.None,
          Shader.Parameter.GLSLType.Void,
          string.Empty),
        "simple1MVP_main",
        new Shader.Parameter[0],
        string.Format(@"{0}();
gl_Position = in_mvpMatrix * gl_Position;", s.EntryFunction)));

      Shader.Cache[ret.Name] = ret;

      return ret;
    }
  }
}

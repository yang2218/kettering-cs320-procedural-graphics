using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestProject.Objects.Materials
{
  public class FontMaterial : Material
  {
    public FontMaterial()
    {
      Program = ShaderProgram.Cache["FontShader"];
    }


  }
}

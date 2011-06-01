using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestProject.Objects
{
  public abstract class ProgramModifier
  {
    public abstract string Name { get; }

    public abstract ShaderProgram DeriveProgram(ShaderProgram sp);
  }
}

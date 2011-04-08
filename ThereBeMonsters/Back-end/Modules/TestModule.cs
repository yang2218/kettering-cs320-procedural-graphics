using System;
using System.Collections.Generic;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Back_end.Modules
{
  public class TestModule : Module
  {
    [Parameter("Operand 1")]
    public int A { private get; set; }
    //[Parameter("Operand 2", optional: true, type: Parameter.IOType.INOUT )]
    public int B { private get; set; }
    //[Parameter("Answer", type: Parameter.IOType.OUTPUT)]
    public int C { get; private set; }

    public override void Run()
    {
      C = A + B;
    }
  }
}

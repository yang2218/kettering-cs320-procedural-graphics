using System;
using System.Collections.Generic;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Back_end.Modules
{
  [ModuleAttribute("Test description")] // Adds a description to this module's metadata. Optional attribute.
  public class ExampleModule : Module
  {
    [Parameter("Operand 1")] // adds a description to this parameter's metadata
    public int A { private get; set; }

    // Setting some of the optional arguments to the Parameter:
    // This overrides the I/O direction of the parameter, and sets an "optional" flag (just some metadata for now)
    [Parameter("Operand 2", optional: true, direction: Parameter.IODirection.INPUT)]
    public int B { get; set; } // without the direction override, this parameter would be detected as INOUT (bidirectional)

    // The Parameter attribute itself is optional
    //[Parameter("Answer")]
    public int C { get; private set; }

    [NonParameter] // hides this property as a Module Parameter
    public string MyProperty { get { return "Some property we don't care about"; } }

    /// <summary>
    /// Calculates A + B, stores the result in C
    /// </summary>
    public override void Run()
    {
      C = A + B;
    }
  }
}

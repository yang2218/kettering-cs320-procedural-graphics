using System;
using System.Collections.Generic;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Back_end.Modules
{
  // This attribute is optional and just for adding metadata
  [Module("Test description",  // Adds a description to this module's metadata
      Reusable = true)] // Marks reusable (the default). Set this to false if you want this module instantiated for each time it appears in the module graph, or leave it out.
  public class ExampleModule : Module
  {
    [Parameter("Operand 1")] // adds a description to this parameter's metadata
    public int A { private get; set; }

    [Parameter("Operand 2",                       // we can also set some of the properties on the Parameter as well:
        Optional = true,                          // sets an "optional" flag
        Direction = Parameter.IODirection.INPUT)] // overrides the I/O direction of the parameter
    public int B { get; set; } // without the direction override, this parameter would be detected as INOUT (bi-directional)

    // The Parameter attribute itself is optional
    //[Parameter("Answer")]
    public int C { get; private set; }

    [NonParameter] // prevents this property from becoming a module parameter
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

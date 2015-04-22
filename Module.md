# Writing a Module #

All procedural content generator modules should inherit from the `ThereBeMonsters.Back_end.Module` abstract class. Module parameters are simply defined by public properties, and the actual generation code is put in the Run method.

`Parameter` attributes may optionally be used to add a description or override some configuration related to the parameter.

_**I may be tweaking the Module class still; see the `ExampleModule` class in the repository for the most complete/up-to-date example of a module.**_

Example module:
```
using System;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Back_end.Modules
{
  public class TestModule : Module
  {
    // This is still a parameter, even without the (optional) Parameter attribute
    public int A { private get; set; }

    [Parameter("This is a an input parameter with a description.")]    
    public int B { private get; set; }

    [Parameter("An optional (and in this case redundant) parameter to Parameter"
        + " specifying the input/output disposition of this property.", Direction = Parameter.IODirection.OUTPUT)]
    public int C { get; private set; }

    public override void Run()
    {
      C = A + B;
    }
  }
}
```

# Working with Modules #

Module parameters may be conveniently get or set by using the indexer:
```
Module myModule = new TestModule();
myModule["A"] = 2;
myModule["B"] = 3;
myModule.Run();
System.Console.WriteLine(myModule["C"]);
```

The additional metadata (such as the description) can be accessed through the `Parameters` property. `Parameters` is a `Dictionary<string, Parameter>`.
```
foreach(string param in myModule.Parameters.Keys) // names of all parameters
{ 
  System.Console.WriteLine(string.Format(@"
Parameter: {0}
Description: {1}
Input/Output Direction: {2}",
    param,
    myModule.Parameters[param].Description,
    myModule.Parameters[param].Direction));
}
```
using System;
using ThereBeMonsters.Back_end;
using ThereBeMonsters.Back_end.Modules;

namespace ThereBeMonsters
{
  public class ModuleTester
  {
    public static void Main()
    {
      Module m = new ExampleModule();

      System.Console.WriteLine(m.Description);

      m["A"] = 2;
      m["B"] = 2;

      m.Run();

      System.Console.WriteLine(m["C"]);
      
      foreach(string param in m.Parameters.Keys) // names of all parameters
      { 
        System.Console.WriteLine(string.Format(@"
Parameter: {0}
ParamType: {1}
Description: {2}
Input/Output Dir: {3}
Optional: {4}",
          param,
          m.Parameters[param].Type,
          m.Parameters[param].Description,
          m.Parameters[param].Direction,
          m.Parameters[param].Optional));
      }

      System.Console.ReadLine();
    }
  }
}

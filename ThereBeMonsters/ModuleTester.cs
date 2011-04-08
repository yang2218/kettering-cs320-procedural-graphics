using System;
using ThereBeMonsters.Back_end;
using ThereBeMonsters.Back_end.Modules;

namespace ThereBeMonsters
{
  public class ModuleTester
  {
    public static void Main()
    {
      Module m = new TestModule();
      /*
      int i = 1;
      foreach (Module.Parameter p in m.Parameters.Values)
      {
        //System.Console.WriteLine(p);
        if (p.Type == Module.Parameter.IOType.INPUT)
        {
          p.Property.SetValue(m, i++, null);
        }
      }
      */

      m["A"] = 2;
      m["B"] = 2;

      m.Run();

      System.Console.WriteLine(m["C"]);

      /*
      foreach (Module.Parameter p in m.Parameters.Values)
      {
        if (p.Type == Module.Parameter.IOType.OUTPUT)
        {
          System.Console.WriteLine(p.Property.GetValue(m, null));
        }
      }
      */

      System.Console.ReadLine();
    }
  }
}

using System;
using System.Collections.Generic;
using ThereBeMonsters.Back_end;
using ThereBeMonsters.Back_end.Modules;
using OpenTK;

using System.ComponentModel;

namespace ThereBeMonsters
{
  public class ModuleTester
  {
    public static void Main()
    {

      Gasket cookie = new Gasket();
      cookie.Run();
      /*Generator g = new Generator("TestGraph.xml");
      g.RunGraph();
      */

      /*byte[,] hm = new byte[32, 32];
      List<Vector3> c = new List<Vector3>() {
        new Vector3(0.5f, 0.5f, 0.4f)};

      ExtrudeCirclesToHeight m = new ExtrudeCirclesToHeight();
      m.Cap = ExtrudeCirclesToHeight.CapType.Hemisphere;
      m.Circles = c;
      m.HeightMap = hm;
      m.BlendFunc = Blend8bppFunctions.Additive;
      m.BlendFuncSrcFactor = 1f;
      m.BlendFuncDstFactor = 1f;

      m.Run();

      hm = m.HeightMap;
      int i = 0;
      foreach (byte b in hm)
      {
        if (i++ % 32 == 0)
        {
          Console.WriteLine();
        }
        
        Console.Write(string.Format("{0,2}", b / 10));
      }

      System.Console.ReadLine();*/
    }

    /*
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
    */
  }
}

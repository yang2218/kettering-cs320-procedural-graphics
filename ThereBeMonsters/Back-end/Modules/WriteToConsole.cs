using System;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Back_end.Modules
{
  [Module("Takes an input and prints it to the console.")]
  public class WriteToConsole : Module
  {
    [Parameter("Object to print")]
    public object Input1 { private get; set; }

    public override void Run()
    {
      System.Console.WriteLine(Input1);
    }
  }
}

using System;

namespace ThereBeMonsters.Back_end.Modules
{
  public class InitMapsModule : Module
  {
    [Parameter("Resolution of the terrain (# of verticies on one side)",
      Default = 512)]
    public int Size { private get; set; }
    public byte[,] HeightMap { get; private set; }
    public uint[,] ColorMap { get; private set; }

    public override void Run()
    {
      HeightMap = new byte[Size, Size];
      ColorMap = new uint[Size, Size];
    }
  }
}

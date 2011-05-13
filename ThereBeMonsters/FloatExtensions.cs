using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThereBeMonsters.Front_end
{
  public static class FloatExtensions
  {
    public static float Clamp(this float me, float min, float max)
    {
      return Math.Min(Math.Max(me, min), max);
    }
  }
}

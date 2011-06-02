using System.Collections.Generic;
using System.Drawing;
using OpenTK;

namespace ThereBeMonsters.Back_end.Modules
{
  public class ColorCircle : Module
  {
    public IEnumerable<Vector3> Circles { private get; set; }

    [Parameter(@"How the calculated colormap will be blended with the input colormap.
(Input colormap will be the Source, generated colormap will be Destination)",
      Editor = typeof(Blend32bppDelegateEditor))]
    public Blend32bppDelegate BlendFunc { private get; set; }

    [Parameter(Hidden = true)]
    public float BlendFuncSrcFactor { private get; set; }
    [Parameter(Hidden = true)]
    public float BlendFuncDstFactor { private get; set; }

    public Color? Color { private get; set; }

    private uint[,] inputColorMap, outputColorMap;

    public uint[,] ColorMap
    {
      get { return outputColorMap; }
      set { inputColorMap = value; }
    }

    public override void Run()
    {
      int res = inputColorMap.GetLength(0);
      outputColorMap = new uint[res, res];
      int centerX, centerY, circleRadius, radiusSq, distSqFromCenter;
      int r, l, t, b;
      uint colorValue = 0;
      uint redValue, greenValue, blueValue;

      foreach (Vector3 circle in Circles)
      {
        centerX = (int)(circle.X * res);
        centerY = (int)(circle.Y * res);
        circleRadius = (int)(circle.Z * res);
        radiusSq = circleRadius * circleRadius;

        if (this.Color.HasValue)
        {
          redValue = this.Color.Value.R;
          greenValue = this.Color.Value.G;
          blueValue = this.Color.Value.B;
        }
        else
        {
          redValue = (uint)Module.rng.Next(255);
          greenValue = (uint)Module.rng.Next(255);
          blueValue = (uint)Module.rng.Next(255);
        }

        colorValue = 0xFF000000 | blueValue << 16 | greenValue << 8 | redValue;

        for (int i = 0; i <= circleRadius; i++)
        {
          for (int j = 0; j <= circleRadius; j++)
          {
            distSqFromCenter = i * i + j * j;
            if (distSqFromCenter > radiusSq)
            {
              continue;
            }

            r = Clamp(centerX + i, 0, res - 1);
            l = Clamp(centerX - i, 0, res - 1);
            t = Clamp(centerY + j, 0, res - 1);
            b = Clamp(centerY - j, 0, res - 1);

            outputColorMap[r, t] = colorValue;
            outputColorMap[l, t] = colorValue;
            outputColorMap[r, b] = colorValue;
            outputColorMap[l, b] = colorValue;
          }
        }
      }
    }

    private int Clamp(int v, int min, int max)
    {
      return v < min ? min : v > max ? max : v;
    }
  }
}


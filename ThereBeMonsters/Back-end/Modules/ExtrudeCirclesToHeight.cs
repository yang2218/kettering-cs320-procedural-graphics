using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using OpenTK;

namespace ThereBeMonsters.Back_end.Modules
{
  [Module("Rasterizes circles onto a heightmap")]
  public class ExtrudeCirclesToHeight : Module
  {
    public enum Cap
    {
      Flat,
      Cone,
      Hemisphere
    }

    public enum Scale
    {
      None,
      Linear,
      Quadradic
    }

    private byte[,] inputHeightMap, outputHeightMap;

    public IEnumerable<Vector3> Circles { private get; set; }

    public Cap CapMode { private get; set; }

    // TODO: this can be generalized to an arbitrary second-degree polynomial...
    // but, probably no reason to do the extra work
    public Scale ScaleMode { private get; set; }

    [Parameter(Default = true)]
    public bool ScaleByRadius { private get; set; }

    public byte[,] HeightMap
    {
      get { return outputHeightMap; }
      set { inputHeightMap = value; }
    }

    [Parameter(@"How the calculated heightmap will be blended with the input heightmap.
(Input heightmap will be the Source, generated heightmap will be Destination)",
      Editor = typeof(Blend8bppDelegateEditor))]
    public Blend8bppDelegate BlendFunc { private get; set; }

    [Parameter(Hidden = true)]
    public float BlendFuncSrcFactor { private get; set; }
    [Parameter(Hidden = true)]
    public float BlendFuncDstFactor { private get; set; }

    private static byte[] lookupTable = {};

    public override void Run()
    {
      int res = inputHeightMap.GetLength(0);
      outputHeightMap = new byte[res, res];
      int centerX, centerY, circleRadius, radiusSq, distSqFromCenter;
      int r, l, t, b;
      byte v = 0;
      float heightScale = 1f;

      // generate lookup table
      int tableSize = (int)Math.Sqrt(res * res * 2);
      if (lookupTable.Length != tableSize)
      {
        lookupTable = new byte[tableSize];
        for (int i = 0; i < lookupTable.Length; i++)
        {
          switch (CapMode)
          {
            case Cap.Flat:
              lookupTable[i] = byte.MaxValue;
              break;
            case Cap.Cone:
              lookupTable[i] = (byte)(byte.MaxValue * (1f - (float)i / tableSize));
              break;
            case Cap.Hemisphere:
              lookupTable[i] = (byte)(byte.MaxValue * Math.Sin(Math.Acos((float)i / tableSize)));
              break;
          }
        }
      }
      
      foreach (Vector3 circle in Circles)
      {
        centerX = (int)(circle.X * res);
        centerY = (int)(circle.Y * res);
        circleRadius = (int)(circle.Z * res);
        radiusSq = circleRadius * circleRadius;
        switch (ScaleMode)
        {
          case Scale.Linear:
            heightScale = circle.Z * 2f;
            break;
          case Scale.Quadradic:
            heightScale = 1f - (float)Math.Pow(1f - circle.Z * 2f, 2.0);
            break;
        }

        for (int i = 0; i <= circleRadius; i++)
        {
          for (int j = 0; j <= circleRadius; j++)
          {
            distSqFromCenter = i * i + j * j;
            if (distSqFromCenter > radiusSq)
            {
              continue;
            }

            if (distSqFromCenter == 0)
            {
              v = lookupTable[0];
            }
            else
            {
              v = lookupTable[(int)(tableSize
                * MathHelper.InverseSqrtFast((float)radiusSq / distSqFromCenter))];
            }

            v = (byte)(v * heightScale);

            r = Clamp(centerX + i, 0, res - 1);
            l = Clamp(centerX - i, 0, res - 1);
            t = Clamp(centerY + j, 0, res - 1);
            b = Clamp(centerY - j, 0, res - 1);

            outputHeightMap[r, t] = v;
            outputHeightMap[l, t] = v;
            outputHeightMap[r, b] = v;
            outputHeightMap[l, b] = v;
          }
        }
      }

      BlendFunc(inputHeightMap, outputHeightMap, BlendFuncSrcFactor, BlendFuncDstFactor);
    }

    private int Clamp(int v, int min, int max)
    {
      return v < min ? min : v > max ? max : v;
    }
  }
}

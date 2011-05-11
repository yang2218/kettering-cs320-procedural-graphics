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
    public enum CapType
    {
      Flat,
      Cone,
      Hemisphere
    }

    private byte[,] inputHeightMap, outputHeightMap;

    public IList<Vector3> Circles { private get; set; }

    public CapType Cap { private get; set; }

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

    public override void Run()
    {
      int res = inputHeightMap.GetLength(0);
      outputHeightMap = new byte[res, res];
      int cx, cy, cr, r2, d2;
      float rr, r2r;
      byte v = 0;
      foreach (Vector3 circle in Circles)
      {
        rr = circle.X * res;
        r2r = circle.Z * 2f;
        cx = (int)rr;
        cy = (int)(circle.Y * res);
        cr = (int)(circle.Z * res);
        r2 = cr * cr;
        for (int i = 0; i <= cr; i++)
        {
          for (int j = 0; j <= cr; j++)
          {
            d2 = i * i + j * j;
            if (d2 > r2)
            {
              continue;
            }

            switch (Cap)
            { // TODO: precomute lookup tables for sqrt and sin(acos())
              case CapType.Flat:
                v = (byte)(255 * r2r);
                break;
              case CapType.Cone:
                v = (byte)(255 * r2r * (1f - Math.Sqrt(d2) / rr));
                break;
              case CapType.Hemisphere:
                v = (byte)(255 * r2r * Math.Sin(Math.Acos(Math.Sqrt(d2) / rr)));
                break;
            }

            outputHeightMap[cx + i, cy + j] = v;
            outputHeightMap[cx - i, cy + j] = v;
            outputHeightMap[cx + i, cy - j] = v;
            outputHeightMap[cx - i, cy - j] = v;
          }
        }
      }

      BlendFunc(inputHeightMap, outputHeightMap, BlendFuncSrcFactor, BlendFuncDstFactor);
    }
  }
}

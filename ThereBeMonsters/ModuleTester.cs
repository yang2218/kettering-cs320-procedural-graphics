﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using ThereBeMonsters.Back_end;
using ThereBeMonsters.Back_end.Modules;
using OpenTK;
using System.Drawing;

namespace ThereBeMonsters
{
  public class ModuleTester
  {
    private static IEnumerable<Vector3> CirlceFilter(IEnumerable<Vector3> input)
    {
      foreach (Vector3 v in input)
      {
        if (v.Z > 0f && v.Z < 1f)
        {
          yield return v;
        }
      }
    }

    public static void Main()
    {
      Gasket g = new Gasket();
      g.InitialShapePoints = new List<Vector2> {
        new Vector2(0f, 0f),
        new Vector2(1f, 0f),
        new Vector2(1f, 1f),
        new Vector2(0f, 1f),
        new Vector2(0f, 0f)
      };
      g.MaxDepth = 8;
      g.Run();

      ExtrudeCirclesToHeight e = new ExtrudeCirclesToHeight();
      e.CapMode = ExtrudeCirclesToHeight.Cap.Hemisphere;
      e.Circles = CirlceFilter(g.Circles);
      e.HeightMap = new byte[64, 64];
      e.ScaleMode = ExtrudeCirclesToHeight.Scale.Quadradic;
      e.BlendFunc = Blend8bppFunc.Additive;
      e.BlendFuncSrcFactor = 1f;
      e.BlendFuncDstFactor = 1f;
      e.Run();

      TexturePreview p = new TexturePreview();
      p.HeightMap = e.HeightMap;
      //preview.ColorMap = painter.ColorMap;
      p.Run();

      //return;

      InitShape shape = new InitShape();
      shape.Run();

      Gasket cookie = new Gasket();
      cookie.InitialShapePoints = shape.Shape;
      /*new Vector2(0f, 0f), 
        new Vector2(0f, 1f),
        new Vector2(1f, 1f),
        new Vector2(1f, 0f),
        new Vector2(0f, 0f)
        /*
        10f * new Vector2(2.00f, 1.00f),
        10f * new Vector2(1.50f, 1.87f),
        10f * new Vector2(0.5f, 1.87f),
        10f * new Vector2(0f, 1.00f),
        10f * new Vector2(0.5f, 0.13f),
        10f * new Vector2(1.50f, 0.13f),
        10f * new Vector2(2.00f, 1.00f)
      };*/
      cookie.MaxDepth = 7;

      Stopwatch sw = new Stopwatch();
      sw.Start();
      cookie.Run();
      sw.Stop();
      Console.WriteLine(string.Format("Gasket generator run time: {0}ms", sw.ElapsedMilliseconds));
      /*
      ColorCircle painter = new ColorCircle();
      painter.Circles = CirlceFilter(cookie.Circles);
      painter.ColorMap = new uint[512, 512];
      painter.BlendFunc = Blend32bppFunc.Additive;
      painter.BlendFuncSrcFactor = 1f;
      painter.BlendFuncDstFactor = 1f;
      painter.Color = Color.Pink;
      painter.Run();
      */
      
      ExtrudeCirclesToHeight extruder = new ExtrudeCirclesToHeight();
      extruder.CapMode = ExtrudeCirclesToHeight.Cap.Hemisphere;
      extruder.Circles = CirlceFilter(cookie.Circles);
      extruder.HeightMap = new byte[1024, 1024];
      extruder.ScaleMode = ExtrudeCirclesToHeight.Scale.None;
      extruder.BlendFunc = Blend8bppFunc.Additive;
      extruder.BlendFuncSrcFactor = 1f;
      extruder.BlendFuncDstFactor = 1f;
        
      sw.Reset();
      sw.Start();
      extruder.Run();
      sw.Stop();
      Console.WriteLine(string.Format("Extruder run time: {0}ms", sw.ElapsedMilliseconds));
      
      TexturePreview preview = new TexturePreview();
      preview.HeightMap = extruder.HeightMap;
      //preview.ColorMap = painter.ColorMap;
      preview.Run();
    }
  }
}

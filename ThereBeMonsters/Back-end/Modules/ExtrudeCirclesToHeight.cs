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

    // Either assume the first circle in the list is the biggest,
    // or add some parameters to specify how brightness should scale with radius
    public IList<Vector3> Circles { private get; set; }

    public CapType Cap { private get; set; }

    public byte[,] HeightMap { get; set; }

    [Parameter(Editor = typeof(BlendDelegateEditor))]
    public Blend8bppDelegate BlendFunc { private get; set; }

    [Parameter(Hidden = true)]
    public float BlendFuncSrcFactor { private get; set; }
    [Parameter(Hidden = true)]
    public float BlendFuncDstFactor { private get; set; }

    public override void Run()
    {
      // TODO
    }
  }
}

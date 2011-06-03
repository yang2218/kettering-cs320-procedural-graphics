using System;
using System.Collections.Generic;
using System.Diagnostics;
using ThereBeMonsters.Front_end.Controls;
using OpenTKGUI;
using ThereBeMonsters.Front_end;

namespace ThereBeMonsters.Back_end
{
  public delegate void Blend8bppDelegate(byte[,] src, byte[,] dst, float srcFactor = 1f, float dstFactor = 1f);
  public delegate void Blend32bppDelegate(uint[,] src, uint[,] dst, float srcFactor = 1f, float dstFactor = 1f);
  
  public enum Blend8bppFunc
  {
    DestinationOnly,
    Additive
  }

  public enum Blend32bppFunc
  {
    DestinationOnly,
    Additive
  }

  public static class Blend8bppFunctions
  {
    public static Blend8bppDelegate GetFunc(Blend8bppFunc f)
    {
      switch (f)
      {
        case Blend8bppFunc.DestinationOnly:
          return DestinationOnly;
        case Blend8bppFunc.Additive:
          return Additive;
        default:
          throw new ArgumentException("Function not found.");
      }
    }

    public static void Additive(byte[,] src, byte[,] dst, float srcFactor = 1f, float dstFactor = 1f)
    {
      Debug.Assert(src.Length == dst.Length, "Source and Destination arrays are not the same size!");
      int result;
      unchecked
      {
        if (srcFactor == 1f && dstFactor == 1f)
        {
          for (int i = 0; i < src.GetLength(0); i++)
          {
            for (int j = 0; j < src.GetLength(1); j++)
            {
              result = dst[i, j] + src[i, j];
              dst[i, j] = result > 255 ? (byte)255 : (byte)result;
            }
          }
        }
        else
        {
          for (int i = 0; i < src.GetLength(0); i++)
          {
            for (int j = 0; j < src.GetLength(1); j++)
            {
              result = (int)(dst[i, j] * dstFactor) + (int)(src[i, j] * srcFactor);
              dst[i, j] = result < 0 ? (byte)0 : (result > 255 ? (byte)255 : (byte)result);
            }
          }
        }
      }
    }

    public static void DestinationOnly(byte[,] src, byte[,] dst, float srcFactor = 1f, float dstFactor = 1f)
    {
      return; // Destination is already in the destination
      // TODO: if dstFactor != 1f multiply destination pixels
    }
  }

  public static class Blend32bppFunctions
  {
    public static Blend32bppDelegate GetFunc(Blend32bppFunc f)
    {
      switch (f)
      {
        case Blend32bppFunc.DestinationOnly:
          return DestinationOnly;
        case Blend32bppFunc.Additive:
          return Additive;
        default:
          throw new ArgumentException("Function not found.");
      }
    }

    public static void Additive(uint[,] src, uint[,] dst, float srcFactor = 1f, float dstFactor = 1f)
    {
      Debug.Assert(src.Length == dst.Length, "Source and Destination arrays are not the same size!");
      uint result, comp;
      unchecked // disables overflow checks
      {
        if (srcFactor == 1f && dstFactor == 1f)
        {
          for (int i = 0; i < src.GetLength(0); i++)
          {
            for (int j = 0; j < src.GetLength(1); j++)
            {
              result = 0;
              comp = (dst[i, j] & 0xff) + (src[i, j] & 0xff);
              result |= comp > 0xff ? 0xff : comp;
              comp = (dst[i, j] & 0xff00) + (src[i, j] & 0xff00);
              result |= comp > 0xff00 ? 0xff00 : comp;
              comp = (dst[i, j] & 0xff0000) + (src[i, j] & 0xff0000);
              result |= comp > 0xff0000 ? 0xff0000 : comp;
              checked
              {
                try
                {
                  comp = (dst[i, j] & 0xff000000) + (src[i, j] & 0xff000000);
                }
                catch (OverflowException)
                {
                  comp = 0xff000000;
                }
              }
              result |= comp;
              dst[i, j] = result;
            }
          }
        }
        else
        {
          for (int i = 0; i < src.GetLength(0); i++)
          {
            for (int j = 0; j < src.GetLength(1); j++)
            {
              // TODO: check for "underflow" ("negative" results caused by negative factors)
              result = 0;
              comp = ((uint)(dst[i, j] * dstFactor) & 0xff) + ((uint)(src[i, j] * srcFactor) & 0xff);
              result |= comp > 0xff ? 0xff : comp;
              comp = ((uint)(dst[i, j] * dstFactor) & 0xff00) + ((uint)(src[i, j] * srcFactor) & 0xff00);
              result |= comp > 0xff00 ? 0xff00 : comp;
              comp = ((uint)(dst[i, j] * dstFactor) & 0xff0000) + ((uint)(src[i, j] * srcFactor) & 0xff0000);
              result |= comp > 0xff0000 ? 0xff0000 : comp;
              checked
              {
                try
                {
                  comp = (dst[i, j] & 0xff000000) + (src[i, j] & 0xff000000);
                }
                catch (OverflowException)
                {
                  comp = 0xff000000;
                }
              }
              result |= comp;
              dst[i, j] = result;
            }
          }
        }
      }
    }

    public static void DestinationOnly(uint[,] src, uint[,] dst, float srcFactor = 1f, float dstFactor = 1f)
    {
      return; // Destination is already in the destination
      // TODO: if dstFactor != 1f multiply destination pixels
    }
  }

  public class BlendFuncEditor : EditorControl
  {
    public override double PreferredHeight
    {
      get { return 40; }
    }

    public BlendFuncEditor(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      FlowContainer container = new FlowContainer(Axis.Vertical);

      container.AddChild(new EnumControl(parentNode, paramName), 20);
      FlowContainer horzFlow = new FlowContainer(Axis.Horizontal);

      // TODO: add labels?
      horzFlow.AddChild(new FloatControl(parentNode, paramName + "SrcFactor"), 75);
      horzFlow.AddChild(new FloatControl(parentNode, paramName + "DstFactor"), 75);
      container.AddChild(horzFlow, 20);

      Client = container;
    }

    public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
    {
    }
  }
}

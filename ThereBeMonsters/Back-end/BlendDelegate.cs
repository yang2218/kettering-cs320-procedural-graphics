using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTKGUI;

namespace ThereBeMonsters.Back_end
{
  public delegate void Blend8bppDelegate(byte[,] src, byte[,] dst, float srcFactor = 1f, float dstFactor = 1f);

  public class BlendDelegateEditor : EditorControl
  {
    private Blend8bppDelegate _function;
    private float _srcFactor, _dstFactor;

    //private Listbox
    private Textbox _srcFactorField, _dstFactorField;

    public BlendDelegateEditor(string paramName)
      : base(paramName)
    {
      // TODO: add controls:
      // a select list of functions from Blend8bppFunctions.List
      // collaspable srcFactor and dstFactor fields as well

      // TODO: listbox event

      _srcFactorField.TextEntered += (string text) =>
        {
          // TODO: validation
          SetModuleParameterValue(parameterName + "SrcFactor", float.Parse(text));
        };

      _dstFactorField.TextEntered += (string text) =>
      {
        SetModuleParameterValue(parameterName + "DstFactor", float.Parse(text));
      };
    }

    public override void Render(GUIRenderContext Context)
    {
      base.Render(Context);
    }

    public override void Update(GUIControlContext Context, double Time)
    {
      base.Update(Context, Time);
    }
  }

  public static class Blend8bppFunctions
  {
    private static List<Blend8bppDelegate> _functions;
    public static List<Blend8bppDelegate> List
    {
      get
      {
        if (_functions == null)
        {
          _functions = new List<Blend8bppDelegate>()
          { // Add functions here
            Additive,
            DestinationOnly
          };
        }

        return _functions;
      }
    }

    public static void Additive(byte[,] src, byte[,] dst, float srcFactor = 1f, float dstFactor = 1f)
    {
      Debug.Assert(src.Length == dst.Length, "Source and Destination arrays are not the same size!");
      int result;
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

    public static void DestinationOnly(byte[,] src, byte[,] dst, float srcFactor = 1f, float dstFactor = 1f)
    {
      return; // Destination is already in the destination
      // TODO: if dstFactor != 1f multiply destination pixels
    }
  }
}

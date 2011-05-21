using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTKGUI;
using ThereBeMonsters.Front_end;

namespace ThereBeMonsters.Back_end
{
  public delegate void Blend8bppDelegate(byte[,] src, byte[,] dst, float srcFactor = 1f, float dstFactor = 1f);
  public delegate void Blend32bppDelegate(uint[,] src, uint[,] dst, float srcFactor = 1f, float dstFactor = 1f);

  public abstract class BlendDelegateEditor : EditorControl
  {
    protected float _srcFactor, _dstFactor;

    //private Listbox
    private Textbox _srcFactorField, _dstFactorField;

    public override double PreferredHeight
    {
      get { return 30; }
    }

    public BlendDelegateEditor(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      // TODO: add controls:
      // a select list of functions from Blend8bppFunctions.List
      // collaspable srcFactor and dstFactor fields as well

      // TODO: listbox event

      _srcFactorField = new Textbox();
      _dstFactorField = new Textbox();

      // TODO: change parameter updating to after focus is lost?
      _srcFactorField.TextEntered += (string text) =>
      {
        // TODO: validation
        SetModuleParameterValue(ParameterName + "SrcFactor", float.Parse(text));
      };

      _dstFactorField.TextEntered += (string text) =>
      {
        SetModuleParameterValue(ParameterName + "DstFactor", float.Parse(text));
      };
    }
    
    public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
    {
      throw new NotImplementedException();
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
  
  public class Blend8bppDelegateEditor : BlendDelegateEditor
  {
    private Blend8bppDelegate _function;

    public Blend8bppDelegateEditor(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      _function = Blend8bppFunctions.Default;
    }
  }

  public class Blend32bppDelegateEditor : BlendDelegateEditor
  {
    private Blend32bppDelegate _function;

    public Blend32bppDelegateEditor(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      _function = Blend32bppFunctions.Default;
    }
  }

  public static class Blend8bppFunctions
  {
    private static List<Blend8bppDelegate> _functions;
    public static Blend8bppDelegate Default { get { return DestinationOnly; } }
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
    private static List<Blend32bppDelegate> _functions;
    public static Blend32bppDelegate Default { get { return DestinationOnly; } }
    public static List<Blend32bppDelegate> List
    {
      get
      {
        if (_functions == null)
        {
          _functions = new List<Blend32bppDelegate>()
          { // Add functions here
            Additive,
            DestinationOnly
          };
        }

        return _functions;
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

}

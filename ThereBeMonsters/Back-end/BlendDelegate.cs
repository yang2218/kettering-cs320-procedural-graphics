﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTKGUI;
using ThereBeMonsters.Front_end;
using ThereBeMonsters.Front_end.Controls;

namespace ThereBeMonsters.Back_end
{
  public delegate void Blend8bppDelegate(byte[,] src, byte[,] dst, float srcFactor = 1f, float dstFactor = 1f);
  public delegate void Blend32bppDelegate(uint[,] src, uint[,] dst, float srcFactor = 1f, float dstFactor = 1f);

  public abstract class BlendDelegateEditor : EditorControl
  {
    protected Textbox _functionListbox;
    protected Textbox _srcFactorField, _dstFactorField;

    public override double PreferredHeight
    {
      get { return 100; }
    }

    public BlendDelegateEditor(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      FlowContainer container = new FlowContainer(Axis.Vertical);

      container.AddChild(CreateFunctionPicker(), 30);

      // TODO: replace these fields with EditorControl for floats
      // TODO: make the two fields on the same row
      // TODO: if time, make these collaspable
      _srcFactorField = new Textbox();
      _dstFactorField = new Textbox();

      _srcFactorField.TextEntered += (string text) =>
      {
        SetModuleParameterValue(ParameterName + "SrcFactor", float.Parse(text));
      };

      _dstFactorField.TextEntered += (string text) =>
      {
        SetModuleParameterValue(ParameterName + "DstFactor", float.Parse(text));
      };

      container.AddChild(_srcFactorField, 30);
      container.AddChild(_dstFactorField, 30);
      Client = container;
    }

    protected abstract Control CreateFunctionPicker();
  }
  
  public class Blend8bppDelegateEditor : BlendDelegateEditor
  {
    public Blend8bppDelegateEditor(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
    }

    protected override Control CreateFunctionPicker()
    {
      _functionListbox = new Textbox();
      _functionListbox.Text = ((Blend8bppDelegate)ModuleParameterValue).Method.Name;
      PopupContainer pc = new PopupContainer(_functionListbox);
      // TODO: show on left click as well
      pc.ShowOnRightClick = true;
      List<MenuItem> options = new List<MenuItem>();
      foreach (Blend8bppDelegate func in Blend8bppFunctions.List)
      {
        options.Add(MenuItem.Create(
          func.Method.Name,
          () => { ModuleParameterValue = func; }));
      }

      pc.Items = options.ToArray();

      return pc;
    }

    public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
    {
      _functionListbox.Text = ((Blend8bppDelegate)ModuleParameterValue).Method.Name;
    }
  }

  public class Blend32bppDelegateEditor : BlendDelegateEditor
  {
    public Blend32bppDelegateEditor(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
    }

    protected override Control CreateFunctionPicker()
    {
      _functionListbox = new Textbox();
      _functionListbox.Text = ((Blend32bppDelegate)ModuleParameterValue).Method.Name;
      PopupContainer pc = new PopupContainer(_functionListbox);
      // TODO: show on left click as well
      pc.ShowOnRightClick = true;
      List<MenuItem> options = new List<MenuItem>();
      foreach (Blend32bppDelegate func in Blend32bppFunctions.List)
      {
        options.Add(MenuItem.Create(
          func.Method.Name,
          () => { ModuleParameterValue = func; }));
      }

      pc.Items = options.ToArray();

      return pc;
    }

    public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
    {
      _functionListbox.Text = ((Blend32bppDelegate)ModuleParameterValue).Method.Name;
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

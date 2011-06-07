using System;
using System.Collections.Generic;
using OpenTKGUI;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Front_end.Controls
{
  public class FloatControl : EditorControl
  {
    public override double PreferredHeight
    {
      get { return 20.0; }
    }

    private Textbox _textbox;

    public FloatControl(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      _textbox = new Textbox();
      _textbox.Text = (ModuleParameterValue ?? string.Empty).ToString();
      Client = _textbox;
      _textbox.TextEntered += (text) =>
      {
        float value;
        if (float.TryParse(text, out value))
        {
          ModuleParameterValue = value;
        }
      };
    }

    public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
    {
      float? val = ModuleParameterValue as float?;
      if (val.HasValue)
      {
        _textbox.Text = val.Value.ToString();
      }
      else
      {
        _textbox.Text = string.Empty;
      }
    }
  }
}

using System;
using System.Collections.Generic;
using OpenTKGUI;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Front_end.Controls
{
  public class IntControl : EditorControl
  {
    public override double PreferredHeight
    {
      get { return 20; }
    }

    private Textbox _textbox;

    public IntControl(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      _textbox = new Textbox();
      Client = _textbox;
      _textbox.Text = (ModuleParameterValue ?? string.Empty).ToString();
      _textbox.TextEntered += (text) =>
      {
        int value;
        if (int.TryParse(text, out value))
        {
          ModuleParameterValue = value;
        }
      };

    }

    public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
    {
      int? val = ModuleParameterValue as int?;
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

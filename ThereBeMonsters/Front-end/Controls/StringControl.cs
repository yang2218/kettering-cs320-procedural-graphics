using System;
using System.Collections.Generic;
using OpenTKGUI;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Front_end.Controls
{
  public class StringControl : EditorControl
  {
    public override double PreferredHeight
    {
      get { return 20.0; }
    }
    
    private Textbox _textbox;

    public StringControl(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      _textbox = new Textbox();
      _textbox.Text = (ModuleParameterValue ?? string.Empty).ToString();
      Client = _textbox;
      _textbox.TextEntered += (text) =>
      {
        ModuleParameterValue = text;
      };

    }

    public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
    {
      string val = ModuleParameterValue as string;
      if (val != null)
      {
        _textbox.Text = val;
      }
      else
      {
        _textbox.Text = string.Empty;
      }
    }
  }
}

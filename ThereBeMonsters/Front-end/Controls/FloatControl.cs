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

    public FloatControl(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      Textbox t = new Textbox();
      t.Text = (ModuleParameterValue ?? string.Empty).ToString();
      Client = t;
      t.TextEntered += (text) =>
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
    }
  }
}

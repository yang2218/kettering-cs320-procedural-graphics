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

    public IntControl(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      Textbox t = new Textbox();
      Client = t;
      t.Text = (ModuleParameterValue ?? string.Empty).ToString();
      t.TextEntered += (text) =>
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
    }
  }
}

using System;
using System.Collections.Generic;
using OpenTKGUI;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Front_end.Controls
{
  public class BoolEditor : EditorControl
  {
    public override double PreferredHeight
    {
      get { return 20; }
    }

    private Checkbox _checkbox;

    public BoolEditor(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      bool? val = ModuleParameterValue as bool?;
      _checkbox = new Checkbox();
      Client = _checkbox;
      OnValueChanged(null, null);
      _checkbox.Click += (check) =>
      {
        ModuleParameterValue = check;
      };

    }

    public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
    {
      bool? val = ModuleParameterValue as bool?;
      if (val.HasValue)
      {
        _checkbox.Text = this.ParameterName;
        _checkbox.Checked = val.Value;
      }
      else
      {
        _checkbox.Text = "(Null)";
      }
    }
  }
}

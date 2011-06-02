using System;
using System.Collections.Generic;
using OpenTKGUI;
using ThereBeMonsters.Back_end;
using System.Reflection;

namespace ThereBeMonsters.Front_end.Controls
{
  public class EnumControl : EditorControl
  {
    public override double PreferredHeight
    {
      get { return 20.0; }
    }

    public EnumControl(ModuleNodeControl parentNode, string paramName)
      : base(parentNode, paramName)
    {
      Textbox t = new Textbox();
      t.Text = (ModuleParameterValue ?? string.Empty).ToString();
      PopupContainer pc = new PopupContainer(t);
      List<MenuItem> options = new List<MenuItem>();
      Type type = ThereBeMonsters.Back_end.Module.GetModuleParameters(NodeControl.Node.ModuleType)[ParameterName].Type;
      foreach (MemberInfo mi in type.GetMembers(BindingFlags.Static | BindingFlags.Public))
      {
        FieldInfo fi = mi as FieldInfo;
        if (fi == null)
        {
          continue;
        }

        options.Add(MenuItem.Create(fi.Name, () =>
        {
          t.Text = fi.Name;
          ModuleParameterValue = fi.GetValue(null);
        }));
      }
      pc.Items = options.ToArray();
      Client = pc;
    }

    public override void OnValueChanged(object sender, ModuleParameterEventArgs e)
    {
    }
  }
}

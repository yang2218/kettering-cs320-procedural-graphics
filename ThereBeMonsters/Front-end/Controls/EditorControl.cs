using System;
using System.Collections.Generic;
using OpenTKGUI;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Front_end
{
  public abstract class EditorControl : Control
  {
    public ModuleNodeControl NodeControl { get; protected set; }
    public string ParameterName { get; protected set; }

    public abstract double PreferredHeight { get; }

    public EditorControl(ModuleNodeControl parentNode, string paramName)
    {
      this.ParameterName = paramName;
    }

    public virtual void OnValueChangedPrefiltered(object sender, ModuleParameterEventArgs e)
    {
      if (e.ParameterName == this.ParameterName)
      {
        OnValueChanged(sender, e);
      }
    }

    // WARNING: this will be called after you call SetModuleParameterValue
    // DO NOT invoke SetModuleParameterValue or otherwise change the ModuleNode's
    // parameter from this method (infinite feedback loop)
    public abstract void OnValueChanged(object sender, ModuleParameterEventArgs e);

    protected void SetModuleParameterValue(object value)
    {
      this.NodeControl.Node[ParameterName] = value;
    }

    protected void SetModuleParameterValue(string otherParamName, object value)
    {
      this.NodeControl.Node[otherParamName] = value;
    }

    public static EditorControl CreateDefaultEditorInstanceFor(
      Type type, ModuleNodeControl control, string parameterName)
    {
      // TODO: create a registry of default editors, registered via subclasses using
      // an EditorAttribute
      throw new NotImplementedException();
    }
  }
}

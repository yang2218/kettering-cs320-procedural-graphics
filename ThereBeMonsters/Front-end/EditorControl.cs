using System;
using System.Collections.Generic;
using OpenTKGUI;

namespace ThereBeMonsters.Front_end
{
  public abstract class EditorControl : Control
  {
    // TODO: reference to the ModuleGraph this control is inside of
    protected string parameterName;

    public abstract double PreferredHeight { get; }

    public EditorControl(string paramName)
    {
      this.parameterName = paramName;
    }

    protected void SetModuleParameterValue(string name, object value)
    {
      // TODO: put the value in the modulegraph's valueWireups
    }
  }
}

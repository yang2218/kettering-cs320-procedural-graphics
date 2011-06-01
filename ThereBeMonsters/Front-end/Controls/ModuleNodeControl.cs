using System;
using System.Collections.Generic;
using OpenTK;
using OpenTKGUI;
using ThereBeMonsters.Front_end.Controls;

using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Front_end
{
  public class ModuleNodeControl : Form
  {
    public ModuleNode Node { get; private set; }
    public Dictionary<string, EditorControl> EditorControls { get; private set; }

    public ModuleNodeControl(ModuleNode bide)
      : base(null, string.Empty) // TODO: figure out how base class is going to be used
    {
      this.Node = Node;
      this.EditorControls = new Dictionary<string, EditorControl>();

      Dictionary<string, Module.Parameter> parameters
        = Module.GetModuleParameters(Node.ModuleType);

      EditorControl editor;
      foreach (KeyValuePair<string, Module.Parameter> kvp in parameters)
      {
        if (kvp.Value.Hidden)
        {
          continue;
        }

        editor = kvp.Value.GetEditorInstance(this);
        EditorControls[kvp.Key] = editor;

        // TODO: add editor to child control heiarchy
      }
    }

    public void OnParameterUpdated(object sender, ModuleParameterEventArgs e)
    {
      if (EditorControls.ContainsKey(e.ParameterName))
      {
        EditorControls[e.ParameterName].OnValueChanged(sender, e);
      }
    }
  }
}

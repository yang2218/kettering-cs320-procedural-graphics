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

    public ModuleNodeControl(ModuleNode node)
      : base(new FlowContainer(Axis.Vertical), node.ModuleType.Name)
    {
      this.Node = node;
      this.EditorControls = new Dictionary<string, EditorControl>();

      this.ClientSize = new Point(150, 200);

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

    private Vector2 _lastPostion;
    public override void Update(GUIControlContext Context, double Time)
    {
      base.Update(Context, Time);

      if (this.Position != _lastPostion)
      {
        Node.Position = _lastPostion = this.Position;
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

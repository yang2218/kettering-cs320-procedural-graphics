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

      FlowContainer flow = (FlowContainer)this.Client;

      Textbox tb = new Textbox();
      tb.Text = node.ModuleId;
      flow.AddChild(tb, 20);

      tb.TextEntered += (text) =>
      {
        node.ModuleId = text;
        // TODO validate uniquness and non-empty
      };

      double borderSize = this.ClientRectangle.TopLeft.X;
      double titleSize = this.ClientRectangle.TopLeft.Y;

      EditorControl editor;
      foreach (KeyValuePair<string, Module.Parameter> kvp in parameters)
      {
        if (kvp.Value.Hidden)
        {
          continue;
        }

        FlowContainer horiz = new FlowContainer(Axis.Horizontal);
        // add left bubble
        if((kvp.Value.Direction & Module.Parameter.IODirection.INPUT) > 0)
        {
          horiz.AddChild(new BubbleControl(this, kvp.Key, false, new Point(borderSize + 6, flow.SuggestLength + 10 + titleSize)), 12);
        }
        else
        {
          horiz.AddChild(new Blank(Color.RGB(.8,.8,.8)), 12);
        }

        horiz.AddChild(new Label(kvp.Key), 126);

        // add right bubble
        if((kvp.Value.Direction & Module.Parameter.IODirection.OUTPUT) > 0)
        {
          horiz.AddChild(new BubbleControl(this, kvp.Key, true, new Point(borderSize + 150 - 6, flow.SuggestLength + 10 + titleSize)), 12);
        }
        else
        {
          horiz.AddChild(new Blank(Color.RGB(.8, .8, .8)), 12);
        }
        flow.AddChild(horiz, 20);

        if ((kvp.Value.Direction & Module.Parameter.IODirection.INPUT) == 0)
        {
          continue;
        }

        editor = kvp.Value.GetEditorInstance(this);
        EditorControls[kvp.Key] = editor;

        // TODO: add editor to child control heiarchy
        if( editor != null)
        {
          flow.AddChild(editor, editor.PreferredHeight);
        }
      }

      this.ResizeChild(this.Client,this.ClientSize);
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

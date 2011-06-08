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
    public ModuleGraphControl Parent { get; private set; }
    public ModuleNode Node { get; private set; }
    public Dictionary<string, EditorControl> EditorControls { get; private set; }
    public Dictionary<string, BubbleControl> LeftBubbleControls { get; private set; }
    public Dictionary<string, BubbleControl> RightBubbleControls { get; private set; }

    public EditorControl this[string paramName]
    {
      get
      {
        return EditorControls[paramName];
      }
    }

    public BubbleControl this[string paramName, bool right]
    {
      get
      {
        if (right)
        {
          return RightBubbleControls[paramName];
        }
        else
        {
          return LeftBubbleControls[paramName];
        }
      }
    }

    private double _closeGrase;

    public ModuleNodeControl(ModuleGraphControl parent, ModuleNode node)
      : base(new FlowContainer(Axis.Vertical), node.ModuleType.Name)
    {
      this.Parent = parent;
      this.Node = node;
      this.EditorControls = new Dictionary<string, EditorControl>();
      this.LeftBubbleControls = new Dictionary<string, BubbleControl>();
      this.RightBubbleControls = new Dictionary<string, BubbleControl>();

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
      BubbleControl bubble;
      foreach (KeyValuePair<string, Module.Parameter> kvp in parameters)
      {
        if (kvp.Value.Hidden)
        {
          continue;
        }

        FlowContainer horiz = new FlowContainer(Axis.Horizontal);
        // add left bubble
        if ((kvp.Value.Direction & Module.Parameter.IODirection.INPUT) > 0)
        {
          bubble = new BubbleControl(this, kvp.Key, false,
            new Point(borderSize + 6, flow.SuggestLength + titleSize + borderSize + 4));
          LeftBubbleControls[kvp.Key] = bubble;
          horiz.AddChild(bubble, 12);
        }
        else
        {
          horiz.AddChild(new Blank(Color.RGB(.8,.8,.8)), 12);
        }

        horiz.AddChild(new Label(kvp.Key), 126);

        // add right bubble
        if ((kvp.Value.Direction & Module.Parameter.IODirection.OUTPUT) > 0)
        {
          bubble = new BubbleControl(this, kvp.Key, true,
            new Point(borderSize + 150 - 6, flow.SuggestLength + titleSize + borderSize + 4));
          RightBubbleControls[kvp.Key] = bubble;
          horiz.AddChild(bubble, 12);
        }
        else
        {
          horiz.AddChild(new Blank(Color.RGB(.8, .8, .8)), 12);
        }

        flow.AddChild(horiz, 20);
        
        if ((kvp.Value.Direction & Module.Parameter.IODirection.INPUT) == 0
          && (kvp.Value.Direction & Module.Parameter.IODirection.NOWIREUP) == 0)
        {
          continue;
        }

        editor = kvp.Value.GetEditorInstance(this);
        EditorControls[kvp.Key] = editor;

        if( editor != null)
        {
          flow.AddChild(editor, editor.PreferredHeight);
        }
      }

      this.ClientSize = new Point(150, flow.SuggestLength);
      this.ResizeChild(this.Client,this.ClientSize);

      // Add a close button
      Button b = this.AddTitlebarButton(FormStyle.Default.CloseButtonStyle, null);
      b.Click += ConfirmedClose;
    }

    public BubbleControl GetBubbleForPoint(Point p)
    {
      // TODO:
      //   check x position to see if it might be left, right, or none
      //   then go through the dictionary and see if there's a bubble
      //   whose offset is near the point's .Y

      foreach (BubbleControl bc in LeftBubbleControls.Values)
      {
        if(new Rectangle(bc.Offset.X - 6, bc.Offset.Y - 6, 12, 12).In(p))
        {
          return bc;
        }
      }

      foreach (BubbleControl bc in RightBubbleControls.Values)
      {
        if (new Rectangle(bc.Offset.X - 6, bc.Offset.Y - 6, 12, 12).In(p))
        {
          return bc;
        }
      }

      return null;
    }

    private void ConfirmedClose()
    {
      if (_closeGrase > 0.0)
      {
        Parent.Graph.Remove(Node.ModuleId);
        return;
      }

      _closeGrase = 1.0;
    }

    public void OnRemoved()
    {
      foreach (BubbleControl bc in LeftBubbleControls.Values)
      {
        bc.OnRemove();
      }
    }

    private Vector2 _lastPostion;
    public override void Update(GUIControlContext Context, double Time)
    {
      base.Update(Context, Time);

      _closeGrase -= Time;

      if (this.Position != _lastPostion)
      {
        if (this.Position.X < 0 || this.Position.Y < 0)
        {
          this.Position = new Point(
            this.Position.X < 0 ? 0 : this.Position.X,
            this.Position.Y < 0 ? 0 : this.Position.Y);
        }

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

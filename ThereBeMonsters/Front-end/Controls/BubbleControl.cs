using System;
using System.Collections.Generic;
using OpenTKGUI;
using OpenTK;
using ThereBeMonsters.Back_end;
using OpenTK.Input;

namespace ThereBeMonsters.Front_end.Controls
{
  public class BubbleControl : Label
  {
    public ModuleNodeControl NodeControl { get; private set; }
    public string ParameterName { get; private set; }
    bool IsOutput;
    public Point Offset { get; private set; }

    public BubbleControl(ModuleNodeControl nodeControl, 
                         string parameterName, 
                         bool isOutput, 
                         Point offset)
      : base("o",Color.RGB(.5,0,0))
    {
      NodeControl = nodeControl;
      ParameterName = parameterName;
      IsOutput = isOutput;
      Offset = offset;

      if (isOutput == false)
      {
        NodeControl.Node.ParameterUpdated += OnParameterChanged;
      }
    }

    private void OnParameterChanged(object sender, ModuleParameterEventArgs e)
    {
      if (e.ParameterName != this.ParameterName)
      {
        return;
      }

      object value = NodeControl.Node[ParameterName];
      if (value == null || value.GetType() != typeof(ParameterWireup))
      {
        // TODO: remove connector
      }
      else
      {
        ParameterWireup pw = (ParameterWireup)value;
        // TODO: move connector's far endpoint
      }
    }

    public override void Update(GUIControlContext Context, double Time)
    {
      // input buble can have only 1 link
      // output bubble can have as many links as it wants

      // disconnect only using input bubbles
      
      // connect only from output to inputs

      // start out only support click drag from output to input

      // Bubble responsible for changing the model, not connectors

      //nodeControl.Node[parameterName].GetType()==typeof(ParameterWireup);

      OpenTKGUI.MouseState ms = Context.MouseState;
      if(ms == null) return;

      if(new Rectangle(0,0,12,12).In(ms.Position))
      {
        if(ms.HasPushedButton(MouseButton.Left))
        {
          if(IsOutput)
          {
            // create connection control to follow mouse coursor

            // when released, try to find the bubble under the mouse
            Context.CaptureMouse();
          }
          else
          {
            // if connection, unconnect and have it follow the mouse
            NodeControl.Node[ParameterName] = null;
            // otherrwise, create connection
            // when releasd and find the bubble under the cursor
          }
        }

        if(ms.HasReleasedButton(MouseButton.Left))
        {
          if (IsOutput)
          {
            //uncapture mouse
            Context.ReleaseMouse();

          }
          else
          {
          }
        }
      }
    }
  }
}

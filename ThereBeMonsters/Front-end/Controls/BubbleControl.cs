using System;
using System.Collections.Generic;
using OpenTKGUI;
using OpenTK;
using ThereBeMonsters.Back_end;

namespace ThereBeMonsters.Front_end.Controls
{
  public class BubbleControl : Label
  {
    public ModuleNodeControl NodeControl { get; private set; }
    public string ParameterName { get; private set; }
    bool IsOutput;
    public Point Offset { get; private set; }

    public static BubbleControl ClickDraggedBubbledControl;


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

      //nodeControl.Node[parameterName].GetType()==typeof(ParameterWireup);
    }

    public override void Update(GUIControlContext Context, double Time)
    {
      //base.Update(Context, Time);

      // input buble can have only 1 link
      // output bubble can have as many links as it wants

      // disconnect only using input bubbles
      
      // connect only from output to inputs

      // start out only support click drag from output to input

      // tell connect control exact position of the bubble

      // we can calculate it manually or use from update polls

      MouseState ms = Context.MouseState;
      if(ms == null) return;

      if(new Rectangle(0,0,12,12).In(ms.Position))
      {
        if(ms.HasPushedButton(OpenTK.Input.MouseButton.Left))
        {
          if(IsOutput)
          {
            ClickDraggedBubbledControl = this;

            // create connection control to follow mouse coursor until we leftMouseUp somewhere
          }
          else
          {
            // if connection, unconnect and have it follow the mouse
          }
        }

        if(ms.HasReleasedButton(OpenTK.Input.MouseButton.Left))
        {
          if (IsOutput == false && ClickDraggedBubbledControl != null)
          {
            if(ClickDraggedBubbledControl.IsOutput)
            {
              // give all bubbles 1 frame to claim that button was released above them
              // if none claim then nullify the variable ClickDraggedBubbledControl
            }
          }
        }
      }
    }
  }
}

using System;
using System.Collections.Generic;
using OpenTKGUI;
using OpenTK;
using ThereBeMonsters.Back_end;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace ThereBeMonsters.Front_end.Controls
{
  public class BubbleControl : Control
  {
    public static readonly Color defaultColor = Color.RGB(0.2, 0.2, 0.8),
      goodColor = Color.RGB(0.2, 0.8, 0.2),
      badColor = Color.RGB(0.8, 0.2, 0.2),
      maybeColor = Color.RGB(0.8, 0.8, 0.2);

    public ModuleNodeControl NodeControl { get; private set; }
    public string ParameterName { get; private set; }
    public bool IsOutput { get; private set; }
    public Point Offset { get; private set; }
    public ConnectorControl Connector { get; set; }
    public Color Color { get; set; }

    public BubbleControl(ModuleNodeControl nodeControl,
                         string parameterName,
                         bool isOutput,
                         Point offset)
    {
      NodeControl = nodeControl;
      ParameterName = parameterName;
      IsOutput = isOutput;
      Offset = offset;
      Color = Color.RGB(0.2, 0.2, 0.8);

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
      if (value != null && value.GetType() == typeof(ParameterWireup) && Connector == null)
      {
        ParameterWireup pw = (ParameterWireup)value;
        BubbleControl bc = NodeControl.Parent[pw.srcId][pw.srcParam, true];
        /*Connector =*/ new ConnectorControl(bc, this);
      }
    }

    public override void Update(GUIControlContext Context, double Time)
    {
      OpenTKGUI.MouseState ms = Context.MouseState;
      if (ms == null)
      {
        return;
      }

      if (ms.HasPushedButton(MouseButton.Left))
      {
        if (Connector == null)
        {
          new ConnectorControl(IsOutput ? this : null, IsOutput ? null : this);
        }
        else
        {
          Connector.RBubble = null;
          NodeControl.Node[ParameterName] = null;
        }
      }
    }

    public override void Render(GUIRenderContext Context)
    {
      base.Render(Context);

      Context.DrawSolid(Color.RGB(0.8, 0.8, 0.8), new Rectangle(this.Size));
      GL.Begin(BeginMode.TriangleFan);
      GL.Color4(this.Color);
      GL.Vertex2((Vector2)this.Size / 2f);
      GL.Color4(Color.RGB(0.8, 0.8, 0.8));
      double half = this.Size.X / 2.0;
      double yoff = this.Size.Y / 2.0;
      for (float t = 0; t <= MathHelper.TwoPi + 0.001; t += MathHelper.PiOver6)
      {
        GL.Vertex2(half + Math.Cos(t) * half, yoff + Math.Sin(t) * half);
      }

      GL.End();
    }

    internal void OnRemove()
    {
      if (Connector != null)
      {
        Connector.Remove();
      }
    }
  }
}

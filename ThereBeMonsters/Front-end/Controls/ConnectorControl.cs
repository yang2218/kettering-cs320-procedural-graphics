using System;
using System.Collections.Generic;
using OpenTKGUI;
using ThereBeMonsters.Back_end;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ThereBeMonsters.Front_end.Controls
{
  public class ConnectorControl : LayerControl
  {
    private BubbleControl _lBubble;
    public BubbleControl LBubble
    {
      get
      {
        return _lBubble;
      }
      set
      {
        if (_lBubble != null)
        {
          _lBubble.NodeControl.Node.Moved -= ClearPath;
        }

        _lBubble = value;

        if (_lBubble != null)
        {
          _lBubble.NodeControl.Node.Moved += ClearPath;
        }
      }
    }

    private BubbleControl _rBubble;
    public BubbleControl RBubble
    {
      get
      {
        return _rBubble;
      }
      set
      {
        if (_rBubble != null)
        {
          _rBubble.NodeControl.Node.Moved -= ClearPath;
          _rBubble.NodeControl.Node.ParameterUpdated -= OnParameterChanged;
          _rBubble.Connector = null;
        }

        _rBubble = value;

        if (_rBubble != null)
        {
          _rBubble.NodeControl.Node.Moved += ClearPath;
          _rBubble.NodeControl.Node.ParameterUpdated += OnParameterChanged;
          _rBubble.Connector = this;
        }
      }
    }

    private List<Vector2> _path = new List<Vector2>();
    private Point _mousePos;
    private double _clickOrDragGrace;
    private BubbleControl _mouseOverBubble;

    public ConnectorControl(BubbleControl lBubble, BubbleControl rBubble)
    {
      this.LBubble = lBubble;
      this.RBubble = rBubble;
      (LBubble ?? RBubble).NodeControl.Parent.AddControl(this, new Point(0, 0));
      _clickOrDragGrace = 1.0;
      _mousePos = (LBubble ?? RBubble).Offset + (LBubble ?? RBubble).NodeControl.Position;
    }

    public void ClearPath(object sender, ModuleMovedEventArgs e)
    {
      _path.Clear();
    }

    private void OnParameterChanged(object sender, ModuleParameterEventArgs e)
    {
      if (e.ParameterName != RBubble.ParameterName)
      {
        return;
      }

      object value = RBubble.NodeControl.Node[RBubble.ParameterName];
      if (value == null || value.GetType() != typeof(ParameterWireup))
      {
        RBubble.NodeControl.Parent.RemoveControl(this);
        LBubble = RBubble = null;
      }
      else
      {
        ParameterWireup pw = (ParameterWireup)value;
        LBubble = RBubble.NodeControl.Parent[pw.srcId][pw.srcParam, true];
      }
    }

    public override void Update(GUIControlContext Context, double Time)
    {
      _clickOrDragGrace -= Time;

      if (Context.HasMouse == false)
      {
        if (LBubble == null || RBubble == null)
        {
          Context.CaptureMouse();
        }

        return;
      }

      MouseState ms = Context.MouseState;
      _mousePos = ms.Position;
      _path.Clear();

      ModuleNodeControl node = (LBubble ?? RBubble).NodeControl.Parent.GetNodeForPoint(ms.Position);
      BubbleControl bc = null;
      if (node != null)
      {
        bc = node.GetBubbleForPoint(ms.Position - node.Position);
        if (bc != _mouseOverBubble)
        {
          if (_mouseOverBubble != null)
          {
            _mouseOverBubble.Color = BubbleControl.defaultColor;
          }

          _mouseOverBubble = bc;
          if (bc != null)
          {
            Type t1, t2;
            t1 = Module.GetModuleParameters(bc.NodeControl.Node.ModuleType)[bc.ParameterName].Type;
            t2 = Module.GetModuleParameters((LBubble ?? RBubble)
              .NodeControl.Node.ModuleType)[(LBubble ?? RBubble).ParameterName].Type;

            bc.Color = (bc.IsOutput != (LBubble ?? RBubble).IsOutput
              && (LBubble ?? RBubble).NodeControl != bc.NodeControl)
              ? ((t1 == t2)
               ? BubbleControl.goodColor
               : BubbleControl.maybeColor)
              : BubbleControl.badColor;
          }
        }
      }
      else if (_mouseOverBubble != null)
      {
        _mouseOverBubble.Color = BubbleControl.defaultColor;
        _mouseOverBubble = null;
      }


      if (ms.HasReleasedButton(OpenTK.Input.MouseButton.Left) && _clickOrDragGrace < 0.0)
      {
        if (bc != null
          && bc.IsOutput != (LBubble ?? RBubble).IsOutput
          && (LBubble ?? RBubble).NodeControl != bc.NodeControl)
        {
          LBubble = LBubble ?? bc;
          RBubble = RBubble ?? bc;
          RBubble.NodeControl.Node[RBubble.ParameterName]
            = new ParameterWireup(LBubble.NodeControl.Node.ModuleId, LBubble.ParameterName);
        }
        else
        {
          (LBubble ?? RBubble).NodeControl.Parent.RemoveControl(this);
          LBubble = RBubble = null;
        }

        Context.ReleaseMouse();
        if (_mouseOverBubble != null)
        {
          _mouseOverBubble.Color = BubbleControl.defaultColor;
        }
      }
    }

    public override void Render(GUIRenderContext Context)
    {
      if (_path.Count == 0)
      {
        RecalculatePath();
      }

      GL.Disable(EnableCap.Texture2D);
      GL.Color3(Color.RGB(1, 1, 1));

      GL.Begin(BeginMode.LineStrip);
      foreach (Vector2 v in _path)
      {
        GL.Vertex2(v);
      }

      GL.End();
    }

    private void RecalculatePath()
    {
      Vector2[] points = new Vector2[4];

      points[0] =
        LBubble != null
        ? (LBubble.NodeControl.Position + LBubble.Offset)
        : _mousePos;

      points[3] =
        RBubble != null
        ? (RBubble.NodeControl.Position + RBubble.Offset)
        : _mousePos;

      float midpoint = (points[0].X + points[3].X) / 2;
      points[1] = new Vector2(midpoint, points[0].Y);
      points[2] = new Vector2(midpoint, points[3].Y);

      OpenTK.BezierCurve test = new BezierCurve(points);

      float length = test.CalculateLength(.1f);
      float increment =
        length != 0f
        ? (3f / length)
        : (3f / 1f);

      for (float t = 0; t < 1; t += increment)
      {
        _path.Add(test.CalculatePoint(t));
      }
    }
  }
}

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
      // TODO: use OpenTK.BeizerCurve to generate a bunch of points to put in _path
      // construct a BeizerCurve
      // first point:
      //   if LBubble is null, just use LOffset
      //   otherwise, use LBubble's position + LOFfset
      // second point: just a little to the right of first point
      // third point: just a little to the left of the fourth point
      // fourth point: like first point, but for right bubble/offset

      // call calclength, use t increment of about 3/length (careful for length==0!)
      // call calcpoint for each t in [0..1] by above increment, add to _path

      _path.Add(LBubble != null
        ? (LBubble.NodeControl.Position + LBubble.Offset)
        : _mousePos);
      _path.Add(RBubble != null
        ? (RBubble.NodeControl.Position + RBubble.Offset)
        : _mousePos);
    }
  }
}

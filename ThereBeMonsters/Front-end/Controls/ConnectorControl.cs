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
    public Point LOffset { get; private set; }
    public Point ROffset { get; private set; }
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
        }

        _rBubble = value;

        if (_rBubble != null)
        {
          _rBubble.NodeControl.Node.Moved += ClearPath;
        }
      }
    }

    private List<Vector2> _path = new List<Vector2>();

    private bool _checkForRemoval;

    public ConnectorControl(BubbleControl lBubble, BubbleControl rBubble)
    {
      this.LBubble = lBubble;
      this.RBubble = rBubble;
    }

    public void ClearPath(object sender, ModuleMovedEventArgs e)
    {
      LOffset = LBubble.NodeControl.Position + LBubble.Offset;

      _path.Clear();
    }

    public override void Update(GUIControlContext Context, double Time)
    {
      // TODO: if an endpoint bubble is null,
      // change the corresponding offset to the mouse pos (relative to moduleGraphControl),
      // then ClearPath
      // MainWindow.hack.MouseState.Position 

      _path.Clear();

      if (_checkForRemoval)
      {
        if (RBubble == null)
        {
          this.Container.RemoveControl(this);
        }

        _checkForRemoval = false;
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
    }
  }
}

using System;
using System.Collections.Generic;
using ThereBeMonsters.Back_end;
using OpenTKGUI;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace ThereBeMonsters.Front_end
{
  public class ModuleGraphWindowControl : WindowContainer
  {
    private Point _dragOffset;

    public ModuleGraphWindowControl(ModuleGraphControl graph)
      : base(graph)
    {
    }

    public override void Update(GUIControlContext Context, double Time)
    {
      base.Update(Context, Time);

      MouseState ms = Context.MouseState;
      if (ms == null)
      {
        return;
      }

      if (ms.HasPushedButton(OpenTK.Input.MouseButton.Middle))
      {
        _dragOffset = this.Offset + ms.Position;
        Context.CaptureMouse();
      }
      else if (ms.IsButtonDown(OpenTK.Input.MouseButton.Middle))
      {
        Point p = _dragOffset - ms.Position;
        if (p.X < 0 || p.Y < 0)
        {
          p = new Point(p.X < 0 ? 0 : p.X, p.Y < 0 ? 0 : p.Y);
          _dragOffset = p + ms.Position;
        }

        this.Offset = p;

        // TODO: if position near the edge, teleport mouse cursor to other edge
      }
      else if (ms.HasReleasedButton(OpenTK.Input.MouseButton.Middle))
      {
        Context.ReleaseMouse();
      }
    }
  }

  public class ModuleGraphBackground : PopupContainer
  {
    public ModuleGraphControl Parent { get; private set; }
    public Vector2 MousePosAtCall { get; set; }

    private int _serialNum;

    public ModuleGraphBackground(ModuleGraphControl parent)
      : base(new Blank(Color.RGB(0, 0, 0)))
    {
      this.Parent = parent;
      this.ShowOnRightClick = true;
      List<MenuItem> menuItems = new List<MenuItem>();
      foreach (Type t in Module.ModuleList)
      {
        Type t2 = t;
        menuItems.Add(MenuItem.Create(t2.Name, () =>
          {
            parent.Graph.Add(t2.Name + _serialNum++, t2, this.MousePosAtCall);
          }));
      }

      this.Items = menuItems.ToArray();
    }

    public override void Update(GUIControlContext Context, double Time)
    {
      base.Update(Context, Time);
      MouseState ms = Context.MouseState;
      if (ms != null && ms.HasReleasedButton(OpenTK.Input.MouseButton.Right))
      {
        MousePosAtCall = ms.Position;
      }
    }
  }

  public class ModuleGraphControl : LayerContainer
  {
    private ModuleGraph _graph;
    public ModuleGraph Graph
    {
      get
      {
        return _graph;
      }
      set
      {
        if (_graph != null)
        {
          _graph.ModuleAdded -= OnModuleAdded;
          _graph.ModuleRemoved -= OnModuleRemoved;
          _graph.ModuleMoved -= OnModuleMoved;
        }

        foreach (ModuleNodeControl control in _idMap.Values)
        {
          control.OnRemoved();
          this.RemoveControl(control);
        }

        _idMap.Clear();

        _graph = value;
        _graph.ModuleAdded += OnModuleAdded;
        _graph.ModuleRemoved += OnModuleRemoved;
        _graph.ModuleMoved += OnModuleMoved;

        foreach (ModuleNode node in _graph.Nodes.Values)
        {
          OnModuleAdded(_graph, new ModuleEventArgs(node.ModuleId));
        }

        foreach (ModuleNode node in _graph.Nodes.Values)
        {
          node.ForceParameterUpdate();
        }
      }
    }

    public ModuleNodeControl this[string moduleId]
    {
      get
      {
        return _idMap[moduleId];
      }
    }

    private Dictionary<string, ModuleNodeControl> _idMap
      = new Dictionary<string, ModuleNodeControl>();

    public ModuleGraphControl()
      : this(new ModuleGraph())
    {
    }

    public ModuleGraphControl(ModuleGraph g)
      : base(null)
    {
      this.Background = new ModuleGraphBackground(this);
      Graph = g;
    }

    public ModuleNodeControl GetNodeForPoint(Point p)
    {
      Rectangle r;
      foreach (ModuleNodeControl node in _idMap.Values)
      {
        r = new Rectangle(node.Position + node.ClientRectangle.Location, node.ClientRectangle.Size);
        if (r.In(p))
        {
          return node;
        }
      }

      return null;
    }

    private void OnModuleAdded(object sender, ModuleEventArgs e)
    {
      ModuleNode node = Graph[e.ModuleId];
      ModuleNodeControl nodeControl = new ModuleNodeControl(this, node);
      _idMap[e.ModuleId] = nodeControl;
      AddControl(nodeControl, new Point(node.X, node.Y));
    }

    private void OnModuleMoved(object sender, ModuleMovedEventArgs e)
    {
      // TODO: reposition child control
      // - not really necessary since currently moving the control causes the
      //   model to move (instead of the other way around)
    }

    private void OnModuleRemoved(object sender, ModuleEventArgs e)
    {
      _idMap[e.ModuleId].OnRemoved();
      this.RemoveControl(_idMap[e.ModuleId]);
      _idMap.Remove(e.ModuleId);
    }
  }
}

using System;
using System.Collections.Generic;
using ThereBeMonsters.Back_end;
using OpenTKGUI;
using OpenTK.Graphics.OpenGL;

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
      }
    }
  }

  public class ModuleGraphBackground : PopupContainer
  {
    public ModuleGraphControl Parent { get; private set; }

    public ModuleGraphBackground(ModuleGraphControl parent)
      : base(new Blank())
    {
      this.Parent = parent;
      this.ShowOnRightClick = true;
      this.Items = new MenuItem[] {
        // TODO: Menu items create a new module at the popup positoin
        MenuItem.Create("Test"),
        MenuItem.Create("Test2")
      };
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

        // TODO: remove all child controls

        _graph = value;
        _graph.ModuleAdded += OnModuleAdded;
        _graph.ModuleRemoved += OnModuleRemoved;
        _graph.ModuleMoved += OnModuleMoved;

        foreach (ModuleNode node in _graph.Nodes.Values)
        {
          OnModuleAdded(_graph, new ModuleEventArgs(node.ModuleId));
        }
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
      foreach (ModuleNodeControl node in _idMap.Values)
      {
        if (node.ClientRectangle.In(p))
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
    }
  }
}

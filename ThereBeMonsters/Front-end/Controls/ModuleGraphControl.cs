using System;
using System.Collections.Generic;
using ThereBeMonsters.Back_end;
using OpenTKGUI;

namespace ThereBeMonsters.Front_end
{
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
      : base(null)
    {
      Graph = new ModuleGraph();
    }

    public ModuleGraphControl(ModuleGraph g)
      : base(null)
    {
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

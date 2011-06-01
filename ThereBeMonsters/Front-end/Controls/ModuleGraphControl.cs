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
      }
    }

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

    private void OnModuleAdded(object sender, ModuleEventArgs e)
    {
      ModuleNodeControl nodeControl = new ModuleNodeControl(Graph[e.ModuleId]);

      // TODO: position control
      // change the event info to include mouse position?

      AddControl(nodeControl, new Point());
    }

    private void OnModuleMoved(object sender, ModuleMovedEventArgs e)
    {
      // TODO: reposition child control
    }

    private void OnModuleRemoved(object sender, ModuleEventArgs e)
    {
      // TODO: remove the child for the id
    }
  }
}

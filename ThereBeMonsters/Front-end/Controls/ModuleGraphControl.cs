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
      private set
      {
        if (_graph != null)
        {
          _graph.ModuleAdded -= OnModuleAdded;
          _graph.ModuleRemoved -= OnModuleRemoved;
        }

        // TODO: remove all child controls

        _graph = value;
        _graph.ModuleAdded += OnModuleAdded;
        _graph.ModuleRemoved += OnModuleRemoved;
      }
    }

    public ModuleGraphControl()
      : base(null)
    {
      Graph = new ModuleGraph();
    }

    private void OnModuleAdded(object sender, ModuleEventArgs e)
    {
      ModuleNodeControl nodeControl = new ModuleNodeControl(_graph[e.ModuleId]);

      // TODO: position control
      // change the event info to include mouse position?

      AddControl(nodeControl, new Point());
    }

    private void OnModuleRemoved(object sender, ModuleEventArgs e)
    {
      // TODO: remove the child for the id
    }
  }
}

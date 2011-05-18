using System;
using System.Collections.Generic;
using ThereBeMonsters.Back_end;
using OpenTKGUI;

namespace ThereBeMonsters.Front_end
{
  public class ModuleGraphControl : LayerContainer //, ICollection<ModuleNodeControl>
  {
    public ModuleGraph Graph { get; private set; }

    public ModuleGraphControl()
      : base(null)
    {
      Graph = new ModuleGraph();
    }


  }
}

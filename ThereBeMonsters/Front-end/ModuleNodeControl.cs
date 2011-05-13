using System;
using System.Collections.Generic;
using OpenTK;
using OpenTKGUI;

namespace ThereBeMonsters.Front_end
{
  public class ModuleNodeControl : Form
  {
    public string Id { get; private set; }

    public ModuleNodeControl()
      : base(new Button("formtest"), "test form")
    {
    }

  }
}

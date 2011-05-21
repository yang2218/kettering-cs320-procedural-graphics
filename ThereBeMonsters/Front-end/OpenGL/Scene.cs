using System;
using System.Collections.Generic;
using OpenTK;

namespace ThereBeMonsters.Front_end
{
  public class Scene
  {
    // TODO: any need for heiarchical structure?
    public LinkedList<Entity> Entities { get; private set; }

    public Scene()
    {
      Entities = new LinkedList<Entity>();
    }

    public void Draw()
    {
      foreach (Entity e in Entities)
      {
        e.Draw();
      }
    }
  }
}

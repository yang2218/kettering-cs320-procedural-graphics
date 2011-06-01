using System;
using OpenTK;

namespace TestProject.Objects
{
  public class MovingEntity : Entity
  {
    public Vector3 velocity;

    public void OnUpdatePosition(object sender, FrameEventArgs e)
    {
      this.transform.Pos += velocity * (float)e.Time;
    }
  }
}

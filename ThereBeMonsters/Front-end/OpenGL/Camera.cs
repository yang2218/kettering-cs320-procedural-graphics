using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;

namespace ThereBeMonsters.Front_end.OpenGL
{
  public class Camera
  {
    float xAngle, yAngle;

    public void SetupCamera(object sender, FrameEventArgs e)
    {
      if (MainWindow.hack.Keyboard[OpenTK.Input.Key.A])
      {
        xAngle -= MathHelper.PiOver6 * (float)e.Time;
      }

      Vector3 cameraPos = new Vector3(0f, 0f, 15f);

      Quaternion xrot = Quaternion.FromAxisAngle(Vector3.UnitY, xAngle);
      Vector3.Transform(ref cameraPos, ref xrot, out cameraPos);

      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadIdentity();
      GL.Ortho(-10, 10, -10, 10, 0, 50);
      GL.MatrixMode(MatrixMode.Modelview);

      Matrix4 camera;
      camera = Matrix4.LookAt(cameraPos, Vector3.Zero, Vector3.UnitY);
      GL.LoadMatrix(ref camera);

      GL.Clear(ClearBufferMask.DepthBufferBit);
    }
  }
}

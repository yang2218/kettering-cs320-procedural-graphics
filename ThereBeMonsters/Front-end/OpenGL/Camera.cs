using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTKGUI;

namespace ThereBeMonsters.Front_end.OpenGL
{
  public class Camera
  {
    public float fov;
    public Vector3 pos, forward, right, lean;
    public Vector3 velocity;
    public float speed, smoothFactor;
    public float lookSpeed;

    private System.Drawing.Point _previousMousePos;

    public Camera()
    {
      fov = (float)(Math.PI / 2.4);
      pos = new Vector3(0f, 0f, -5f);
      forward = Vector3.UnitZ;
      lean = Vector3.UnitY;
      speed = 15f;
      smoothFactor = 10f;
      lookSpeed = 0.001f;
    }

    public void Update(object sender, Viewport.UpdateEventArgs e)
    {
      KeyboardState ks = e.Context.KeyboardState;
      if (ks != null)
      {
        Vector3 targetVelocity = Vector3.Zero;

        if (ks.IsKeyDown(OpenTK.Input.Key.W))
        {
          targetVelocity += speed * forward;
        }

        if (ks.IsKeyDown(OpenTK.Input.Key.S))
        {
          targetVelocity -= speed * forward;
        }

        if (ks.IsKeyDown(OpenTK.Input.Key.A))
        {
          targetVelocity -= speed * right;
        }

        if (ks.IsKeyDown(OpenTK.Input.Key.D))
        {
          targetVelocity += speed * right;
        }

        // TODO: rotate by curent orientation

        Vector3.Lerp(ref velocity, ref targetVelocity,
          smoothFactor * e.Time, out velocity);
      }

      MouseState ms = e.Context.MouseState;
      if (ms == null)
      {
        return;
      }

      if (ms.HasPushedButton(OpenTK.Input.MouseButton.Left))
      {
        e.Context.CaptureMouse();
        e.Context.CaptureKeyboard();
        System.Windows.Forms.Cursor.Hide();
        _previousMousePos = System.Windows.Forms.Cursor.Position;
      }
      else if (ms.HasReleasedButton(OpenTK.Input.MouseButton.Left))
      {
        e.Context.ReleaseMouse();
        e.Context.ReleaseKeyboard();
        System.Windows.Forms.Cursor.Show();
        System.Windows.Forms.Cursor.Position = _previousMousePos;
      }

      if (e.Context.HasMouse)
      {
        // change position based on velocity
        Vector3 deltaPos;
        Vector3.Multiply(ref velocity, (float)e.Time, out deltaPos);
        Vector3.Add(ref pos, ref deltaPos, out pos);

        // Pitch, yaw rotations based on mouse
        // code "inspired" from http://www.opentk.com/node/952?page=1

        GameWindow w = MainWindow.Active;

        Vector2 mouseDelta = new Vector2(
          System.Windows.Forms.Cursor.Position.X - (w.Bounds.Left + w.Bounds.Right) / 2,
          System.Windows.Forms.Cursor.Position.Y - (w.Bounds.Top + w.Bounds.Bottom) / 2);

        System.Windows.Forms.Cursor.Position = new System.Drawing.Point(
          (w.Bounds.Left + w.Bounds.Right) / 2,
          (w.Bounds.Top + w.Bounds.Bottom) / 2);

        Vector3 up = Vector3.UnitY;
        Quaternion xrot = Quaternion.FromAxisAngle(right, mouseDelta.Y * -lookSpeed);
        Quaternion yrot = Quaternion.FromAxisAngle(up, mouseDelta.X * -lookSpeed);
        Quaternion rot;
        Quaternion.Multiply(ref xrot, ref yrot, out rot);

        Vector3.Transform(ref forward, ref rot, out forward);
        Vector3.Cross(ref forward, ref up, out right);

        right.NormalizeFast();
      }
    }

    public void SetupCamera(object sender, Viewport.PreRenderEventArgs e)
    {
      GL.MatrixMode(MatrixMode.Projection);
      Matrix4 mat;
      Matrix4.CreatePerspectiveFieldOfView(
        fov,
        (float)e.ViewSize.AspectRatio,
        0.1f,
        100f,
        out mat);
      GL.LoadMatrix(ref mat);

      GL.MatrixMode(MatrixMode.Modelview);
      mat = Matrix4.LookAt(pos, pos + forward, lean);
      GL.LoadMatrix(ref mat);
    }
  }
}

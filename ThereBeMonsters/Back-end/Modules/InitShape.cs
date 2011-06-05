using System;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace ThereBeMonsters.Back_end.Modules
{
  [Module("Initial Shape")]
  class InitShape : Module
  {
    [Parameter("Initial Shape")]
    public List<Vector2> Shape { get; private set; }

    public override void Run()
    {
      using (PointInput window = new PointInput())
      {
        window.Title = "RAWR";
        window.Size = new Size(900, 900);
        window.X = 2;
        window.Y = 2;
        window.Run(30.0, 0.0);
        Shape = window.Shape;
      }

      Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = Vector2.Zero;
      foreach (Vector2 v in Shape)
      {
        min = Vector2.ComponentMin(min, v);
        max = Vector2.ComponentMax(max, v);
      }

      Vector2 scale;
      Vector2.Subtract(ref max, ref min, out scale);
      scale = new Vector2(1000f / scale.X, 1000f / scale.Y);

      for (int i = 0; i < Shape.Count; i++)
      {
        Shape[i] = Vector2.Multiply(Shape[i] - min, scale);
      }

      isInOrder();
      Convex();

      Shape.Add(Shape[0]);
    }

    #region inputCheck
    public void isInOrder()
    {
      int i, j, k;
      int count = 0;
      double z;

      for (i = 0; i < Shape.Count; i++)
      {
        j = (i + 1) % Shape.Count;
        k = (i + 2) % Shape.Count;
        z = (Shape[j].X - Shape[i].X) * (Shape[k].Y - Shape[j].Y);
        z -= (Shape[j].Y - Shape[i].Y) * (Shape[k].X - Shape[j].X);
        if (z < 0)
          count--;
        else if (z > 0)
          count++;
      }
      if (count < 0)
        Console.WriteLine("Counter Clock Wise");
      else if (count > 0)
        Console.WriteLine("Clockwise");
      else
        Console.WriteLine("You are a bad person, put in CCW or CW!!!!");
    }


    public void Convex()
    {
      int i, j, k;
      int flag = 0;
      double z;

      for (i = 0; i < Shape.Count; i++)
      {
        j = (i + 1) % Shape.Count;
        k = (i + 2) % Shape.Count;
        z = (Shape[j].X - Shape[i].X) * (Shape[k].Y - Shape[j].Y);
        z -= (Shape[j].Y - Shape[i].Y) * (Shape[k].Y - Shape[j].Y);
        if (z < 0)
          flag |= 1;
        else if (z > 0)
          flag |= 2;
        if (flag == 3)
        {
          Console.WriteLine("Concave");
          return;
        }
      }
      if (flag != 0)
        Console.WriteLine("Convex");
      else
        return;
    }
    #endregion

    #region Get Points
    class PointInput : GameWindow
    {
      public List<Vector2> Shape { get; private set; }

      protected override void OnLoad(EventArgs e)
      {
        Shape = new List<Vector2>();

        GL.Viewport(ClientRectangle);
        GL.ClearColor(Color.Black);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(0, 900, 0, 900, 0, 100);
        Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
      }

      protected override void OnResize(EventArgs e)
      {
        base.OnResize(e);
        GL.Viewport(ClientRectangle);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(0, ClientSize.Width, 0, ClientSize.Height, 0, 100);
      }

      protected override void OnRenderFrame(FrameEventArgs e)
      {
        GL.Clear(ClearBufferMask.ColorBufferBit);

        if (Shape.Count > 0)
        {
          Draw();
        }

        this.SwapBuffers();
      }

      private void Mouse_ButtonDown(object s, MouseButtonEventArgs e)
      {
        Shape.Add(new Vector2(Mouse.X, ClientSize.Height - Mouse.Y));
        Console.WriteLine(Shape.Count + " " + Shape[Shape.Count - 1]);
      }

      public void Draw()
      {
        GL.Begin(BeginMode.LineStrip);
        foreach (Vector2 v in Shape)
        {
          GL.Vertex2(v);
        }
        GL.Vertex2(Shape[0]);
        GL.End();
      }

      [STAThread]
      public static void Main()
      {
        using (PointInput window = new PointInput())
        {
          window.Title = "RAWR";
          window.Size = new Size(1600, 900);
          window.X = 2;
          window.Y = 2;
          window.Run(30.0, 0.0);
        }
      }
    }
    #endregion
  }
}

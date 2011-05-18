using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using TestProject.Objects;

namespace TestProject.Lab3
{
  public class CS41 : GameWindow
  {
    private Viewport viewport;
    private Entity kyubei;
    private VertexTweenData qbVertexData;
    private float[] tweenFactors, tweenTargets;
    private Vector2[] sourceScreenPositions;

    private float tweenDecay = 0.5f;
    private float tweenDistAdjustment = 0.0001f;
    private float tweenMouseMotionFactor = 0.1f;
    private float tweenMouseDistanceFactor = 200f;
    private float tweenSmoothing = 5f;
    
    protected override void OnLoad(EventArgs e)
    {
      GL.ClearColor(Color.Black);

      viewport = new Viewport(new Rectangle(0, 0, 640, 480));
      Matrix4.CreateOrthographic(640f, 480f, -1f, 1f, out viewport.projectionMatrix);
      viewport.viewMatrix = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
      
      VertexTweenData.Setup();
      qbVertexData = new VertexTweenData();
      if (qbVertexData.LoadFromLineDrawFiles("Lab3/dog.rel", "Lab3/qb.rel") == false)
      {
        Console.WriteLine("Error loading files");
        Exit();
      }

      tweenFactors = qbVertexData.TweenFactors;
      tweenTargets = new float[tweenFactors.Length];

      qbVertexData.Update(); // uploads the model to a vertex buffer on the GPU

      Material material = Material.Cache["TweenLines"];
      material["color"] = Vector3.One;

      kyubei = new Entity();
      kyubei.VertexData = qbVertexData;
      kyubei.Material = material;
      kyubei.transform = new Transform(Matrix4.CreateTranslation(-325f, -275f, 0f)
        * Matrix4.Scale(1f, -1f, 1f));

      VertexTweenData.VertexLayout[] data = qbVertexData.Data;
      sourceScreenPositions = new Vector2[data.Length];
      /*Matrix4 MVP = viewport.projectionMatrix
        * viewport.viewMatrix
        * kyubei.transform.matrix;*/
      Matrix4 MVP = Matrix4.Identity;
      for (int i = 0; i < data.Length; i++)
      {
        sourceScreenPositions[i] = Vector4.Transform(new Vector4(data[i].position1, 1f), MVP).Xy;
      }

      Mouse.Move += OnMouseMove;
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);

      float frameTime = (float)e.Time;

      ProcessInput(frameTime);

      float lerpBlend = frameTime * tweenSmoothing;
      for (int i = 0; i < tweenTargets.Length; i++)
      {
        tweenFactors[i] = (tweenFactors[i] + lerpBlend * (tweenTargets[i] - tweenFactors[i])).Clamp(0f, 1f);
      }

      qbVertexData.Update();
      for (int i = 0; i < tweenTargets.Length; i++)
      {
        tweenTargets[i] = Math.Max(tweenTargets[i] - frameTime * tweenDecay, 0f);
      }
    }

    private void OnMouseMove(object sender, MouseMoveEventArgs e)
    {
      // TODO: make a proper conversion function (in Viewport?)
      Vector2 mousePos = new Vector2(e.X, e.Y);
      float dist, tween, distFactor;
      distFactor = tweenMouseDistanceFactor
        + new Vector2(e.XDelta, e.YDelta).LengthSquared * tweenMouseMotionFactor;
      for (int i = 0; i < tweenTargets.Length; i++)
      {
        dist = distFactor / Vector2.Subtract(mousePos, sourceScreenPositions[i]).LengthSquared - tweenDistAdjustment;
        dist = dist.Clamp(0f, 1f);
        tween = tweenTargets[i];
        tween = Math.Min(tween + dist, 1f);
        tweenTargets[i] = tween;
      }
    }

    private void ProcessInput(float frameTime)
    {
      if (Keyboard[Key.Escape])
      {
        Exit();
      }

      if (Keyboard[Key.Space])
      {
        for (int i = 0; i < tweenTargets.Length; i++)
        {
          tweenTargets[i] = 1f;
        }
      }
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);

      GL.Clear(ClearBufferMask.ColorBufferBit);

      viewport.Draw(); // right now just makes the viewport active

      kyubei.Draw();

      this.SwapBuffers();
    }

    /// <summary>
    /// Main.
    /// </summary>
    [STAThread]
    public static void Main()
    {
      using (CS41 win = new CS41())
      {
        win.ClientSize = new Size(640, 480);
        win.Title = "Case Study 4.1 - Tweening (By Alec Emmett)";
        win.Run(30);
      }
    }
  }
}

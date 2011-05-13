using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TestProject.Objects;

namespace TestProject.Lab1
{
  public class HW234 : GameWindow
  {
    // Normally, Entitys would be in a Scene class, but for now...
    private Entity house;

    private Viewport viewport;

    private Random rng = new Random();
    private Vector2 peakA, peakB, scaleA, scaleB;

    protected override void OnLoad(EventArgs e)
    {
      GL.ClearColor(Color.Black);

      viewport = new Viewport(new Rectangle(0, 0, 250, 250));
      Matrix4.CreateOrthographicOffCenter(-2f, 2f, -2f, 2f, -1f, 1f, out viewport.projectionMatrix);
      viewport.viewMatrix = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);

      viewport.Draw();

      VertexPosData.Setup();

      VertexPosData.VertexLayout[] data = new VertexPosData.VertexLayout[]
      {
        new VertexPosData.VertexLayout(new Vector3(0f, 0f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0f, 0.6f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.15f, 0.72f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.15f, 0.95f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.25f, 0.95f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.25f, 0.8f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.5f, 1f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(1f, 0.6f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(1f, 0f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0f, 0f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.15f, 0f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.15f, 0.3f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.4f, 0.3f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.4f, 0f, 0f)),

        new VertexPosData.VertexLayout(new Vector3(0.6f, 0.25f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.6f, 0.45f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.75f, 0.45f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.75f, 0.25f, 0f)),
        new VertexPosData.VertexLayout(new Vector3(0.6f, 0.25f, 0f))
      };
      int[] firsts = new int[] { 0, 14 };
      int[] counts = new int[] { 14, 5 };

      VertexPosData vertexData = new VertexPosData();
      vertexData.Load(data, BeginMode.LineStrip, firsts, counts);
      vertexData.Update(); // uploads the model to a vertex buffer on the GPU
      
      Material material = Material.LoadMaterial("Default"); // loads a material (currently just hardcoded)
      
      house = new Entity();
      house.VertexData = vertexData;
      house.Material = material;
      material["color"] = Vector3.One;

      house.transform = new Transform(Matrix4.Identity);

      peakB.X = (float)rng.NextDouble() * 2f - 1f;
      peakB.Y = (float)rng.NextDouble() * 3f - 1f;
      scaleB.X = (float)rng.NextDouble() * 4f - 2f;
      scaleB.Y = (float)rng.NextDouble() * 4f - 2f;
    }

    float t;

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Clear(ClearBufferMask.ColorBufferBit);

      // this is supposed to also draw the scene it's attached to, but I haven't created a Scene object yet,
      // plus, the point of the assignment is the DrawHouse function anyway...
      viewport.Draw();

      if (t > 1f)
      {
        t = 0;
        peakA = peakB;
        scaleA = scaleB;

        peakB.X = (float)rng.NextDouble() * 3f - 1f;
        peakB.Y = (float)rng.NextDouble() * 4f - 1f;
        scaleB.X = (float)rng.NextDouble() * 4f - 2f;
        scaleB.Y = (float)rng.NextDouble() * 4f - 2f;
      }

      t += (float)e.Time;
      
      Vector2 peak, scale;
      Vector2.Lerp(ref peakA, ref peakB, t, out peak);
      Vector2.Lerp(ref scaleA, ref scaleB, t, out scale);

      DrawHouseAt(peak, scale);

      this.SwapBuffers();
    }

    private void DrawHouseAt(Vector2 peak, Vector2 scale)
    {
      // the model's origin is the lower-left corner, because it was easier for me to
      // figure out and type in the numbers that way. Adjust so peak is the origin instead.
      peak += new Vector2(-0.5f, -1f);

      Matrix4 transform = Matrix4.Scale(new Vector3(scale.X, scale.Y, 1f)) * Matrix4.CreateTranslation(new Vector3(peak.X, peak.Y, 0f));
      house.transform.matrix = transform;

      house.Draw();
    }
    
    /// <summary>
    /// Main.
    /// </summary>
    [STAThread]
    public static void Main()
    {
      using (HW234 win = new HW234())
      {
        win.ClientSize = new Size(250, 250);
        win.Title = "Problem 2.3.4 - House";
        win.Run();
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TestProject.Objects;

namespace TestProject.Lab1
{
  public class HW235 : GameWindow
  {
    // Normally, Entitys would be in a Scene class, but for now...
    private List<Entity> diamonds;
    private VertexData squareVertexData;
    private Material material;

    private Viewport viewport;

    private Random rng = new Random();

    private const float fallingSpeedFactor = 20f;
    private const float minFallingSpeed = 0.5f;
    private const float coordScale = 4f / 250f;
    private const float diamondsPerArea = 6.25f;

    private float width, height;

    protected override void OnLoad(EventArgs e)
    {
      GL.ClearColor(Color.Black);

      viewport = new Viewport(new Rectangle(0, 0, 250, 250));
      Matrix4.CreateOrthographicOffCenter(-2f, 2f, -2f, 2f, -1f, 1f, out viewport.projectionMatrix);
      viewport.viewMatrix = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);

      VertexPosData.Setup();

      VertexPosData vertexData = new VertexPosData();
      vertexData.LoadTestModel(); // temp code: loads a square into system memory
      vertexData.Update(); // uploads the model to a vertex buffer on the GPU
      squareVertexData = vertexData;

      material = Material.GetMaterial("Default"); // loads a material (currently just hardcoded)
      material["color"] = Vector3.One;
      
      diamonds = new List<Entity>(100);

      Entity diamond;
      Vector3 scale, pos;
      float temp;
      for (int i = 0; i < 100; i++)
      {
        diamond = new Entity();
        diamond.VertexData = vertexData;
        diamond.Material = material;

        // the square model (from the LoadTestModel method) is a 2x2 square with center at origin.
        // Let's rotate it 45* to make it a diamond, and give it a random position/scale
        temp = (float)rng.NextDouble() * 0.15f;
        scale = new Vector3(temp, temp, 1f);
        pos = new Vector3(
          (float)rng.NextDouble() * 6f - 3f,
          (float)rng.NextDouble() * 4f + 2f,
          0f);
        diamond.transform = new Transform(
          Matrix4.Scale(scale)
          * Matrix4.CreateRotationZ((float)Math.PI / 4.0f)
          * Matrix4.CreateTranslation(pos));

        diamonds.Add(diamond);
      }
    }

    protected override void OnResize(EventArgs e)
    {
      viewport.ViewportRect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

      width = ClientSize.Width * coordScale;
      height = ClientSize.Height * coordScale;
      Matrix4.CreateOrthographicOffCenter(-width/2f, width/2f, -height/2f, height/2f, -1f, 1f, out viewport.projectionMatrix);

      Entity diamond;
      int newCount = (int)(width * height * diamondsPerArea);
      int oldCount = diamonds.Count;
      if (newCount < oldCount)
      {
        diamonds.RemoveRange(newCount, oldCount - newCount);
        return;
      }

      for (int i = oldCount; i < newCount; i++)
      {
        diamond = new Entity();
        diamond.VertexData = squareVertexData;
        diamond.Material = material;
        diamond.transform = new Transform();
        RespawnDiamond(diamond);
        diamonds.Add(diamond);
      }
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Clear(ClearBufferMask.ColorBufferBit);

      // this is supposed to also draw the scene it's attached to, but I haven't created a Scene object yet,
      // plus, the point of the assignment is to do something like the loop I have below anyway...
      viewport.Draw();

      foreach (Entity diamond in diamonds)
      {
        diamond.Draw();
        diamond.transform.PosY -= (minFallingSpeed +
          fallingSpeedFactor * diamond.transform.ScaleX) * (float)e.Time;
        if (diamond.transform.PosY  < -height/2f - 1f)
        {
          RespawnDiamond(diamond);
        }
      }

      this.SwapBuffers();
    }

    private void RespawnDiamond(Entity diamond)
    {
      Vector3 scale, pos;
      float temp;
      temp = (float)rng.NextDouble() * 0.15f;
      scale = new Vector3(temp, temp, 1f);
      pos = new Vector3(
        (float)rng.NextDouble() * (width + 2f) - (width/2 + 1f),
        (float)rng.NextDouble() * (height + 2f) + (height / 2 + 1f),
        0f);
      diamond.transform.matrix = Matrix4.Scale(scale)
        * Matrix4.CreateRotationZ((float)Math.PI / 4.0f)
        * Matrix4.CreateTranslation(pos);
    }
    
    /// <summary>
    /// Main.
    /// </summary>
    [STAThread]
    public static void Main()
    {
      using (HW235 win = new HW235())
      {
        win.ClientSize = new Size(250, 250);
        win.Title = "Problem 2.3.5 - Diamonds";
        win.Run();
      }
    }
  }
}

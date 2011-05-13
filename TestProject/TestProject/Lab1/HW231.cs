using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TestProject.Objects;

namespace TestProject.Lab1
{
  public class HW231 : GameWindow
  {
    // Normally, Entitys would be in a Scene class, but for now...
    private Entity square;

    private Viewport viewport;

    protected override void OnLoad(EventArgs e)
    {
      GL.ClearColor(Color.Black);

      viewport = new Viewport(new Rectangle(0, 0, 250, 250));
      Matrix4.CreateOrthographicOffCenter(0f, 8f, 0f, 8f, -1f, 1f, out viewport.projectionMatrix);
      viewport.viewMatrix = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);

      viewport.Draw();

      VertexPosData.Setup();

      VertexPosData vertexData = new VertexPosData();
      vertexData.LoadTestModel(); // temp code: loads a square into system memory
      vertexData.Update(); // uploads the model to a vertex buffer on the GPU
      
      Material material = Material.Cache["Default"]; // loads a material (currently just hardcoded)
      
      square = new Entity();
      square.VertexData = vertexData;
      square.Material = material;

      // the square model (from the LoadTestModel method) is a 2x2 square with center at origin.
      // Let's make it so it's 1x1 and the lower-left corner is at the origin
      square.transform = new Transform(Matrix4.Scale(0.5f) * Matrix4.CreateTranslation(0.5f, 0.5f, 0f));
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Clear(ClearBufferMask.ColorBufferBit);

      // this is supposed to also draw the scene it's attached to, but I haven't created a Scene object yet,
      // plus, the point of the assignment is to do something like the loop I have below anyway...
      viewport.Draw();

      Matrix4 originalMatrix = square.transform.matrix;
      
      for (int i = 0; i < 64; i++)
      {
        if ((i + i/8) % 2 == 0)
        {
          square["color"] = new Vector3(0f, 0f, 1f);
        }
        else
        {
          square["color"] = new Vector3(1f, 1f, 0f);
        }

        square.Draw();

        // Translations are reversed until I make the conveience properties
        // re-reverse... or something, them
        if (i % 8 == 7)
        {
          square.transform.PosX -= 7f;
          square.transform.PosY += 1f;
        }
        else
        {
          square.transform.PosX += 1f;
        }
      }

      square.transform.matrix = originalMatrix;

      this.SwapBuffers();
    }
    
    /// <summary>
    /// Main.
    /// </summary>
    [STAThread]
    public static void Main()
    {
      using (HW231 win = new HW231())
      {
        win.ClientSize = new Size(250, 250);
        win.Title = "Problem 2.3.1 - Checkerboard";
        win.Run();
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using TestProject.Objects;

namespace TestProject.Lab2
{
  public class CS33 : GameWindow
  {
    private LinkedList<SquishableEntity> squishableObjects = new LinkedList<SquishableEntity>();
    private VertexPosData houseVertexData;
    private Material material, redmat;

    private Viewport viewport;
    private float viewportXTarget = 0f;
    private const float viewportSmoothFactor = 0.8f;

    private MovingEntity dino;
    private Vector3 targetVelocity;
    private const float xCenterOffset = 100f;
    private int score = 0;

    private Random rng = new Random();
    private float nextObjectXPos;
    private const float objXIntervalMin = 30f, objXFreqLinear = 20f, objXFreqQuad = 50f;

    protected override void OnLoad(EventArgs e)
    {
      GL.ClearColor(Color.Black);

      viewport = new Viewport(new Rectangle(0, 0, 640, 480));
      // TODO: switch to projection?
      Matrix4.CreateOrthographic(640f, 480f, -1f, 1000f, out viewport.projectionMatrix);
      // change camera position so it's looking down slightly
      //viewport.viewMatrix = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
      viewport.viewMatrix = Matrix4.LookAt(Vector3.Zero, new Vector3(0f, 150f, -200f), Vector3.UnitY);

      VertexPosData.Setup();

      VertexPosData dinoVertexData = new VertexPosData();
      dinoVertexData.LoadFromPolylineFile("Lab2/dino.dat");
      dinoVertexData.Update(); // uploads the model to a vertex buffer on the GPU

      houseVertexData = new VertexPosData();
      houseVertexData.LoadFromPolylineFile("Lab2/house.dat");
      houseVertexData.Update();

      material = Material.Cache["Default"]; // loads a material (currently just hardcoded)
      material["color"] = Vector3.One;

      redmat = material.CloneInstance();
      redmat["color"] = Vector3.UnitX;

      dino = new MovingEntity();
      dino.VertexData = dinoVertexData;
      dino.Material = material;
      dino.transform = new Transform(Matrix4.Scale(-0.5f, 0.5f, 1f)
        * Matrix4.CreateTranslation(xCenterOffset, 0f, -200f));
      this.RenderFrame += dino.OnUpdatePosition;

      nextObjectXPos = -640f;
      while (nextObjectXPos < 640f)
      {
        SpawnNewSqusihable(true);
      }
    }

    private const float moveSpeed = 200f;
    private const float moveSmoothFactor = 5f;
    private const float xSheerFactor = -1/500f;
    private const float xSheerSmoothFactor = 1f;

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);

      float frameTime = (float)e.Time;

      ProcessInput(frameTime);

      // TODO: lerp (slerp?) rotation based on velocity
      // - only 6 target orientations
      // - put in the render frame code? just the lerping part?

      // acceleration effect
      Vector3 targetSheer = new Vector3(1f, (targetVelocity.X - dino.velocity.X) * xSheerFactor, 0f);
      dino.transform.ShearX = Vector3.Lerp(dino.transform.ShearX, targetSheer, xSheerSmoothFactor * frameTime);

      // check if position near screen boundaries (scroll viewport)
      // - when scrolling, forget houses more than a certain distance
      //   offscreen, and create new houses just before they come onscreen
      float dinoCenterXPos = dino.transform.PosX + dino.transform.ScaleX * xCenterOffset * 2;
      if (dinoCenterXPos + viewport.viewMatrix.M41 < -200f)
      { // scroll left TODO: upon reaching left edge, increase buffer
        viewportXTarget = -dinoCenterXPos;
        if (squishableObjects.Last.Value.transform.PosX + viewport.viewMatrix.M41 > 960f)
        {
          squishableObjects.RemoveLast();
        }

        if (nextObjectXPos + viewport.viewMatrix.M41 > 0f)
        {
          float rand = (float)rng.NextDouble();
          nextObjectXPos = squishableObjects.First.Value.transform.PosX;
          nextObjectXPos -= rand * rand * objXFreqQuad + rand * objXFreqLinear + objXIntervalMin;
        }

        if (nextObjectXPos + viewport.viewMatrix.M41 > -640f)
        {
          SpawnNewSqusihable(false);
        }
      }
      else if (dinoCenterXPos + viewport.viewMatrix.M41 > 200f)
      { // scroll right
        viewportXTarget = -dinoCenterXPos;
        if (nextObjectXPos + viewport.viewMatrix.M41 < 0f)
        {
          float rand = (float)rng.NextDouble();
          nextObjectXPos = squishableObjects.Last.Value.transform.PosX;
          nextObjectXPos += rand * rand * objXFreqQuad + rand * objXFreqLinear + objXIntervalMin;
        }

        if (nextObjectXPos + viewport.viewMatrix.M41 < 640f)
        {
          SpawnNewSqusihable(true);
        }
      }

      CheckForCollisions();
    }

    private void CheckForCollisions()
    {
      // too lazy to implement bounding boxes and collision, just use simple distance metrics
      Vector3 dir, sePos, dinoPos = dino.transform.Pos;
      dinoPos.X = dino.transform.PosX + dino.transform.ScaleX * xCenterOffset * 2;
      LinkedListNode<SquishableEntity> temp, node = squishableObjects.First;
      SquishableEntity se;
      int oldScore = score;
      while (node.Next != null)
      {
        se = node.Value;
        sePos = se.transform.Pos;
        node = node.Next;
        if (se.Squashed == true
          || sePos.X < dinoPos.X - 120f
          || sePos.X > dinoPos.X + 50f
          || sePos.Z < dinoPos.Z - 50f
          || sePos.Z > dinoPos.Z + 50f
          || dinoPos.Y > 40f)
        {
          continue;
        }

        if (dinoPos.Y > 5f)
        {
          se.SquishFromTop();
          score += 50;
          continue;
        }

        Vector3.Subtract(ref sePos, ref dinoPos, out dir);
        dir.NormalizeFast();
        if (dir.X < -.7f)
        {
          se.SquishFromSide(false);
        }
        else if (dir.X > 0.7f)
        {
          se.SquishFromSide(true);
        }
        else
        {
          se.SquishFromTop();
        }

        score += 25;
      }

      if (score != oldScore)
      {
        Console.WriteLine(string.Format("Score: {0}", score));
      }
    }

    private void SpawnNewSqusihable(bool direction)
    { // true is right (and right is true)
      float zpos = (float)rng.NextDouble() * -400f;

      SquishableEntity house = new SquishableEntity(this);
      house.VertexData = houseVertexData;
      house.Material = material;
      house.transform = new Transform(Matrix4.Scale(50f) * Matrix4.CreateTranslation(nextObjectXPos, 0f, zpos));

      if (direction)
      {
        squishableObjects.AddLast(house);
      }
      else
      {
        squishableObjects.AddFirst(house);
      }

      float rand = (float)rng.NextDouble();
      nextObjectXPos += (direction ? 1 : -1)
        * (rand * rand * objXFreqQuad + rand * objXFreqLinear + objXIntervalMin);
    }

    private Vector3 minPos = new Vector3(float.MinValue, 0f, -400f),
      maxPos = new Vector3(float.MaxValue, 300f, 0f);

    private float timeSinceJumpKey = 1f;
    private const float jumpVelocity = 350f;
    private const float gravity = 800f;

    private void ProcessInput(float frameTime)
    {
      if (Keyboard[Key.Escape])
      {
        Exit();
      }

      if (Keyboard[Key.Space])
      {
        timeSinceJumpKey = 0f;
      }

      float ypos = dino.transform.PosY;

      if (ypos == 0f)
      {
        targetVelocity = Vector3.Zero;

        if (Keyboard[Key.A])
        {
          targetVelocity += new Vector3(-moveSpeed, 0f, 0f);
        }

        if (Keyboard[Key.D])
        {
          targetVelocity += new Vector3(moveSpeed, 0f, 0f);
        }

        if (Keyboard[Key.S] && dino.transform.PosZ > -390f)
        {
          targetVelocity += new Vector3(0f, 0f, -moveSpeed);
        }

        if (Keyboard[Key.W] && dino.transform.PosZ < -10f)
        {
          targetVelocity += new Vector3(0f, 0f, moveSpeed);
        }

        Vector3.Lerp(ref dino.velocity, ref targetVelocity, frameTime * moveSmoothFactor, out dino.velocity);
        dino.transform.PosZ = Clamp(dino.transform.PosZ, -400f, 0f);

        if (timeSinceJumpKey < 0.2f)
        {
          dino.velocity.Y = jumpVelocity;
        }
      }
      else if (ypos > 0)
      {
        dino.velocity.Y -= gravity * frameTime;
        dino.transform.PosZ = Clamp(dino.transform.PosZ, -400f, 0f);
      }
      else if (ypos + dino.velocity.Y * frameTime < 0)
      {
        dino.velocity.Y = 0f;
        dino.transform.PosY = 0f;
        // TODO: landing effect(s)
        //  squish landing effect
        //  - a timestamp and some lerping
        //  camera shake?
        //  destruction of houses within a certain radius?
      }

      timeSinceJumpKey += frameTime;
    }

    private float Clamp(float input, float min, float max)
    {
      return Math.Min(Math.Max(input, min), max);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);

      GL.Clear(ClearBufferMask.ColorBufferBit);

      // clear to sky blue, draw a green rect for the ground
      // - textured?!?
      //  - overkill!

      viewport.viewMatrix.M41 += (viewportXTarget - viewport.viewMatrix.M41) * viewportSmoothFactor * (float)e.Time;

      viewport.Draw(); // right now just makes the viewport active

      dino.Draw();
      foreach (Entity obj in squishableObjects)
      {
        obj.Draw();
      }

      this.SwapBuffers();
    }

    /// <summary>
    /// Main.
    /// </summary>
    [STAThread]
    public static void Main()
    {
      using (CS33 win = new CS33())
      {
        win.ClientSize = new Size(640, 480);
        win.Title = "Case Study 3.3 - Dinosaur";
        win.Run(30);
      }
    }

    private class SquishableEntity : Entity
    {
      private CS33 superThis;
      public bool Squashed { get; private set; }
      private float animTime = 0f;
      private Vector3 startSheerX, endShearX;
      private Vector3 startScale, endScale;

      public SquishableEntity(CS33 super)
      {
        superThis = super;
      }

      public void SquishFromTop()
      {
        Squashed = true;
        endScale = startScale = this.transform.Scale;
        endScale.X *= 1.5f;
        endScale.Y = 0f;
        superThis.UpdateFrame += TopSquishUpdate;
        Material = superThis.redmat;
      }

      protected void TopSquishUpdate(object sender, FrameEventArgs e)
      {
        animTime += (float)e.Time * 5f;
        if (animTime > 1f)
        {
          superThis.squishableObjects.Remove(this);
          superThis.UpdateFrame -= TopSquishUpdate;
          return;
        }

        this.transform.Scale = Vector3.Lerp(startScale, endScale, animTime);
      }

      public void SquishFromSide(bool right)
      {
        Squashed = true;
        superThis.UpdateFrame += SideSqushUpdate;
        Material = superThis.redmat;

        endScale = startScale = this.transform.Scale;
        endScale.Y = 0f;
        endShearX = startSheerX = this.transform.ShearX;
        if (right)
        {
          endShearX.Y = 50f;
        }
        else
        {
          endShearX.Y = -50f;
        }
      }

      protected void SideSqushUpdate(object sender, FrameEventArgs e)
      {
        animTime += (float)e.Time * 5f;
        if (animTime > 1f)
        {
          superThis.squishableObjects.Remove(this);
          superThis.UpdateFrame -= SideSqushUpdate;
          return;
        }

        this.transform.Scale = Vector3.Lerp(startScale, endScale, animTime);
        this.transform.ShearX = Vector3.Lerp(startSheerX, endShearX, animTime);
      }
    }
  }
}

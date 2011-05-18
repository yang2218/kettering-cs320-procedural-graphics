
/* Copyright (c) Mark J. Kilgard, 1994. */

/**
 * (c) Copyright 1993, Silicon Graphics, Inc.
 * ALL RIGHTS RESERVED
 * Permission to use, copy, modify, and distribute this software for
 * any purpose and without fee is hereby granted, provided that the above
 * copyright notice appear in all copies and that both the copyright notice
 * and this permission notice appear in supporting documentation, and that
 * the name of Silicon Graphics, Inc. not be used in advertising
 * or publicity pertaining to distribution of the software without specific,
 * written prior permission.
 *
 * THE MATERIAL EMBODIED ON THIS SOFTWARE IS PROVIDED TO YOU "AS-IS"
 * AND WITHOUT WARRANTY OF ANY KIND, EXPRESS, IMPLIED OR OTHERWISE,
 * INCLUDING WITHOUT LIMITATION, ANY WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL SILICON
 * GRAPHICS, INC.  BE LIABLE TO YOU OR ANYONE ELSE FOR ANY DIRECT,
 * SPECIAL, INCIDENTAL, INDIRECT OR CONSEQUENTIAL DAMAGES OF ANY
 * KIND, OR ANY DAMAGES WHATSOEVER, INCLUDING WITHOUT LIMITATION,
 * LOSS OF PROFIT, LOSS OF USE, SAVINGS OR REVENUE, OR THE CLAIMS OF
 * THIRD PARTIES, WHETHER OR NOT SILICON GRAPHICS, INC.  HAS BEEN
 * ADVISED OF THE POSSIBILITY OF SUCH LOSS, HOWEVER CAUSED AND ON
 * ANY THEORY OF LIABILITY, ARISING OUT OF OR IN CONNECTION WITH THE
 * POSSESSION, USE OR PERFORMANCE OF THIS SOFTWARE.
 *
 * US Government Users Restricted Rights
 * Use, duplication, or disclosure by the Government is subject to
 * restrictions set forth in FAR 52.227.19(c)(2) or subparagraph
 * (c)(1)(ii) of the Rights in Technical Data and Computer Software
 * clause at DFARS 252.227-7013 and/or in similar or successor
 * clauses in the FAR or the DOD or NASA FAR Supplement.
 * Unpublished-- rights reserved under the copyright laws of the
 * United States.  Contractor/manufacturer is Silicon Graphics,
 * Inc., 2011 N.  Shoreline Blvd., Mountain View, CA 94039-7311.
 *
 * OpenGL(TM) is a trademark of Silicon Graphics, Inc.
 */
/**
 *  teapots.c
 *  This program demonstrates lots of material properties.
 *  A single light source illuminates the objects.
 */

/* Ported to C#/OpenTK */

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace TestProject
{
  public class TeapotDemo : GameWindow
  {
    /// <summary>
    /// Initialize depth buffer, projection matrix, light source, and lighting
    /// model.  Do not specify a material property here.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnLoad(EventArgs e)
    { 
      float[] ambient = {0.0f, 0.0f, 0.0f, 1.0f};
      float[] diffuse = {1.0f, 1.0f, 1.0f, 1.0f};
      float[] position = {0.0f, 3.0f, 3.0f, 0.0f};

      float[] lmodel_ambient = {0.2f, 0.2f, 0.2f, 1.0f};
      float[] local_view = {0.0f};

      GL.Light(LightName.Light0, LightParameter.Ambient, ambient);
      GL.Light(LightName.Light0, LightParameter.Diffuse, diffuse);
      GL.Light(LightName.Light0, LightParameter.Position, position);
      GL.LightModel(LightModelParameter.LightModelAmbient, lmodel_ambient);
      GL.LightModel(LightModelParameter.LightModelLocalViewer, local_view);

      GL.FrontFace(FrontFaceDirection.Cw);
      GL.Enable(EnableCap.Lighting);
      GL.Enable(EnableCap.Light0);
      GL.Enable(EnableCap.AutoNormal);
      GL.Enable(EnableCap.Normalize);
      GL.Enable(EnableCap.DepthTest);
      GL.DepthFunc(DepthFunction.Less);

      GL.ClearColor(Color.Black);
    }

    protected override void OnResize(EventArgs e)
    {
      GL.Viewport(ClientRectangle);
      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadIdentity();
      if (ClientSize.Width <= ClientSize.Height)
        GL.Ortho(0.0, 16.0, 0.0, 16.0 * (float)ClientSize.Height / (float)ClientSize.Width,
          -10.0, 10.0);
      else
        GL.Ortho(0.0, 16.0 * (float)ClientSize.Width / (float)ClientSize.Height, 0.0, 16.0,
          -10.0, 10.0);
      GL.MatrixMode(MatrixMode.Modelview);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      if (Keyboard[Key.Escape])
      {
        Exit();
      }
    }

    /// <summary>
    /// Move object into position.  Use 3rd through 12th parameters to specify the
    /// material property.  Draw a teapot.
    /// </summary>
    private void RenderTeapot(float x, float y,
      float ambr, float ambg, float ambb,
      float difr, float difg, float difb,
      float specr, float specg, float specb, float shine)
    {
      float[] mat = new float[4];

      GL.PushMatrix();
      GL.Translate(x, y, 0.0f);
      mat[0] = ambr;
      mat[1] = ambg;
      mat[2] = ambb;
      mat[3] = 1.0f;
      GL.Material(MaterialFace.Front, MaterialParameter.Ambient, mat);
      mat[0] = difr;
      mat[1] = difg;
      mat[2] = difb;
      GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, mat);
      mat[0] = specr;
      mat[1] = specg;
      mat[2] = specb;
      GL.Material(MaterialFace.Front, MaterialParameter.Specular, mat);
      GL.Material(MaterialFace.Front, MaterialParameter.Shininess, shine * 128.0f);
      Teapot.DrawSolidTeapot(1f);
      GL.PopMatrix();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
      RenderTeapot(2.0f, 17.0f, 0.0215f, 0.1745f, 0.0215f,
        0.07568f, 0.61424f, 0.07568f, 0.633f, 0.727811f, 0.633f, 0.6f);
      RenderTeapot(2.0f, 14.0f, 0.135f, 0.2225f, 0.1575f,
        0.54f, 0.89f, 0.63f, 0.316228f, 0.316228f, 0.316228f, 0.1f);
      RenderTeapot(2.0f, 11.0f, 0.05375f, 0.05f, 0.06625f,
        0.18275f, 0.17f, 0.22525f, 0.332741f, 0.328634f, 0.346435f, 0.3f);
      RenderTeapot(2.0f, 8.0f, 0.25f, 0.20725f, 0.20725f,
        1f, 0.829f, 0.829f, 0.296648f, 0.296648f, 0.296648f, 0.088f);
      RenderTeapot(2.0f, 5.0f, 0.1745f, 0.01175f, 0.01175f,
        0.61424f, 0.04136f, 0.04136f, 0.727811f, 0.626959f, 0.626959f, 0.6f);
      RenderTeapot(2.0f, 2.0f, 0.1f, 0.18725f, 0.1745f,
        0.396f, 0.74151f, 0.69102f, 0.297254f, 0.30829f, 0.306678f, 0.1f);
      RenderTeapot(6.0f, 17.0f, 0.329412f, 0.223529f, 0.027451f,
        0.780392f, 0.568627f, 0.113725f, 0.992157f, 0.941176f, 0.807843f,
        0.21794872f);
      RenderTeapot(6.0f, 14.0f, 0.2125f, 0.1275f, 0.054f,
        0.714f, 0.4284f, 0.18144f, 0.393548f, 0.271906f, 0.166721f, 0.2f);
      RenderTeapot(6.0f, 11.0f, 0.25f, 0.25f, 0.25f,
        0.4f, 0.4f, 0.4f, 0.774597f, 0.774597f, 0.774597f, 0.6f);
      RenderTeapot(6.0f, 8.0f, 0.19125f, 0.0735f, 0.0225f,
        0.7038f, 0.27048f, 0.0828f, 0.256777f, 0.137622f, 0.086014f, 0.1f);
      RenderTeapot(6.0f, 5.0f, 0.24725f, 0.1995f, 0.0745f,
        0.75164f, 0.60648f, 0.22648f, 0.628281f, 0.555802f, 0.366065f, 0.4f);
      RenderTeapot(6.0f, 2.0f, 0.19225f, 0.19225f, 0.19225f,
        0.50754f, 0.50754f, 0.50754f, 0.508273f, 0.508273f, 0.508273f, 0.4f);
      RenderTeapot(10.0f, 17.0f, 0.0f, 0.0f, 0.0f, 0.01f, 0.01f, 0.01f,
        0.50f, 0.50f, 0.50f, .25f);
      RenderTeapot(10.0f, 14.0f, 0.0f, 0.1f, 0.06f, 0.0f, 0.50980392f, 0.50980392f,
        0.50196078f, 0.50196078f, 0.50196078f, .25f);
      RenderTeapot(10.0f, 11.0f, 0.0f, 0.0f, 0.0f,
        0.1f, 0.35f, 0.1f, 0.45f, 0.55f, 0.45f, .25f);
      RenderTeapot(10.0f, 8.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f, 0.0f,
        0.7f, 0.6f, 0.6f, .25f);
      RenderTeapot(10.0f, 5.0f, 0.0f, 0.0f, 0.0f, 0.55f, 0.55f, 0.55f,
        0.70f, 0.70f, 0.70f, .25f);
      RenderTeapot(10.0f, 2.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.0f,
        0.60f, 0.60f, 0.50f, .25f);
      RenderTeapot(14.0f, 17.0f, 0.02f, 0.02f, 0.02f, 0.01f, 0.01f, 0.01f,
        0.4f, 0.4f, 0.4f, .078125f);
      RenderTeapot(14.0f, 14.0f, 0.0f, 0.05f, 0.05f, 0.4f, 0.5f, 0.5f,
        0.04f, 0.7f, 0.7f, .078125f);
      RenderTeapot(14.0f, 11.0f, 0.0f, 0.05f, 0.0f, 0.4f, 0.5f, 0.4f,
        0.04f, 0.7f, 0.04f, .078125f);
      RenderTeapot(14.0f, 8.0f, 0.05f, 0.0f, 0.0f, 0.5f, 0.4f, 0.4f,
        0.7f, 0.04f, 0.04f, .078125f);
      RenderTeapot(14.0f, 5.0f, 0.05f, 0.05f, 0.05f, 0.5f, 0.5f, 0.5f,
        0.7f, 0.7f, 0.7f, .078125f);
      RenderTeapot(14.0f, 2.0f, 0.05f, 0.05f, 0.0f, 0.5f, 0.5f, 0.4f,
        0.7f, 0.7f, 0.04f, .078125f);

      this.SwapBuffers();
    }

    public static void Main()
    {
      using (TeapotDemo win = new TeapotDemo())
      {
        win.Title = "C#/OpenTK Teapot Demo";
        win.ClientSize = new Size(1024, 768);
        win.Run();
      }
    }
  }
}

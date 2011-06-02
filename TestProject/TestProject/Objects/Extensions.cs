using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace TestProject.Objects
{
  public static class Extensions
  {
    public static float Clamp(this float me, float min, float max)
    {
      return Math.Min(Math.Max(me, min), max);
    }

    public static Matrix4 ToMatrix(this Quaternion rot)
    {
      Matrix4 ret;
      rot.ToMatrix(out ret);
      return ret;
    }

    public static void ToMatrix(this Quaternion rot, out Matrix4 mat)
    {
      // TODO: pretty sure this is broken somewhere

      float xx = rot.X * rot.X;
      float xy = rot.X * rot.Y;
      float xz = rot.X * rot.Z;
      float xw = rot.X * rot.W;

      float yy = rot.Y * rot.Y;
      float yz = rot.Y * rot.Z;
      float yw = rot.Y * rot.W;

      float zz = rot.Z * rot.Z;
      float zw = rot.Z * rot.W;
      mat = new Matrix4(
        1 - 2 * (yy + zz),
            2 * (xy - zw),
            2 * (xz + yw),
        0,
            2 * (xy + zw),
        1 - 2 * (xx + zz),
            2 * (yz - xw),
        0,
            2 * (xz - yw),
            2 * (yz + xw),
        1 - 2 * (xx + yy),
        0,
        0, 0, 0, 1
      );
    }
  }
}

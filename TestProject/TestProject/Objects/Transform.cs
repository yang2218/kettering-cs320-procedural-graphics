using System;
using OpenTK;

namespace TestProject.Objects
{
  public class Transform
  {
    public Matrix4 matrix;
    // TODO: Quaternion for rotation?

    public Transform()
    {
      matrix = Matrix4.Identity;
    }

    public Transform(Matrix4 matrix)
    {
      this.matrix = matrix;
    }

    // TODO: ref/out methods for these properties?

    public Vector3 Pos
    {
      get
      {
        return new Vector3(matrix.Row3);
      }
      set
      {
        matrix.Row3 = new Vector4(value, 1f);
      }
    }
    public float PosX
    {
      get
      {
        return matrix.M41;
      }
      set
      {
        matrix.M41 = value;
      }
    }
    public float PosY
    {
      get
      {
        return matrix.M42;
      }
      set
      {
        matrix.M42 = value;
      }
    }
    public float PosZ
    {
      get
      {
        return matrix.M43;
      }
      set
      {
        matrix.M43 = value;
      }
    }

    public Vector3 Scale
    {
      get
      {
        return new Vector3(
          matrix.M11,
          matrix.M22,
          matrix.M33);
      }
      set
      {
        matrix.M11 = value.X;
        matrix.M22 = value.Y;
        matrix.M33 = value.Z;
      }
    }
    public float ScaleX
    {
      get
      {
        return matrix.M11;
      }
      set
      {
        matrix.M11 = value;
      }
    }
    public float ScaleY
    {
      get
      {
        return matrix.M22;
      }
      set
      {
        matrix.M22 = value;
      }
    }
    public float ScaleZ
    {
      get
      {
        return matrix.M33;
      }
      set
      {
        matrix.M33 = value;
      }
    }

    public Vector3 ShearX
    {
      get
      {
        return new Vector3(matrix.Column0);
      }
      set
      {
        matrix.M21 = value.Y;
        matrix.M31 = value.Z;
      }
    }
    public Vector3 ShearY
    {
      get
      {
        return new Vector3(matrix.Column2);
      }
      set
      {
        matrix.M12 = value.X;
        matrix.M32 = value.Z;
      }
    }
    public Vector3 ShearZ
    {
      get
      {
        return new Vector3(matrix.Column3);
      }
      set
      {
        matrix.M13 = value.X;
        matrix.M23 = value.Y;
      }
    }

    // Other helper properties

    // TODO: this implicit conversion may not be beneficial.. consider removing
    public static implicit operator Matrix4(Transform t)
    {
      return t.matrix;
    }
  }
}

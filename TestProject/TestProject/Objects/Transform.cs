using System;
using OpenTK;

namespace TestProject.Objects
{
  public class Transform
  {
    // TODO: make matrix private; to that end, create methods which perform
    // the needed operations that would normally act on the field via ref
    public Matrix4 matrix;
    public Quaternion rotation;
    private bool _noRotation;
    public bool NeedsRegeneration { get; private set; }
    public Vector3 position, scale;

    public Transform()
    {
      matrix = Matrix4.Identity;
      rotation = Quaternion.Identity;
      _noRotation = true;
    }

    public Transform(Matrix4 matrix)
    {
      this.matrix = matrix;
    }

    public void ResetRotation()
    {
      rotation = Quaternion.Identity;
      _noRotation = true;
    }

    // TODO: ref/out methods for these properties?

    public void RegenerateMatrix()
    {
      if (!NeedsRegeneration)
      {
        return;
      }

      rotation.ToMatrix(out matrix);
      Matrix4 temp = Matrix4.Scale(scale);
      Matrix4.Mult(ref matrix, ref temp, out matrix);
      matrix.Row3 = new Vector4(position, 1f);
    }

    public Matrix4 Matrix
    {
      get
      {
        if (NeedsRegeneration)
        {
          RegenerateMatrix();
        }

        return matrix;
      }
    }

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
        this.scale = value;
        if (_noRotation)
        {
          matrix.M11 = value.X;
          matrix.M22 = value.Y;
          matrix.M33 = value.Z;
        }
        else
        {
          NeedsRegeneration = true;
        }
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
        scale.X = value;
        if (_noRotation)
        {
          matrix.M11 = value;
        }
        else
        {
          NeedsRegeneration = true;
        }
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
        scale.Y = value;
        if (_noRotation)
        {
          matrix.M22 = value;
        }
        else
        {
          NeedsRegeneration = true;
        }
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
        scale.Z = value;
        if (_noRotation)
        {
          matrix.M33 = value;
        }
        else
        {
          NeedsRegeneration = true;
        }
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

    // TODO: a method to set rotation with a pass by ref?
    public Quaternion Rotation
    {
      get
      {
        return rotation;
      }
      set
      {
        rotation = value;
        _noRotation = false;
        NeedsRegeneration = true;
      }
    }

    // Other helper properties


    // TODO: this implicit conversion may not be beneficial.. consider removing
    public static implicit operator Matrix4(Transform t)
    {
      return t.matrix;
    }

    public void Rotate(ref Quaternion qrot)
    {
      Quaternion.Multiply(ref rotation, ref qrot, out rotation);
      NeedsRegeneration = true;
      //Vector3 savedPos = Pos;
      //Pos = Vector3.Zero;
      /*Vector3 axis;
      float angle;
      qrot.ToAxisAngle(out axis, out angle);
      Matrix4 mrot = Matrix4.CreateFromAxisAngle(axis, angle);*/
      /*
      Matrix4 mrot;
      qrot.ToMatrix(out mrot);
      Matrix4.Mult(ref matrix, ref mrot, out matrix);
      Pos = savedPos;
      */
    }
  }
}

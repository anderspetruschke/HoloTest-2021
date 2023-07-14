using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloUtil
{
  public enum Orientation
  {
    Up,
    Right,
    Down,
    Left,
  }

  public enum Eye
  {
    Left,
    Right,
  }

  public class Quad
  {
    public static Mesh Create(Rect rect, bool addBackFace = false, Orientation rot = Orientation.Up)
    {
      return Create(rect, addBackFace, rot, new Vector2(0, 0), new Vector2(1, 1));
    }

    public static Mesh Create(Rect rect, bool addBackFace, Orientation rot, Vector2 uvMin, Vector2 uvMax)
    {
      return Create(
        new Vector3(rect.xMin, rect.yMax, 0),
        new Vector3(rect.xMax, rect.yMax, 0),
        new Vector3(rect.xMin, rect.yMin, 0),
        new Vector3(rect.xMax, rect.yMin, 0),
        new Vector2(uvMin.x, uvMax.y),
        uvMax,
        uvMin,
        new Vector2(uvMax.x, uvMin.y),
        -Vector3.forward,
        addBackFace,
        rot
      );
    }

    public static Mesh Create(Vector3 tl, Vector3 tr, Vector3 bl, Vector3 br, Vector2 tlUV, Vector2 trUV, Vector2 blUV, Vector2 brUV, Vector3 normal, bool addBackFace = false, Orientation rot = Orientation.Up)
    {
      Mesh m = new Mesh();

      m.vertices = new Vector3[4] { bl, br, tl, tr };
      m.normals = new Vector3[4] { normal, normal, normal, normal };

      if (rot == Orientation.Up)
        m.uv = new Vector2[4] { blUV, brUV, tlUV, trUV };
      else if (rot == Orientation.Right)
        m.uv = new Vector2[4] { tlUV, blUV, trUV, brUV };
      else if (rot == Orientation.Down)
        m.uv = new Vector2[4] { trUV, tlUV, brUV, blUV };
      else if (rot == Orientation.Left)
        m.uv = new Vector2[4] { brUV, trUV, blUV, tlUV };

      if (addBackFace)
      {
        m.triangles = new int[12]
        {
          0, 2, 1,
          2, 3, 1,
          1, 2, 0, // Reverse winding order for back face
          1, 3, 2
        };
      }
      else
      {
        m.triangles = new int[6]
        {
          0, 2, 1,
          2, 3, 1
        };
      }

      return m;
    }

    public static Vector3[] GetVertices(Rect rect)
    {
      return new Vector3[4] {
        new Vector3(rect.xMin, rect.yMax, 0),
        new Vector3(rect.xMax, rect.yMax, 0),
        new Vector3(rect.xMin, rect.yMin, 0),
        new Vector3(rect.xMax, rect.yMin, 0)
      };
    }
  }
}

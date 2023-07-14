using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloUtil;

public class HoloRenderSurface : MonoBehaviour
{
  // Quad to render to
  public Vector2Int Resolution = new Vector2Int(800, 800);
  public int Depth = 24;
  public bool Enabled = true;

  public MeshFilter SurfaceMesh { get; protected set; }
  public MeshRenderer Renderer { get; protected set; }
  public MeshCollider Collider { get; protected set; }

  private Vector2Int m_targetRes = new Vector2Int(0, 0);
  private RenderTexture[] m_targetTex = null;

  public Rect SurfaceRect = new Rect(-0.5f, -0.5f, 1.0f, 1.0f);

  public void SetResolution(Vector2Int newRes) { Resolution = newRes; }

  private Mesh GenerateMesh() { return HoloUtil.Quad.Create(new Rect(-0.5f, -0.5f, 1.0f, 1.0f), true); }

  public void Init()
  {
    // Set up the quad
    SurfaceMesh = gameObject.GetComponent<MeshFilter>();
    if (!SurfaceMesh)
      SurfaceMesh = gameObject.AddComponent<MeshFilter>();

    SurfaceMesh.mesh = GenerateMesh();

    // Set up the quad renderer
    Renderer = gameObject.GetComponent<MeshRenderer>();
    if (!Renderer)
      Renderer = gameObject.AddComponent<MeshRenderer>();
    Renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

    // Add a collider so we can ray-trace the surface
    Collider = gameObject.GetComponent<MeshCollider>();
    if (!Collider)
      Collider = gameObject.AddComponent<MeshCollider>();

    SetResolution(Resolution);
  }

  public bool Render(Camera head, HoloUtil.Eye eye, float interocularDist)
  {
    if (!Enabled)
      return false;

    // Don't render the layer that the render cave is set up on
    head.cullingMask &= ~(1 << gameObject.layer);

    ResizeRenderTargets(); // Only bother resizing if the surface is enabled

    float halfIOD = interocularDist / 2;

    // Scaled clipping planes using the device scale
    float prevNearPlane = head.nearClipPlane;
    float prevFarPlane = head.farClipPlane;
    float scaledNearPlane = prevNearPlane * HoloDevice.active.GetWorldScale();
    float scaledFarPlane = prevFarPlane * HoloDevice.active.GetWorldScale();

    // Get head transforms
    Vector3 headPos = head.transform.position;
    Vector3 headRgt = head.transform.right;
    Vector3 a_eye = headPos + (eye == HoloUtil.Eye.Left ? -headRgt : headRgt) * halfIOD;

    // Collect the required points
    Vector3 surfaceBL = transform.TransformPoint(SurfaceMesh.mesh.vertices[0]); // Bottom-left
    Vector3 surfaceBR = transform.TransformPoint(SurfaceMesh.mesh.vertices[1]); // Bottom-right
    Vector3 surfaceTL = transform.TransformPoint(SurfaceMesh.mesh.vertices[2]); // Top-left
    Vector3 surfaceTR = transform.TransformPoint(SurfaceMesh.mesh.vertices[3]); // Top-right

    Vector3 pe = -a_eye;

    // Calculate basis
    Vector3 surfaceRight  = Vector3.Normalize(surfaceBR - surfaceBL);
    Vector3 surfaceUp     = Vector3.Normalize((surfaceTL + surfaceTR) * 0.5f - (surfaceBL + surfaceBR) * 0.5f);
    Vector3 surfaceNormal = Vector3.Normalize(Vector3.Cross(surfaceUp, surfaceRight));

    Vector3 eyeToBL = surfaceBL + pe;
    Vector3 eyeToBR = surfaceBR + pe;
    Vector3 eyeToTL = surfaceTL + pe;

    float left = Vector3.Dot(surfaceRight, eyeToBL);
    float right = Vector3.Dot(surfaceRight, eyeToBR);
    float bottom = Vector3.Dot(surfaceUp, eyeToBL);
    float top = Vector3.Dot(surfaceUp, eyeToTL);

    // Calculate vertical FOV of the camera
    float eyeToSurfaceDist = Vector3.Dot(-eyeToBL, surfaceNormal);
    head.fieldOfView = Mathf.Rad2Deg * Mathf.Atan(head.sensorSize.y / (eyeToSurfaceDist * 2)) * 2;

    if (head.usePhysicalProperties)
    { // Set physical camera properties
      // No gate fit needed
      head.gateFit = Camera.GateFitMode.None;

      // Sensor size can be set to the same dimensions as the surface
      head.sensorSize = new Vector2((surfaceBR - surfaceBL).magnitude, (surfaceBL - surfaceTL).magnitude);

      // Calculate lens shift
      Vector2 lensShift = new Vector2();
      lensShift.x = (left + right) / (2 * head.sensorSize.x); // Eye to surface center / surface size
      lensShift.y = (top + bottom) / (2 * head.sensorSize.y);
      head.lensShift = lensShift;

      head.nearClipPlane = scaledNearPlane;
      head.farClipPlane = scaledFarPlane;

      head.worldToCameraMatrix = new Matrix4x4(surfaceRight, surfaceUp, surfaceNormal, new Vector4(0, 0, 0, 1)).transpose * Matrix4x4.Translate(pe);
    }
    else
    {
      // Calculate frustum params
      float scale = -scaledNearPlane / Vector3.Dot(eyeToBL, surfaceNormal);
      left *= scale;
      right *= scale;
      bottom *= scale;
      top *= scale;

      // Set matrices
      head.projectionMatrix = Matrix4x4.Frustum(left, right, bottom, top, scaledNearPlane, scaledFarPlane);
      head.worldToCameraMatrix = new Matrix4x4(surfaceRight, surfaceUp, surfaceNormal, new Vector4(0, 0, 0, 1)).transpose * Matrix4x4.Translate(pe);
    }

    // Do the render
    head.targetTexture = GetTargetTexture(eye);
    head.Render();

    // Restore previous clipping planes
    head.nearClipPlane = prevNearPlane;
    head.farClipPlane = prevFarPlane;
    return true;
  }

  private void ResizeRenderTargets()
  {
    if (m_targetRes != Resolution || m_targetTex == null)
    {
      if (m_targetTex != null)
      {
        foreach (var texture in m_targetTex)
          texture.Release();
      }

      m_targetTex = new RenderTexture[2];
      m_targetTex[0] = new RenderTexture(Resolution.x, Resolution.y, Depth);
      m_targetTex[1] = new RenderTexture(Resolution.x, Resolution.y, Depth);
      m_targetRes = Resolution;
    }

    Renderer.material.mainTexture = m_targetTex[0];
  }

  public RenderTexture GetTargetTexture(HoloUtil.Eye eye)
  {
    return m_targetTex != null ? m_targetTex[(int)eye] : null;
  }

  void OnDrawGizmos()
  {
    Mesh m = GenerateMesh();
    Vector3[] vertices = m.vertices;
    Vector3 mid = Vector3.zero;
    for (int i = 0; i < vertices.Length; ++i) {
      mid += vertices[i];
      vertices[i] = transform.TransformPoint(vertices[i]);
    }
    mid /= vertices.Length;

    Vector3 normalStart = transform.TransformPoint(mid);
    Vector3 normalEnd   = transform.TransformPoint(mid + m.normals[0] * 0.1f);

    Gizmos.DrawLine(vertices[0], vertices[1]);
    Gizmos.DrawLine(vertices[1], vertices[3]);
    Gizmos.DrawLine(vertices[3], vertices[2]);
    Gizmos.DrawLine(vertices[2], vertices[0]);
    Gizmos.DrawLine(normalStart, normalEnd);
  }
}

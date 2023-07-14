using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

public class HoloRenderCave : MonoBehaviour
{
  [Serializable]
  public struct UserCave
  {
    [SerializeField]
    public Camera head;

    [SerializeField]
    public HoloRenderSurface[] surfaces;
  }

  public HoloDevice.Devices DeviceType;

  // Params for the surfaces
  private int m_depth = 24;
  public Vector2Int m_resolution = new Vector2Int(800, 800);
  public float m_quality = 1;
  public bool m_autoAdjustResolution = true;
  public float m_minQuality = 0.25f;
  public float m_maxQuality = 1;
  public int m_frameTargetMin = 15;
  public int m_frameTargetMax = 48;

  public bool m_enableExternalDisplay = true;
  private Camera m_externalDisplayCam = null;
  private Vector2Int m_externalDisplayMaxRes = new Vector2Int(-1, -1);
  private RenderTexture m_externalDisplayTex = null;

  // 3D Params
  public float m_interocularDistance = 0.065f; // 6.5cm
  public bool m_defaultInvert3D = false;
  public bool m_invert3D = false;

  // Child objects
  public UserCave[] m_userCave;

  private HoloViewer m_viewer;
  private RenderTexture[] m_renderTarget;
  private HoloUtil.Eye m_curEye;

  public void Init(HoloViewer viewClient)
  {
    m_viewer = viewClient;

    foreach (UserCave userCave in m_userCave)
    {
      foreach (HoloRenderSurface surface in userCave.surfaces)
      {
        surface.Resolution = m_resolution;
        surface.Depth = m_depth;
        surface.Init();
      }
    }
  }

  public Camera GetExternalDisplayCam() { return m_externalDisplayCam; }

  public void SetExternalDisplayCam(Camera cam, Vector2Int maxRes)
  {
    if (m_externalDisplayCam != cam && m_externalDisplayCam)
      m_externalDisplayCam.targetTexture = null; // Clear the target texture from the previous camera

    m_externalDisplayCam = cam;
    m_externalDisplayMaxRes = maxRes;

    if (m_externalDisplayCam)
      m_externalDisplayCam.targetTexture = m_externalDisplayTex;
  }

  void LateUpdate()
  {
    // Determine the scale of the interocular distance based on the device scale
    Vector3 caveScale3 = transform.lossyScale;
    float caveScale = Mathf.Max(Mathf.Max(caveScale3.x, caveScale3.y), caveScale3.z);
    float iod = m_interocularDistance * caveScale;

    // Time spent rendering
    Stopwatch renderTimer = new Stopwatch();

    // Render the cave if the server is ready.
    if (m_viewer.ShouldRender())
    {
      renderTimer.Start();
      // Render each eye
      int surfaceID = 0;
      for (int userID = 0; userID < HoloDevice.active.GetUserCount(); ++userID)
      {
        // Call pre-user callbacks
        HoloRenderCallbacks.InvokePreRenderUser(userID);

        UserCave userCave = m_userCave[userID];
        foreach (HoloRenderSurface surface in userCave.surfaces)
        {
          for (int eye = 0; eye < 2; ++eye)
          {
            // Determine which eye (in case 3D is inverted)
            m_curEye = (HoloUtil.Eye)(m_defaultInvert3D ^ m_invert3D ? 1 - eye : eye);

            // Call pre-eye callbacks
            HoloRenderCallbacks.InvokePreRenderEye(userID, m_curEye);

            // Render eye to surface
            if (surface.Render(userCave.head, m_curEye, iod))
              m_viewer.Present(surfaceID, (HoloUtil.Eye)eye, surface.GetTargetTexture(m_curEye));

            // Call post-eye callbacks
            HoloRenderCallbacks.InvokePostRenderEye(userID, m_curEye);
          }
          ++surfaceID;
        }

        // Call post-user callbacks
        HoloRenderCallbacks.InvokePostRenderUser(userID);
      }

      // Render the external display
      if (m_enableExternalDisplay && m_externalDisplayCam && m_externalDisplayCam.gameObject.activeInHierarchy)
      {
        // Disable the camera to stop unity from rendering it automatically
        m_externalDisplayCam.enabled = false;

        // Update render target for the external display
        int externalDisplayWidth  = (int)(m_viewer.GetExternalDisplaySize().x * m_quality);
        int externalDisplayHeight = (int)(m_viewer.GetExternalDisplaySize().y * m_quality);

        if (m_externalDisplayMaxRes.x > 0) externalDisplayWidth  = Mathf.Min(externalDisplayWidth, m_externalDisplayMaxRes.x);
        if (m_externalDisplayMaxRes.y > 0) externalDisplayHeight = Mathf.Min(externalDisplayHeight, m_externalDisplayMaxRes.y);

        if (externalDisplayWidth > 0 && externalDisplayHeight > 0)
        {
          if (!m_externalDisplayTex || m_externalDisplayTex.width != externalDisplayWidth || m_externalDisplayTex.height != externalDisplayHeight)
          {
            if (m_externalDisplayTex)
              m_externalDisplayTex.Release();

            m_externalDisplayTex = new RenderTexture(externalDisplayWidth, externalDisplayHeight, 24);
            m_externalDisplayCam.targetTexture = m_externalDisplayTex;
          }

          // Render the external display camera
          m_externalDisplayCam.Render();

          if (m_externalDisplayTex && m_externalDisplayTex.IsCreated())
            m_viewer.SetExternalDisplayTex(m_externalDisplayTex);
        }
      }
      else
      {
        m_viewer.CloseExternalDisplay();
      }

      m_viewer.Swap();
      renderTimer.Stop();

      if (m_autoAdjustResolution && m_viewer.IsRemote())
      {
        if (renderTimer.Elapsed.TotalSeconds > 1.0 / m_frameTargetMin)
          m_quality -= 0.1f;

        if (renderTimer.Elapsed.TotalSeconds < 1.0 / m_frameTargetMax)
          m_quality += 0.1f;
      }

      m_quality = Mathf.Clamp(m_quality, m_minQuality, m_maxQuality);
    }

    // Update the surface resolution
    foreach (UserCave userCave in m_userCave)
      foreach (HoloRenderSurface surface in userCave.surfaces)
        surface.SetResolution(new Vector2Int((int)(m_resolution.x * m_quality), (int)(m_resolution.y * m_quality)));
  }

  // The number of users supported by this HoloRenderCave
  public int GetUserCount()
  {
    return m_userCave.Length;
  }

  // Get surfaces by user
  public HoloRenderSurface[] GetSurfaces(int userID)
  {
    return m_userCave[userID].surfaces;
  }

  // Get all surfaces
  public HoloRenderSurface[] GetSurfaces()
  {
    List<HoloRenderSurface> surfaces = new List<HoloRenderSurface>();
    foreach (UserCave cave in m_userCave)
      surfaces.AddRange(cave.surfaces);
    return surfaces.ToArray();
  }
}
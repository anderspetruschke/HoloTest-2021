using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ExternalDisplayController : MonoBehaviour
{
  public Camera TargetCamera
  {
    get
    {
      if (!m_targetCamera)
        m_targetCamera = gameObject.GetComponent<Camera>();
      return m_targetCamera;
    }
  }

  public Vector2Int m_maxResolution = new Vector2Int(-1, -1);
  protected Camera m_targetCamera = null;

  static int m_externalDisplayCount = 0;

  void OnEnable()
  {
    if (++m_externalDisplayCount > 1)
      Debug.Log("Multiple external display cameras are enabled. Only 1 may be enabled at once");
  }

  void OnDisable()
  {
    --m_externalDisplayCount;

    if (HoloDevice.active.GetRenderCave().GetExternalDisplayCam() == TargetCamera)
      HoloDevice.active.GetRenderCave().SetExternalDisplayCam(null, new Vector2Int(-1, -1));
  }

  void Update()
  {
    HoloDevice.active.GetRenderCave().SetExternalDisplayCam(TargetCamera, m_maxResolution);
  }
}

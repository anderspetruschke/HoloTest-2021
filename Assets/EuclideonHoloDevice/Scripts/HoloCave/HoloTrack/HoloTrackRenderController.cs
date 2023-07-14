using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloTrackRenderController : MonoBehaviour
{
  protected bool m_useOverride = false;
  protected bool m_overrideState = false;

  protected List<HoloRenderSurface> m_surfaces = new List<HoloRenderSurface>();
  protected HoloTrack m_tracking;

  public void SetUseOverride(bool useOverride) { m_useOverride = useOverride; }
  public void SetOverrideState(bool overrideState) { m_overrideState = overrideState; }
  public void AddSurface(HoloRenderSurface surface) { m_surfaces.Add(surface); }

  // Start is called before the first frame update
  void Start()
  {
    // Find the tracking component attached to the gameobject
    m_tracking = gameObject.GetComponent<HoloTrack>();
    if (m_tracking == null)
      Debug.Log("HoloTrackRenderController must be attached to a gameobject with a HoloTrack component");
  }

  // Update is called once per frame
  void Update()
  {
    if (m_tracking == null)
      return;
    foreach (HoloRenderSurface surface in m_surfaces)
      surface.Enabled = m_useOverride ? m_overrideState : m_tracking.IsPositionValid();
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUserController : MonoBehaviour
{
  public int m_User = 0;
  public bool SimulateDisplayBounds = true;
  private Camera m_Camera = null;

  void Start()
  {
    m_Camera = gameObject.AddComponent<Camera>();
    m_Camera.cullingMask = 1 << 31;
    transform.localPosition = new Vector3(0, 1.8f, -2);
    transform.localEulerAngles = new Vector3(45, 0, 0);
  }

  // Update is called once per frame
  void Update()
  {
    HoloTrackWand wand = HoloDevice.active.GetUserWand(m_User);
    HoloTrackGlasses glasses = HoloDevice.active.GetUserGlasses(m_User);
    if (!wand)
      return;

    bool alt = Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.LeftAlt);

    bool primaryDown = Input.GetMouseButton(0) && !alt;
    bool secondaryDown = Input.GetMouseButton(0) && alt;
    bool triggerDown = Input.GetMouseButton(2);

    if (primaryDown) wand.OverrideButtonState(HoloTrackWand.Buttons.Primary, true);
    if (triggerDown) wand.OverrideButtonState(HoloTrackWand.Buttons.Trigger, true);
    if (secondaryDown) wand.OverrideButtonState(HoloTrackWand.Buttons.Secondary, true);
  }

  void LateUpdate()
  {
    HoloTrackWand wand = HoloDevice.active.GetUserWand(m_User);
    HoloTrackGlasses glasses = HoloDevice.active.GetUserGlasses(m_User);
    if (!wand)
      return;

    float deviceScale = HoloDevice.active.GetWorldScale();
    m_Camera.nearClipPlane = 0.05f * deviceScale;
    m_Camera.farClipPlane = 100f * deviceScale;
    m_Camera.cullingMask = SimulateDisplayBounds ? (1 << 31) : ~(1 << 31);

    HoloDevice.active.SetUserDebugSurfaceEnabled(m_User, SimulateDisplayBounds);

    if (!wand.IsPositionValid())
    { // Override the wand position if it's not being tracked
      Ray mouseRay = m_Camera.ScreenPointToRay(Input.mousePosition);
      wand.transform.rotation = Quaternion.LookRotation(mouseRay.direction, Vector3.up);
      wand.transform.localPosition = transform.localPosition;
    }

    if (!glasses.IsPositionValid())
    {
      glasses.transform.localRotation = transform.localRotation;
      glasses.transform.localPosition = transform.localPosition;
    }
  }

  void OnDrawGizmos()
  {
    if (!HoloDevice.active)
      return;

    HoloTrackWand wand = HoloDevice.active.GetUserWand(m_User);
    if (!wand)
      return;

    float deviceScale = HoloDevice.active.GetWorldScale();
    float len = 10 * deviceScale;
    if (wand.IsTriggerDown())
      wand.DrawLaser(new Color(1, 1, 1), len);
    else if (wand.IsButtonADown())
      wand.DrawLaser(new Color(1, 0, 1), len);
    else if (wand.IsButtonBDown())
      wand.DrawLaser(new Color(1, 0, 0), len);
    else
      wand.DrawLaser(HoloDevice.active.GetUserColour(m_User), len);
  }
}

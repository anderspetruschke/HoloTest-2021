using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoloTrackWand : HoloTrackDevice
{
  public int m_id = 0;
  public LayerMask m_UIInteractMask = 1 << 5; // Default to UI layer
  public GameObject m_UICursor = null;
  public bool m_AlignCursorToSurface = true;
  public PointerEventData.InputButton m_PrimaryMapping = PointerEventData.InputButton.Left;
  public PointerEventData.InputButton m_SecondaryMapping = PointerEventData.InputButton.Right;
  public PointerEventData.InputButton m_TriggerMapping = PointerEventData.InputButton.Middle;

  // Camera for input events (disabled so it doesn't render)
  public Camera EventCamera
  {
    get
    {
      if (!m_eventCamera)
      {
        m_eventCamera = gameObject.AddComponent<Camera>();
        m_eventCamera.enabled = false;
      }

      return m_eventCamera;
    }
  }

  protected Camera m_eventCamera;
  protected float m_LaserHitDist = float.MaxValue;

  public override string GetUser()
  {
    return "Wand" + m_id;
  }

  // Update is called once per frame
  new protected void Update()
  {
    base.Update(); // Update buttons
    transform.localPosition = Position();
    transform.localRotation = Rotation();
  }

  // Get a ray in global world space for the wand laser.
  public Ray GetRay() { return new Ray(transform.position, transform.forward); }

  // Get a ray in device space (i.e. relative to the device's current transform)
  public Ray GetRayDeviceSpace() { return new Ray(transform.localPosition, transform.localRotation * Vector3.forward); }

  // Draw a line showing the wand laser.
  public void DrawLaser(Color colour, float length, bool debugOnly = false)
  {
    Ray ray = GetRay();

    if (debugOnly)
    {
      Debug.DrawLine(ray.origin, ray.GetPoint(length), colour);
    }
    else
    {
      Color prevColour = Gizmos.color;
      Gizmos.color = colour;
      Gizmos.DrawLine(ray.origin, ray.GetPoint(length));
      Gizmos.color = prevColour; // Restore previous gizmo colour
    }
  }

  // Check if the trigger button is pressed (i.e. Went from !Down -> Down).
  // This is an instantaneous signal.
  public bool IsTriggerPressed()  { return IsButtonPressed(Buttons.Trigger); }

  // Check if the trigger button is held down.
  // This is a continuous signal.
  public bool IsTriggerDown()     { return IsButtonDown(Buttons.Trigger); }

  // Check if the trigger button is released (i.e. Went from Down -> !Down).
  // This is an instantaneous signal.
  public bool IsTriggerReleased() { return IsButtonReleased(Buttons.Trigger); }

  // Check if the A Button is pressed (i.e. Went from !Down -> Down).
  // This is an instantaneous signal.
  public bool IsButtonAPressed()  { return IsButtonPressed(Buttons.Primary); }

  // Check if the A Button is held down.
  // This is a continuous signal.
  public bool IsButtonADown()     { return IsButtonDown(Buttons.Primary); }

  // Check if the A Button is released (i.e. Went from Down -> !Down).
  // This is an instantaneous signal.
  public bool IsButtonAReleased() { return IsButtonReleased(Buttons.Primary); }

  // Check if the B Button is pressed (i.e. Went from !Down -> Down).
  // This is an instantaneous signal.
  public bool IsButtonBPressed()  { return IsButtonPressed(Buttons.Secondary); }

  // Check if the B Button is held down.
  // This is a continuous signal.
  public bool IsButtonBDown()     { return IsButtonDown(Buttons.Secondary); }

  // Check if the B Button is released (i.e. Went from Down -> !Down).
  // This is an instantaneous signal.
  public bool IsButtonBReleased() { return IsButtonReleased(Buttons.Secondary); }

  // Translate a mouse input button to the mapped wand input.
  public Buttons MouseToWandButton(PointerEventData.InputButton mouseButton)
  {
    if (m_PrimaryMapping == mouseButton)
      return Buttons.Primary;
    if (m_SecondaryMapping == mouseButton)
      return Buttons.Secondary;
    if (m_TriggerMapping == mouseButton)
      return Buttons.Trigger;
    return Buttons.Primary;
  }

  // Translate a wand input button to the mapped mouse input.
  public PointerEventData.InputButton WandToMouseButton(Buttons wandButton)
  {
    switch (wandButton)
    {
    case Buttons.Primary:   return m_PrimaryMapping;
    case Buttons.Secondary: return m_SecondaryMapping;
    case Buttons.Trigger:   return m_TriggerMapping;
    }

    return PointerEventData.InputButton.Left;
  }

  // Get the distance from the wand to the interactable component that is hovered.
  public float GetLaserHitDist() { return m_LaserHitDist; }

  // Used internally to set the distance to the hovered interactable component.
  public void SetLaserHitDist(float dist) { m_LaserHitDist = dist; }
}

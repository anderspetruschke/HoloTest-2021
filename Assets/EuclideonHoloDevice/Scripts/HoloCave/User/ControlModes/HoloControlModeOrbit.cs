using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloControlModeOrbit : HoloControlModeBase
{
  protected bool m_isActive = false;
  protected Vector3 m_orbitOrigin = Vector3.zero;
  protected Vector3 m_lastWandDir = Vector3.zero;
  protected Vector3 m_controlUp = Vector3.up;

  public override bool Apply(HoloTrackGlasses glasses, HoloTrackWand wand, HoloTrackDevice.ButtonState actionButtonState, float flySpeed, bool showLaser)
  {
    if (actionButtonState.pressed)
    {
      m_isActive = true;
      m_controlUp = (wand.transform.localRotation * Vector3.up).normalized;
      m_lastWandDir = GetWandOri(wand);

      Ray wandRay = wand.GetRay();
      RaycastHit hit;

      if (Physics.Raycast(wandRay, out hit))
        m_orbitOrigin = hit.point;
      else if (HoloDevice.active.RaycastDeviceSurface(wandRay, out hit))
        m_orbitOrigin = hit.point;
      else
        m_isActive = false;
    }

    if (m_isActive && actionButtonState.down)
    {
      // Calculate the difference in the wands orientation 
      Vector3 wandDir = GetWandOri(wand);
      float angle = Vector3.Angle(wandDir, m_lastWandDir);
      if (angle > 0) // Now determine the direction of the rotation
        angle *= Vector3.Dot(Vector3.Cross(m_lastWandDir, wandDir), m_controlUp) > 0 ? 1 : -1;

      Quaternion diff = Quaternion.AngleAxis(angle * flySpeed * 2, Vector3.up);

      // Get the current world position and orientation
      Quaternion worldOri = HoloDevice.active.GetWorldOrientation();
      Vector3 worldPos = HoloDevice.active.GetWorldPosition();

      // Rotate the world position about m_orbitOrigin
      worldPos = m_orbitOrigin + diff * (worldPos - m_orbitOrigin);

      HoloDevice.active.SetWorldOrientation(diff * worldOri);
      HoloDevice.active.SetWorldPosition(worldPos);
      m_lastWandDir = wandDir;
    }

    return m_isActive;
  }

  public Vector3 GetWandOri(HoloTrackWand wand)
  {
    Vector3 wandFwd = (wand.transform.localRotation * Vector3.forward).normalized;

    // Subtract wandFwd projected to the up direction, as we don't want the vertical component of the wand direction.
    // This amounts to wandFwd.y = 0, but this is the more correct way to do it. The up direction for the dot product can be changed for a different yaw axis.
    wandFwd -= m_controlUp * Vector3.Dot(wandFwd, m_controlUp);

    return wandFwd;
  }
}

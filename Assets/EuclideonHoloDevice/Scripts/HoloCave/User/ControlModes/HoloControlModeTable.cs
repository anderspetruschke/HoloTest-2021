using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloControlModeTable : HoloControlModeBase
{
  private bool m_isDragging = false;
  private float m_prevScale = 0;

  private Vector3 m_firstWorldPos       = Vector3.zero;
  private Vector3 m_firstIntersect      = Vector3.zero;
  private Vector3 m_firstWorldIntersect = Vector3.zero;
  private Vector3 m_firstIntersectLocal = Vector3.zero;

  private Ray m_firstWandRay = new Ray();
  private Ray m_prevWandRay = new Ray();

  private float m_tableHeight = 0.615f;

  public override bool Apply(HoloTrackGlasses glasses, HoloTrackWand wand, HoloTrackDevice.ButtonState actionButtonState, float flySpeed, bool showLaser)
  {
    if (!actionButtonState.down)
      m_isDragging = false;

    if (m_isDragging)
    {
      MoveWorld(wand);

      if (showLaser)
        wand.DrawLaser(Color.cyan, 100 * HoloDevice.active.GetWorldScale(), true);
    }
    else
    {
      if (showLaser)
        wand.DrawLaser(Color.white, 100 * HoloDevice.active.GetWorldScale(), true);
    }

    if (actionButtonState.pressed)
      StartDragging(wand);

    m_prevWandRay = wand.GetRayDeviceSpace();
    return m_isDragging;
  }

  protected void MoveWorld(HoloTrackWand wand)
  {
    HoloDevice device = HoloDevice.active;
    Ray wandRay = wand.GetRayDeviceSpace();

    // Scale world
    Vector3 wandPosDiff = wandRay.origin - m_prevWandRay.origin;
    float wandScaleAmount = wandPosDiff.magnitude;

    if (wandScaleAmount > 0)
    {
      const float thresholdLower = 25.0f;
      const float thresholdUpper = 40.0f;
      const float thresholdRange = thresholdUpper - thresholdLower;
      float degs = Mathf.Acos(Mathf.Abs(Vector3.Dot(m_prevWandRay.direction, wandPosDiff.normalized))) * Mathf.Rad2Deg;
      float scalingRatio = 1.0f - Mathf.Clamp(degs - thresholdLower, 0, thresholdRange) / thresholdRange;

      const float scaleRatio = 5.0f;
      float direction = Vector3.Dot(wandPosDiff.normalized, m_prevWandRay.direction) > 0 ? 1 : -1;
      float worldScale = device.GetWorldScale();
      float power = direction * scalingRatio * wandScaleAmount * 4.0f;
      float newScale = worldScale * Mathf.Pow(scaleRatio, power);

      // Scale the device
      float appliedScale = Mathf.Clamp(newScale, 0.01f, 400000.0f);
      device.SetWorldScale(appliedScale);
      m_prevScale = newScale;
    }

    // Pan
    Plane devicePlane = GetDeviceScreen();
    float dist = float.MaxValue;
    if (devicePlane.Raycast(wandRay, out dist))
    {
      Vector3 coord = wandRay.origin + wandRay.direction * dist;
      Transform wldTransform = device.GetWorldTransform();
      Vector3 intersectDiff = m_firstWorldIntersect - wldTransform.TransformPoint(coord);
      device.SetWorldPosition(device.GetWorldPosition() + intersectDiff);
    }
  }

  Plane GetDeviceScreen()
  {
    return new Plane(Vector3.up, new Vector3(0, m_tableHeight, 0));
  }

  protected void StartDragging(HoloTrackWand wand)
  {
    // Get the device
    HoloDevice device = HoloDevice.active;

    // Get the wands ray
    Ray wandRay = wand.GetRayDeviceSpace();

    Plane devicePlane = GetDeviceScreen();
    // Test wand intersection with the tables screen
    float dist = float.MaxValue;
    if (devicePlane.Raycast(wandRay, out dist))
    {
      Vector3 coord = wandRay.origin + wandRay.direction * dist;
      m_firstIntersect = coord;
      m_firstWorldIntersect = device.GetWorldTransform().TransformPoint(coord);
      m_firstWandRay = wandRay;
      m_prevWandRay = wandRay;
      m_isDragging = true;
      m_prevScale = device.GetWorldScale();
      m_firstWorldPos = device.GetWorldPosition();
    }
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUserViewEmulation : MonoBehaviour
{
  float m_lastPosition = 0.0f;
  float m_lastHeadHeight = 0.0f;

  public float ViewPosition
  {
    get { return m_lastPosition; }
    set
    {
      if (Mathf.Abs(value - m_lastPosition) > 0.01f)
      {
        MoveOnRails(value, HeadHeight);
        m_lastPosition = value;
      }
    }
  }

  public float HeadHeight
  {
    get { return m_lastHeadHeight; }
    set
    {
      if (Mathf.Abs(value - m_lastHeadHeight) > 0.01f)
      {
        MoveOnRails(ViewPosition, value);
        m_lastHeadHeight = value;
      }
    }
  }

  void Start()
  {
    MoveOnRails(m_lastPosition, m_lastHeadHeight);
  }

  // Moves the object that this script is attached to based on the device type, at roughly eye level (1.6m)
  // e.g. For the Table, this is a circle around the Table, For the wall, a semi-circle in front.
  public void MoveOnRails(float t, float headHeight)
  {
    if (!HoloDevice.active)
      return;

    Transform deviceTransform = HoloDevice.active.GetWorldTransform();
    switch (HoloDevice.active.GetDeviceType())
    {
      case HoloDevice.Devices.HologramTable:
        {
          float tableHeight = 0.52f;
          Vector2 circleCenter = new Vector2(0, -0.6f);
          Vector2 circleOutside = new Vector2(0, 0);
          float distance = 1.1f;
          circleOutside.y = circleCenter.y + distance * Mathf.Sin(t * Mathf.PI - (Mathf.PI / 2));
          circleOutside.x = circleCenter.x + distance * Mathf.Cos(t * Mathf.PI - (Mathf.PI / 2));
          transform.localPosition = new Vector3(circleOutside.x, headHeight, circleOutside.y);
          Vector3 lookAtPos = deviceTransform.TransformPoint(new Vector3(circleCenter.x, tableHeight, circleCenter.y));
          transform.LookAt(lookAtPos);
          break;
        }
      case HoloDevice.Devices.HologramWall:
        {
          float screenWidth = 3.0f;
          transform.localPosition = new Vector3(t * screenWidth, headHeight, -2.0f);
          transform.LookAt(deviceTransform.TransformPoint(new Vector3(0, 1, 0)));
          break;
        }
      case HoloDevice.Devices.HologramRoom:
        {
          Vector3 frontCenter = new Vector3(0, 1.1f, 0);
          transform.localPosition = new Vector3(0, headHeight, -2.0f);
          float pitch = Vector3.Angle(frontCenter - transform.localPosition, Vector3.forward);
          if (transform.localPosition.y < frontCenter.y)
            pitch = -pitch;
          transform.localRotation = Quaternion.Euler(pitch, t * 70, 0);
          break;
        }
      case HoloDevice.Devices.HologramTableSingleUser:
        {
          float tableHeight = 0.6f;
          Vector2 circleCenter = new Vector2(0, -0.5f);
          Vector2 circleOutside = new Vector2(0, 0);
          float distance = 1.2f;
          circleOutside.y = circleCenter.y + distance * Mathf.Sin(t * Mathf.PI - (Mathf.PI / 2));
          circleOutside.x = circleCenter.x + distance * Mathf.Cos(t * Mathf.PI - (Mathf.PI / 2));
          transform.localPosition = new Vector3(circleOutside.x, headHeight, circleOutside.y);
          Vector3 lookAtPos = deviceTransform.TransformPoint(new Vector3(circleCenter.x, tableHeight, circleCenter.y));
          transform.LookAt(lookAtPos);
          break;
        }
    }
  }
}

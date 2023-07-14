using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base HoloTrack class
public class HoloTrack : MonoBehaviour
{
  public bool m_enabled = true;
  public bool m_debugButtonInputs = false;
  public bool m_trackingSmoothed = true;
  public string m_server = "localhost";

  protected long m_lastActiveTime = -1;
  protected Vector3 m_lastTrackingPosition;
  protected int m_activeThreshold = 1000;
  protected bool m_trackingPositionInitialized = false;

  protected List<Vector3> m_positionHistory = new List<Vector3>();

  public virtual string GetUser() { return "Events"; }

  // Check if the tracking position is valid.
  // If the position has changed within the active threshold (40ms ?) then this
  // will return true, otherwise it will return false.
  // If m_enabled==false this function always returns false.
  public bool IsPositionValid() { return m_enabled && ((long)(Time.unscaledTime * 1000) - m_lastActiveTime) < m_activeThreshold; }

  // Get the full host address for this device
  public string Host() { return GetUser() + "@" + m_server; }

  // Returns the current state of the given button
  protected bool Button(int a_button) { return m_enabled ? HoloTrackInterface.vrpnButton(Host(), (int)a_button) : false; /*Button0 is reserved*/ }

  // Returns the reported battery value
  protected double Battery() { return m_enabled ? HoloTrackInterface.vrpnAnalog(Host()) : 1; }

  // Returns the device rotation
  protected Quaternion Rotation() { return m_enabled && IsPositionValid() ? HoloTrackInterface.vrpnTrackerQuat(Host()) : transform.localRotation; }

  // Returns the device position
  protected Vector3 Position()
  {
    if (m_enabled)
    {
      Vector3 rawTrackedPos = HoloTrackInterface.vrpnTrackerPos(Host());
      if (!m_trackingPositionInitialized)
      {
        m_lastTrackingPosition = rawTrackedPos;
        m_lastActiveTime = -2 * m_activeThreshold; // default m_lastActiveTime to a value that indicates the position is definitely not valid
        m_trackingPositionInitialized = true;
      }

      Vector3 filteredTrackedPos = rawTrackedPos;
      if (m_trackingSmoothed)
      {
        // Manage the history
        const int framesToCollect = 10;
        while (m_positionHistory.Count > framesToCollect)
          m_positionHistory.RemoveAt(0);

        // Store the new data
        m_positionHistory.Add(rawTrackedPos);

        // Generate averaged position
        Vector3 averagedPosition = Vector3.zero;
        float total = 0;
        const float exponent = 1.6f;
        for (int deviceIndex = 0; deviceIndex < m_positionHistory.Count; ++deviceIndex)
        {
          float weight = Mathf.Pow((float)deviceIndex, exponent);
          averagedPosition += m_positionHistory[deviceIndex] * weight;
          total += weight;
        }
        if (total > 0)
          averagedPosition /= total;

        // Apply averaged position
        filteredTrackedPos = averagedPosition;
      }

      // Update device validity if the position has changed
      if (rawTrackedPos != m_lastTrackingPosition)
        m_lastActiveTime = (long)(Time.unscaledTime * 1000);
      m_lastTrackingPosition = rawTrackedPos;

      if (IsPositionValid())
        return filteredTrackedPos;
    }

    return transform.localPosition;
  }
}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

// Basic interface for fetching data from vrpn.dll
// FrameCount is used to sync the data between all components that may use this interface
public class HoloTrackInterface
{
  // API imports
  [DllImport("vrpn")]
  private static extern double vrpnAnalogExtern(string address, int channel, int frameCount);

  [DllImport("vrpn")]
  private static extern bool vrpnButtonExtern(string address, int channel, int frameCount);

  [DllImport("vrpn")]
  private static extern double vrpnTrackerExtern(string address, int channel, int component, int frameCount);

  // Convenience wrappers
  public static double vrpnAnalog(string address, int channel = 0) { return vrpnAnalogExtern(address, channel, Time.frameCount); }

  public static bool vrpnButton(string address, int channel = 0) { return vrpnButtonExtern(address, channel, Time.frameCount); }

  public static Vector3 vrpnTrackerPos(string address, int channel = 0)
  {
    return new Vector3(
      (float)vrpnTrackerExtern(address, channel, 0, Time.frameCount),
      (float)vrpnTrackerExtern(address, channel, 2, Time.frameCount),  // Axes swapped due to Unity being Y-up
      (float)vrpnTrackerExtern(address, channel, 1, Time.frameCount));
  }

  public static Quaternion vrpnTrackerQuat(string address, int channel = 0)
  {
    return new Quaternion(
      (float)vrpnTrackerExtern(address, channel, 3, Time.frameCount),
      (float)vrpnTrackerExtern(address, channel, 5, Time.frameCount),  // Axes swapped due to Unity being Y-up
      (float)vrpnTrackerExtern(address, channel, 4, Time.frameCount),
     -(float)vrpnTrackerExtern(address, channel, 6, Time.frameCount)); // Negate the real part to preserve chirality
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HoloDeviceConfig", menuName = "EuclideonHoloDevice/CreateDeviceConfig", order = 1)]
public class HoloConfig : ScriptableObject
{
  [Tooltip("Which prefab to use when instantiating the Device Manager.")]
  public GameObject DeviceManagerPrefab;

  // Hologram Device Config
  [Tooltip("Which type of Hologram Device to act as.")]
  public HoloDevice.Devices TargetDeviceType = HoloDevice.Devices.HologramTable;
  [Tooltip("The IP of the device to connect to.")]
  public string DeviceIP = "localhost";
  [Tooltip("The port to look for on the Hologram Device.")]
  public int DevicePort = 32919;

  [Tooltip("Whether to use Tracking data from the Hologram Device - this will force the position of the User's Glasses and Wand objects. Should be disabled if you can't connect to a device.")]
  public bool EnableTracking = true;
  [Tooltip("Whether to override the Tracking Source IP with the device that is being rendered to.")]
  public bool UseDeviceIPAsTrackingSource = true;
  [Tooltip("The source of Tracking data. Is overridden by the Device IP if the above option is true.")]
  public string TrackingSourceIP = "localhost";

  [Tooltip("Whether to use the Hologram Device render pipeline - can safely be turned off while testing and developing your project.")]
  public bool EnableDeviceRender = true;
  [Tooltip("Whether to stream the render to a remote hologram device - useful to quickly test on a Hologram Device without building the project.")]
  public bool RemoteDisplay = false;
  [Tooltip("Whether to auto adjust the resolution of the surfaces when using the remote display.")]
  public bool AutoAdjustResolution = true;

  // Control Modes
  [Tooltip("Control Modes are an optional helper to allow you to change your view of the world using familiar control systems similar to those in Holoverse Professional.")]
  public HoloDevice.ControlMode StartingControlMode = HoloDevice.ControlMode.None;
  [Tooltip("The speed to use for the selected Control Mode.")]
  public float StartingControlModeSpeed = 1.0f;
  [Tooltip("Which button to use to action the selected Control Mode.")]
  public HoloTrackDevice.Buttons ControlModeButton = HoloTrackDevice.Buttons.Trigger;
  [Tooltip("Whether to add audio listeners to the Scene if none exist on startup.")]
  public bool AddAudioListeners = true;

  // Tracking
  [Tooltip("Whether to apply smoothing to the tracked position.")]
  public bool SmoothTrackedPoints = true;
  [Tooltip("Whether to show the calibration screen on start. This guides the user through tapping and taring, and making sure the 3D is working correctly.")]
  public bool ShowCalibrationScreen = true;

  // Debugging
  [Tooltip("Whether to draw the User's Wand lasers in the Scene.")]
  public bool ShowUserLasers = true;
  [Tooltip("Which view to show the debug surface render texture for. This will show what that user will see on the Hologram Device's display.")]
  public int DebugSurfaceView = 0;
  [Tooltip("Enable the debug user controller. This can be used to test user input from the editor.")]
  public bool EnableDebugUserController = true;
  [Tooltip("Set which user to debug on desktop.")]
  public int DebugUserContollerTarget = 0;
  [Tooltip("If enabled, when using the Debug User Controller, objects in the scene will be clipped to match how it would be displayed on the physical device.")]
  public bool UserContollerSimulateDevice = true;
  [Tooltip("Moving this slider will change the position of the Debug User Controller to a location where a 1.8-metre tall person might view the device from.")]
  public float DebugUserViewEmulationPosition = 0.0f;
  [Tooltip("The height of the virtual user's eyes to set the camera's height to (in metres).")]
  public float DebugUserViewHeight = 1.6f;

  // Rendering Config
  [Tooltip("If this is disabled the user views will only be rendered when the tracking data is valid.")]
  public bool AlwaysRender = false;
  [Tooltip("Enable this to only render the View for User 1. This will improve the performance when using the remote viewer.")]
  public bool OnlyRenderUser1 = false;
  [Tooltip("Try enabling this if you get strange rendering and are using HDR in your project.")]
  public bool HDRCompensation = false;
  [Tooltip("Enable this to output the device render to a local window.")]
  public bool RenderDebugWindow = false;
  [Tooltip("The 3D technique to use when Presenting the Frame. In almost all cases, this should be left as 'Unknown' (default for the device).")]
  public HoloView.API.ProjectorMode ProjectorMode = HoloView.API.ProjectorMode.Default;

  // Internal Settings
  [Tooltip("Whether to swap the left and right eyes during the render.")]
  public bool Invert3D = false;
  [Tooltip("The number of users to support in code.")]
  public int MaxUsers = 2;

  public string Version
  {
    get
    {
      return majorVersion + "." + minorVersion + "." + patchVersion;
    }
  }

  private int majorVersion = 0;
  private int minorVersion = 5;
  private int patchVersion = 0;
}

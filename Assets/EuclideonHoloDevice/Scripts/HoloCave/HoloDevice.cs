using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloDevice : MonoBehaviour
{
  // Supported devices
  public enum Devices
  {
    None,
    HologramTable,
    HologramWall,
    HologramRoom,
    HologramTableSingleUser,
    Count
  }

  // Built-in control modes
  public enum ControlMode
  {
    None = -1,
    Wand = 0,
    // Rocket = 1,     // Not implemented
    // Psychic = 2,    // Not implemented
    // Helicopter = 3, // Not implemented
    Orbit = 4,
    Table = 5,
    // Jump = 6,       // Not implemented
  }

  public const int ControlModeCount = 7;

  // The active HoloDevice interface script.
  // There should only ever be one, however this prevents the user
  // from having to search for the HoloDevice script in the scene
  // when they want access to it.
  public static HoloDevice active = null;

  public HoloConfig DeviceConfig = null;

  public HoloDeviceManager DeviceManager
  {
    get
    {
      if (m_deviceManager == null)
        m_deviceManager = CreateDeviceManager();
      return m_deviceManager;
    }
  }
  static HoloDeviceManager m_deviceManager = null;

  // ** DEVICE **

  // Get the device type the scene is currently running on.
  public Devices GetDeviceType() { return m_renderCave ? m_renderCave.DeviceType : Devices.None; }
  
  // Get the devices world scale.
  public float GetWorldScale()
  {
    // Remove parent so localScale resolves to world space
    Transform currentParent = transform.parent;
    transform.parent = null;
    // Read scale
    float scale = transform.localScale.x;
    // Restore parent
    transform.parent = currentParent;

    return scale;
  }

  // Get the devices world position.
  public Vector3 GetWorldPosition(bool localPosition = true) { return localPosition ? transform.localPosition : transform.position; }

  // Get the devices world rotation as Euler angles.
  public Vector3 GetWorldRotation(bool localRotation = true) { return localRotation ? transform.localEulerAngles : transform.eulerAngles; }

  // Get the devices world orientation as a quaternion.
  public Quaternion GetWorldOrientation(bool localOrientation = true) { return localOrientation ? transform.localRotation : transform.rotation; }

  // Get the devices world transformation.
  public Transform GetWorldTransform() { return transform; }

  // Set the devices world scale.
  public void SetWorldScale(float scale)
  {
    // Remove parent so localScale is in world space
    Transform currentParent = transform.parent;
    transform.parent = null;
    // Set localScale to the desired value (uniformly)
    transform.localScale = new Vector3(scale, scale, scale);
    // Restore the parent, which will calculate the required localScale
    transform.parent = currentParent;
  }

  // Set the devices world position.
  // 'globalPosition' specifies if 'pos' should be set as a a local translation or global translation.
  public void SetWorldPosition(Vector3 pos, bool localPosition = true) { if (localPosition) transform.localPosition = pos; else transform.position = pos; }

  // Set the devices world rotation as Euler angles.
  // 'globalRotation' specifies if 'euler' should be set as a a local rotation or global rotation.
  public void SetWorldRotation(Vector3 euler, bool localRotation = true) { if (localRotation) transform.localEulerAngles = euler; else transform.eulerAngles = euler; }

  // Set the devices world orientation as a quaternion.
  // 'globalOrientation' specifies if 'quat' should be set as a a local orientation or global orientation.
  public void SetWorldOrientation(Quaternion quat, bool localOrientation = true) { if (localOrientation) transform.localRotation = quat; else transform.rotation = quat; }

  // Get the tracking events interface.
  public HoloTrackEvents GetTrackingEvents() { return m_trackingEvents; }

  // Get the number of supported users for the given device type.
  public static int GetNumSupportedUsers(Devices deviceType)
  {
    switch (deviceType)
    {
      case Devices.HologramTable:           return 2;
      case Devices.HologramWall:            return 1;
      case Devices.HologramRoom:            return 1;
      case Devices.HologramTableSingleUser: return 1;
      default: return 0;
    }
  }

  // Enable the users surfaces in the scene. Only one users surfaces can be enabled.
  public void SetUserDebugSurfaceEnabled(int user, bool enabled)
  {
    if (enabled) // Disable other surfaces
      foreach (HoloRenderSurface surface in m_renderCave.GetSurfaces())
        surface.gameObject.SetActive(false);

    // Set enabled state
    foreach (HoloRenderSurface surface in m_renderCave.GetSurfaces(user))
      surface.gameObject.SetActive(enabled);
  }

  // Test the intersection of the device screen and a Ray.
  // Returns true if the screen is hit.
  public bool RaycastDeviceSurface(Ray ray, out RaycastHit hit)
  {
    hit = new RaycastHit();
    if (m_renderCave.m_userCave == null)
      return false;
    foreach (HoloRenderSurface surface in m_renderCave.GetSurfaces(0))
      if (surface.Collider.Raycast(ray, out hit, float.MaxValue))
        return true;
    return false;
  }

  // ** PERIPHERAL INPUT **

  public bool GetKeyDown(KeyCode key)       { return DeviceManager.GetKeyDown(key); }
  public bool GetKeyPressed(KeyCode key)    { return DeviceManager.GetKeyPressed(key); }
  public bool GetKeyReleased(KeyCode key)   { return DeviceManager.GetKeyReleased(key); }
  public double GetKeyDownTime(KeyCode key) { return DeviceManager.GetKeyDownTime(key); }

  public bool GetMouseDown(KeyCode mouse)       { return DeviceManager.GetMouseDown(mouse); }
  public bool GetMousePressed(KeyCode mouse)    { return DeviceManager.GetMousePressed(mouse); }
  public bool GetMouseReleased(KeyCode mouse)   { return DeviceManager.GetMouseReleased(mouse); }
  public double GetMouseDownTime(KeyCode mouse) { return DeviceManager.GetMouseDownTime(mouse); }

  public int GetMouseScroll()          { return DeviceManager.GetMouseScroll(); }
  public Vector2Int GetMousePosition() { return DeviceManager.GetMousePosition(); }

  // ** USERS **

  // Returns the number of active users.
  public int GetUserCount() { return Mathf.Min(GetNumSupportedUsers(GetDeviceType()), DeviceConfig.MaxUsers); }

  // Returns true if 'user' is a valid user ID.
  public bool IsUserValid(int user) { return user >= 0 && user < GetUserCount() && m_users[user].valid; }

  // Get a user's wand tracking interface script.
  public HoloTrackWand GetUserWand(int user) { return IsUserValid(user) ? m_users[user].wand : null; }

  // Get a user's glasses tracking interface script.
  public HoloTrackGlasses GetUserGlasses(int user) { return IsUserValid(user) ? m_users[user].glasses : null; }

  // Get a user's wand GameObject
  // This is the GameObject that the wands tracking interface script is attached to.
  public GameObject GetUserGlassesObject(int user) { return IsUserValid(user) ? GetUserGlasses(user).gameObject : null; }

  // Get a user's glasses GameObject.
  // This is the GameObject that the glasses tracking interface script is attached to.
  public GameObject GetUserWandObject(int user) { return IsUserValid(user) ? GetUserWand(user).gameObject : null; }

  // Get the position of a user's wand.
  public Vector3 GetUserWandPosition(int user) { return IsUserValid(user) ? GetUserWandObject(user).transform.position : Vector3.zero; }

  // Get the forward direction of a user's wand.
  public Vector3 GetUserWandDirection(int user) { return IsUserValid(user) ? GetUserWandObject(user).transform.forward : Vector3.zero; }

  // Get the position of a user's glasses.
  public Vector3 GetUserGlassesPosition(int user) { return IsUserValid(user) ? GetUserGlassesObject(user).transform.position : Vector3.zero; }

  // Get the forward direction of a user's glasses.
  public Vector3 GetUserGlassesDirection(int user) { return IsUserValid(user) ? GetUserGlassesObject(user).transform.forward : Vector3.zero; }

  // Set a user's wand position.
  // This has no affect if tracking is enabled.
  public void SetUserWandPosition(int user, Vector3 position, bool localPosition = true) { if (IsUserValid(user)) GetUserWand(user).SetPosition(position, localPosition); }

  // Set a user's wand orientation as a quaternion.
  // This has no affect if tracking is enabled.
  public void SetUserWandOrientation(int user, Quaternion orientation, bool localOrientation = true) { if (IsUserValid(user)) GetUserWand(user).SetOrientation(orientation, localOrientation); }

  // Set a user's wand rotation as Euler angles.
  // This has no affect if tracking is enabled.
  public void SetUserWandRotation(int user, Vector3 euler, bool localRotation = true) { if (IsUserValid(user)) GetUserWand(user).SetRotation(euler, localRotation); }

  // Set a user's glasses position.
  // This has no affect if tracking is enabled.
  public void SetUserGlassesPosition(int user, Vector3 position, bool localPosition = true) { if (IsUserValid(user)) GetUserWand(user).SetPosition(position, localPosition); }

  // Set a user's glasses orientation as a quaternion.
  // This has no affect if tracking is enabled.
  public void SetUserGlassesOrientation(int user, Quaternion orientation, bool localOrientation = true) { if (IsUserValid(user)) GetUserWand(user).SetOrientation(orientation, localOrientation); }

  // Set a user's glasses rotation using Euler angles.
  // This has no affect if tracking is enabled.
  public void SetUserGlassesRotation(int user, Vector3 euler, bool localRotation = true) { if (IsUserValid(user)) GetUserWand(user).SetRotation(euler, localRotation); }

  // Get a ray in global world space for the given users wand laser.
  public Ray GetUserWandRay(int user)
  {
    HoloTrackWand wand = GetUserWand(user);
    return wand != null ? wand.GetRay() : new Ray(Vector3.zero, Vector3.zero);
  }

  // Draw a line showing the users wand laser.
  public bool DrawUserWandLaser(int user, float length = 1, bool debugOnly = false)
  {
    HoloTrackWand wand = GetUserWand(user);
    if (wand == null)
      return false;
    wand.DrawLaser(GetUserColour(user), length, debugOnly);
    return true;
  }

  // Get a user's colour
  public Color GetUserColour(int user) { return IsUserValid(user) ? m_userColours[user] : Color.black; }

  // Set a user's control mode
  public bool SetUserControlMode(int user, ControlMode mode)
  {
    if (!IsUserValid(user))
      return false;
    m_users[user].controls.Mode = mode;
    return true;
  }

  // Get a user's active control mode
  public ControlMode GetUserControlMode(int user)
  {
    return IsUserValid(user) ? m_users[user].controls.Mode : ControlMode.None;
  }

  public HoloViewer GetViewer()
  {
    return DeviceManager.Viewer;
  }

  public HoloRenderCave GetRenderCave()
  {
    return m_renderCave;
  }


  // ** INTERNAL **

  void OnEnable()
  {
    Initialise();
  }

  void Initialise()
  {
    if (m_initialised)
      return;

    // Find the config script
    m_initialiseAttempted = true;

    if (!Application.isEditor)
    { // Final build so make sure tracking/rendering is enabled
      DeviceConfig.EnableDeviceRender = true;
      DeviceConfig.EnableTracking = true;
      DeviceConfig.RenderDebugWindow = false;

      // Make sure the VRPN and display are on the local device
      DeviceConfig.RemoteDisplay = false;
      DeviceConfig.UseDeviceIPAsTrackingSource = false;
      DeviceConfig.TrackingSourceIP = "localhost";

      // Ensure the device is always rendering in final builds
      DeviceConfig.AlwaysRender = true;
      DeviceConfig.OnlyRenderUser1 = false;

      // Disable debug helpers
      DeviceConfig.EnableDebugUserController = false;
      DeviceConfig.DebugSurfaceView = -1;
    }

    if (!FindDependencies())
    {
      Debug.Log("Holo Device: Failed to find dependencies");
      return; // Failed to find dependencies
    }

    // Create an events device
    m_trackingEvents = gameObject.AddComponent<HoloTrackEvents>();

    // Enable/Disable tracking
    UpdateVRPNDevices();

    // Set the active HoloDevice script after successfully initialising
    if (active == null)
      active = this;

    // Add an audio listener for this device if one doesn't exist in the scene
    AudioListener[] listeners = FindObjectsOfType<AudioListener>();
    if (DeviceConfig.AddAudioListeners && listeners.Length == 0)
    {
      GameObject newChild = new GameObject("AudioListener");
      newChild.AddComponent<AudioListener>();
      newChild.transform.parent = active.m_device.transform;
    }
    else
    {
      Debug.LogWarning("Found " + listeners.Length + " audio listeners. HoloToolkit will add appropriate Audio Listeners for the device if you remove the existing Listeners in the Scene Hierarchy.");
    }

    // Activate tap/tare screen
    if (!DeviceConfig.EnableTracking || (Application.isEditor && !DeviceConfig.RemoteDisplay))
      DeviceConfig.ShowCalibrationScreen = false;

    Transform holoCanvas = m_device.transform.Find("HoloUICanvas");
    if (holoCanvas)
    {
      CalibrationController calibController = holoCanvas.GetComponentInChildren<CalibrationController>(true);
      if (calibController != null)
        calibController.gameObject.SetActive(DeviceConfig.ShowCalibrationScreen);
    }

    DeviceManager.Initialise();

    m_initialised = true;
  }

  // Update is called every frame
  //
  // This function updates the HoloDevice based on changes to the HoloConfig script at runtime.
  void Update()
  {
    if (!m_initialiseAttempted)
      Initialise();

    if (!m_initialised)
      return;

    active = this;

    if (DeviceManager.IsViewerActive())
    {
      if (!m_renderCaveInitialized)
      {
        m_renderCave.Init(DeviceManager.Viewer);
        m_renderCaveInitialized = true;
      }

      m_renderCave.m_invert3D = DeviceConfig.Invert3D;
      m_renderCave.m_autoAdjustResolution = DeviceConfig.AutoAdjustResolution;
      m_renderCave.enabled = DeviceConfig.EnableDeviceRender;
    }

    // Update VRPN device settings
    if (DeviceConfig.EnableTracking != m_lastEnabledTracking)
      UpdateVRPNDevices();

    UpdateUsers();
  }

  // This is an internal function which initialises the script/gameobject
  // references that the HoloDevice script depends on
  private bool FindDependencies()
  {
    if (!FindDevice())
      return false;

    if (!FindUsers())
      return false;

    LinkUserSurfaces();
    return true;
  }

  // This function finds the device selected in the config script and activates it
  private bool FindDevice()
  {
    // Initialise the render cave
    Transform deviceList = transform.Find("Devices");
    if (deviceList == null)
      return false; // device list was not found
    
    // Iterate the available device configurations and enabled the correct one
    foreach (Transform child in deviceList)
    {
      GameObject device = child.gameObject;
      HoloRenderCave deviceCave = device.GetComponent<HoloRenderCave>();
      if (deviceCave.DeviceType == DeviceConfig.TargetDeviceType)
      { // Activate this devices cave setup
        device.SetActive(true);
        m_device = device;
        m_renderCave = device.GetComponent<HoloRenderCave>();
      }
      else
      { // Ensure all other devices are inactive
        device.SetActive(false);
      }
    }

    return m_renderCave != null;
  }

  // This function finds and caches the enabled users and the VRPN device scripts
  private bool FindUsers()
  {
    // Iterate the available users
    Transform userList = transform.Find("Users");
    if (userList == null)
      return false; // No Users

    m_users = new User[GetUserCount()];
    for (int i = 0; i < m_users.Length; ++i)
    {
      Transform user = userList.transform.Find(i.ToString());
      m_users[i].valid = false;
      if (user == null)
        continue;

      // Get and cache the user components
      m_users[i].wand = user.gameObject.GetComponentInChildren<HoloTrackWand>();
      m_users[i].glasses = user.gameObject.GetComponentInChildren<HoloTrackGlasses>();
      m_users[i].renderController = m_users[i].glasses.gameObject.GetComponent<HoloTrackRenderController>();
      m_users[i].wand.m_trackingSmoothed = DeviceConfig.SmoothTrackedPoints;
      m_users[i].glasses.m_trackingSmoothed = DeviceConfig.SmoothTrackedPoints;
      m_users[i].controls = user.gameObject.GetComponent<HoloControlMode>();
      m_users[i].controls.UserID = i;
      m_users[i].controls.Mode = DeviceConfig.StartingControlMode;
      m_users[i].controls.ControlModeButton = DeviceConfig.ControlModeButton;
      m_users[i].controls.FlySpeed = DeviceConfig.StartingControlModeSpeed;
      m_users[i].controls.ShowLaser = DeviceConfig.ShowUserLasers;
      m_users[i].valid = true;
    }

    m_debugController = userList.GetComponentInChildren<DebugUserController>(true);
    return true;
  }

  // This function will attempt to link a users render controller to a surface on the active device.
  private void LinkUserSurfaces()
  {
    for (int i = 0; i < m_users.Length; ++i)
    {
      if (m_users[i].valid)
      {
        foreach (HoloRenderSurface surface in m_renderCave.GetSurfaces(i))
          m_users[i].renderController.AddSurface(surface);
      }
    }
  }

  // This function enables/disables all VRPN devices based on the HoloConfig script
  private void UpdateVRPNDevices()
  {
    string vrpnServer = DeviceConfig.UseDeviceIPAsTrackingSource ? DeviceConfig.DeviceIP : DeviceConfig.TrackingSourceIP;

    // Setup the VRPN server
    for (int i = 0; i < m_users.Length; ++i)
    {
      if (!m_users[i].valid)
        continue;

      // Tracking enabled
      m_users[i].wand.m_enabled = DeviceConfig.EnableTracking;
      m_users[i].glasses.m_enabled = DeviceConfig.EnableTracking;

      // Server location
      m_users[i].wand.m_server = vrpnServer;
      m_users[i].glasses.m_server = vrpnServer;
    }

    m_trackingEvents.m_enabled = DeviceConfig.EnableTracking;
    m_trackingEvents.m_server = vrpnServer;

    m_lastEnabledTracking = DeviceConfig.EnableTracking;
  }

  private void UpdateUsers()
  {
    for (int userID = 0; userID < m_users.Length; ++userID)
    {
      User user = m_users[userID];
      if (!user.valid)
        continue;

      bool useTrackingState = !DeviceConfig.AlwaysRender && DeviceConfig.EnableTracking;
      if (DeviceConfig.OnlyRenderUser1)
      {
        user.renderController.SetUseOverride(!useTrackingState || userID != 0);
        user.renderController.SetOverrideState(userID == 0);
      }
      else
      {
        user.renderController.SetUseOverride(!useTrackingState);
        user.renderController.SetOverrideState(!DeviceConfig.OnlyRenderUser1);
      }
    }

    if (m_debugController)
    {
      m_debugController.gameObject.SetActive(DeviceConfig.EnableDebugUserController);
      m_debugController.m_User = DeviceConfig.DebugUserContollerTarget;
      m_debugController.SimulateDisplayBounds = DeviceConfig.UserContollerSimulateDevice;
      DebugUserViewEmulation viewEmulation = m_debugController.gameObject.GetComponent<DebugUserViewEmulation>();
      viewEmulation.ViewPosition = DeviceConfig.DebugUserViewEmulationPosition;
      viewEmulation.HeadHeight = DeviceConfig.DebugUserViewHeight;
    }
  }

  private HoloDeviceManager CreateDeviceManager()
  {
    HoloDeviceManager manager = Instantiate(DeviceConfig.DeviceManagerPrefab).GetComponent<HoloDeviceManager>();
    DontDestroyOnLoad(manager.gameObject);
    return manager;
  }

  // Internal structures and references
  private struct User
  {
    public bool valid;
    public HoloTrackWand wand;
    public HoloTrackGlasses glasses;
    public HoloControlMode controls;
    public HoloTrackRenderController renderController;
  }

  // Device state
  private bool m_initialised = false;
  private bool m_initialiseAttempted = false;
  private bool m_lastEnabledTracking = true;
  private bool m_renderCaveInitialized = false;

  // Device
  private GameObject m_device = null;

  // Configuration
  private HoloTrackEvents m_trackingEvents = null;

  // Render Cave Scripts
  private HoloRenderCave m_renderCave = null;

  // Users
  private User[] m_users = null;
  private Color[] m_userColours = { Color.blue, Color.green };

  // Debug
  private DebugUserController m_debugController = null;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
[CustomEditor(typeof(HoloConfig))]
public class HoloConfig_Editor : Editor
{
  int activeTab = 0;
  HoloRenderCave activeDeviceCave = null;
  int numUsers = 0;

  GUIStyle bold = new GUIStyle();

  SerializedProperty DeviceManagerPrefab;

  SerializedProperty TargetDeviceType;
  SerializedProperty DeviceIP;
  SerializedProperty DevicePort;

  SerializedProperty EnableTracking;
  SerializedProperty UseDeviceIPAsTrackingSource;
  SerializedProperty TrackingSourceIP;

  SerializedProperty EnableDeviceRender;
  SerializedProperty RemoteDisplay;
  SerializedProperty AutoAdjustResolution;

  SerializedProperty StartingControlMode;
  SerializedProperty StartingControlModeSpeed;
  SerializedProperty ControlModeButton;
  SerializedProperty AddAudioListeners;

  SerializedProperty SmoothTrackedPoints;
  SerializedProperty ShowCalibrationScreen;

  SerializedProperty ShowUserLasers;
  SerializedProperty DebugSurfaceView;

  SerializedProperty AlwaysRender;
  SerializedProperty OnlyRenderUser1;
  SerializedProperty HDRCompensation;
  SerializedProperty RenderDebugWindow;
  SerializedProperty ProjectorMode;

  SerializedProperty EnableDebugUserController;
  SerializedProperty DebugUserContollerTarget;
  SerializedProperty UserContollerSimulateDevice;
  SerializedProperty DebugUserViewEmulationPosition;
  SerializedProperty DebugUserViewHeight;

  SerializedProperty Invert3D;
  SerializedProperty MaxUsers;

  void OnEnable()
  {
    bold.fontStyle = FontStyle.Bold;
    if (EditorGUIUtility.isProSkin)
      bold.normal.textColor = Color.white;

    DeviceManagerPrefab = serializedObject.FindProperty("DeviceManagerPrefab");

    TargetDeviceType = serializedObject.FindProperty("TargetDeviceType");
    DeviceIP = serializedObject.FindProperty("DeviceIP");
    DevicePort = serializedObject.FindProperty("DevicePort");

    EnableTracking = serializedObject.FindProperty("EnableTracking");
    UseDeviceIPAsTrackingSource = serializedObject.FindProperty("UseDeviceIPAsTrackingSource");
    TrackingSourceIP = serializedObject.FindProperty("TrackingSourceIP");

    EnableDeviceRender = serializedObject.FindProperty("EnableDeviceRender");
    RemoteDisplay = serializedObject.FindProperty("RemoteDisplay");

    StartingControlMode = serializedObject.FindProperty("StartingControlMode");
    StartingControlModeSpeed = serializedObject.FindProperty("StartingControlModeSpeed");
    ControlModeButton = serializedObject.FindProperty("ControlModeButton");
    AddAudioListeners = serializedObject.FindProperty("AddAudioListeners");

    SmoothTrackedPoints = serializedObject.FindProperty("SmoothTrackedPoints");
    ShowCalibrationScreen = serializedObject.FindProperty("ShowCalibrationScreen");

    ShowUserLasers = serializedObject.FindProperty("ShowUserLasers");
    DebugSurfaceView = serializedObject.FindProperty("DebugSurfaceView");

    AlwaysRender = serializedObject.FindProperty("AlwaysRender");
    OnlyRenderUser1 = serializedObject.FindProperty("OnlyRenderUser1");
    HDRCompensation = serializedObject.FindProperty("HDRCompensation");
    RenderDebugWindow = serializedObject.FindProperty("RenderDebugWindow");
    ProjectorMode = serializedObject.FindProperty("ProjectorMode");

    EnableDebugUserController = serializedObject.FindProperty("EnableDebugUserController");
    DebugUserContollerTarget = serializedObject.FindProperty("DebugUserContollerTarget");
    UserContollerSimulateDevice = serializedObject.FindProperty("UserContollerSimulateDevice");
    DebugUserViewEmulationPosition = serializedObject.FindProperty("DebugUserViewEmulationPosition");
    DebugUserViewHeight = serializedObject.FindProperty("DebugUserViewHeight");

    Invert3D = serializedObject.FindProperty("Invert3D");
    AutoAdjustResolution = serializedObject.FindProperty("AutoAdjustResolution");
    MaxUsers = serializedObject.FindProperty("MaxUsers");
  }

  static void HorizontalLine(Color color, float height = 2.0f)
  {
    GUIStyle horizontalLine;
    horizontalLine = new GUIStyle();
    horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
    horizontalLine.margin = new RectOffset(0, 0, 4, 4);
    horizontalLine.fixedHeight = height;

    var c = GUI.color;
    GUI.color = color;
    GUILayout.Box(GUIContent.none, horizontalLine);
    GUI.color = c;
  }

  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    activeDeviceCave = null;
    
    // Iterate the available device configurations and enabled the correct one
    foreach (HoloRenderCave deviceCave in Resources.FindObjectsOfTypeAll(typeof(HoloRenderCave)))
    {
      deviceCave.gameObject.SetActive((int)deviceCave.DeviceType == TargetDeviceType.intValue);
      if ((int)deviceCave.DeviceType == TargetDeviceType.intValue)
        activeDeviceCave = deviceCave;
    }

    numUsers = Mathf.Min(MaxUsers.intValue, HoloDevice.GetNumSupportedUsers((HoloDevice.Devices)TargetDeviceType.intValue));

    // Find the users and ensure only `numUsers` are enabled
    foreach (HoloTrackGlasses device in Resources.FindObjectsOfTypeAll(typeof(HoloTrackGlasses)))
      device.transform.parent.gameObject.SetActive(device.m_id < numUsers);

    activeTab = GUILayout.Toolbar(activeTab, new string[] { "Device", "Tracking", "Rendering", "Controls", "Debugging" });
    switch (activeTab)
    {
      case 0: DrawDeviceOptions(); break;
      case 1: DrawTrackingOptions(); break;
      case 2: DrawRenderingOptions(); break;
      case 3: DrawControlsOptions(); break;
      case 4: DrawDebuggingOptions(); break;
    }

    serializedObject.ApplyModifiedProperties();
  }

  void DrawDeviceOptions()
  {
    HorizontalLine(Color.grey);
    // Hologram Device Config
    EditorGUILayout.PropertyField(TargetDeviceType);

    EditorGUI.BeginChangeCheck();
    EditorGUILayout.PropertyField(DeviceIP);
    if (EditorGUI.EndChangeCheck())
      RemoteDisplay.boolValue = true;

    EditorGUILayout.PropertyField(DevicePort);
    HorizontalLine(Color.grey);
    EditorGUILayout.PropertyField(DeviceManagerPrefab);
  }

  void DrawTrackingOptions()
  {
    HorizontalLine(Color.grey);
    EditorGUILayout.LabelField("Tracking", bold);

    EditorGUILayout.PropertyField(EnableTracking);
    if (EnableTracking.boolValue)
    {
      EditorGUILayout.PropertyField(UseDeviceIPAsTrackingSource);
      if (!UseDeviceIPAsTrackingSource.boolValue)
      {
        EditorGUILayout.PropertyField(TrackingSourceIP);
      }
    }

    if (EnableTracking.boolValue)
    {
      EditorGUILayout.PropertyField(SmoothTrackedPoints);
      EditorGUILayout.PropertyField(ShowCalibrationScreen);
    }
  }

  void DrawRenderingOptions()
  {
    HorizontalLine(Color.grey, 1.0f);

    EditorGUILayout.PropertyField(EnableDeviceRender);
    if (EnableDeviceRender.boolValue)
    {
      EditorGUILayout.PropertyField(RemoteDisplay);

      if (RemoteDisplay.boolValue)
      {
        EditorGUILayout.PropertyField(AutoAdjustResolution);
      }
    }

    if (EnableDeviceRender.boolValue)
    {
      HorizontalLine(Color.grey);
      EditorGUILayout.LabelField("Rendering Config", bold);
      if (EnableTracking.boolValue)
        EditorGUILayout.PropertyField(AlwaysRender);
      EditorGUILayout.PropertyField(OnlyRenderUser1);
      EditorGUILayout.PropertyField(HDRCompensation);
      EditorGUILayout.PropertyField(RenderDebugWindow);

      EditorGUILayout.LabelField("Advanced", bold);
      EditorGUILayout.PropertyField(ProjectorMode);
    }
  }

  void DrawControlsOptions()
  {
    HorizontalLine(Color.grey);
    EditorGUILayout.LabelField("Control Modes", bold);
    EditorGUILayout.PropertyField(StartingControlMode);
    if (StartingControlMode.intValue >= 0)
    {
      EditorGUILayout.PropertyField(StartingControlModeSpeed);
      EditorGUILayout.PropertyField(ControlModeButton);
    }

    HorizontalLine(Color.grey);
    EditorGUILayout.LabelField("Audio", bold);
    EditorGUILayout.PropertyField(AddAudioListeners);

  }

  void DrawDebuggingOptions()
  {
    HorizontalLine(Color.grey);
    EditorGUILayout.LabelField("Debugging", bold);
    EditorGUILayout.PropertyField(ShowUserLasers);

    List<string> userOptions = new List<string>();
    userOptions.Capacity = numUsers;
    for (int i = 1; i <= numUsers; ++i)
      userOptions.Add(i.ToString());

    // Debug surface
    EditorGUI.BeginDisabledGroup(EnableDebugUserController.boolValue); // DebugUserController controls the active surfaces

    List<string> debugSurfaceViewOptions = new List<string>(userOptions);
    debugSurfaceViewOptions.Insert(0, "Disabled");

    // Find an active surface, and store which user it belongs to.
    HoloRenderSurface[] surfaceScripts = activeDeviceCave.GetSurfaces();
    int userDebugSurfaceSelected = -1; // This is the user that the active surface belongs to
    for (int userID = 0; userID < activeDeviceCave.GetUserCount(); ++userID)
      foreach (HoloRenderSurface surface in activeDeviceCave.GetSurfaces(userID))
        if (surface.gameObject.activeInHierarchy)
        {
          userDebugSurfaceSelected = userID;
          break;
        }

    // Accept user input
    int optionSelected = EditorGUILayout.Popup("Debug Surface View", userDebugSurfaceSelected + 1, debugSurfaceViewOptions.ToArray());
    userDebugSurfaceSelected = optionSelected - 1;

    DebugSurfaceView.intValue = userDebugSurfaceSelected;

    // Apply to surface objects
    for (int userID = 0; userID < activeDeviceCave.GetUserCount(); ++userID)
      for (int surfaceIndex = 0; surfaceIndex < surfaceScripts.Length; ++surfaceIndex)
        foreach (HoloRenderSurface surface in activeDeviceCave.GetSurfaces(userID))
          surface.gameObject.SetActive(userID == userDebugSurfaceSelected);

    EditorGUI.EndDisabledGroup(); // End debug surface selection

    EditorGUILayout.PropertyField(EnableDebugUserController);

    if (EnableDebugUserController.boolValue)
    {
      if (numUsers > 1)
        DebugUserContollerTarget.intValue = EditorGUILayout.Popup("Controller Target User", DebugUserContollerTarget.intValue, userOptions.ToArray());

      EditorGUILayout.PropertyField(UserContollerSimulateDevice);
      if (UserContollerSimulateDevice.boolValue)
      {
        DebugUserViewEmulationPosition.floatValue = EditorGUILayout.Slider("User Controller Position", DebugUserViewEmulationPosition.floatValue, -1.0f, 1.0f);
        EditorGUILayout.PropertyField(DebugUserViewHeight);
      }
    }

    // Internal Settings
    HorizontalLine(Color.grey);
    EditorGUILayout.LabelField("Internal Settings", bold);
    EditorGUILayout.PropertyField(Invert3D);
    EditorGUILayout.PropertyField(MaxUsers);

    EditorGUILayout.LabelField("About", bold);
    HorizontalLine(Color.grey);

    HoloConfig holoConfig = (HoloConfig)target;
    EditorGUILayout.LabelField("Version", holoConfig.Version);
  }
}

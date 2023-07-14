using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloDeviceManager : MonoBehaviour
{
  public HoloConfig DeviceConfig
  {
    get
    {
      if (m_config == null)
        m_config = HoloDevice.active.DeviceConfig;
      return m_config;
    }
  }

  public HoloViewer Viewer
  {
    get
    {
      if (m_viewer == null)
        m_viewer = gameObject.GetComponent<HoloViewer>();
      return m_viewer;
    }
  }

  private HoloConfig m_config = null;
  private HoloViewer m_viewer = null;
  private bool m_rendererInitialised = false;
  private bool m_initialized = false;

  private bool InitialiseRenderCave()
  {
    if (!DeviceConfig.EnableDeviceRender)
      return true; // Device render is not enabled so don't do anything

    // Setup the viewer
    bool createDisplay = DeviceConfig.RenderDebugWindow || DeviceConfig.RemoteDisplay || !Application.isEditor;

    if (DeviceConfig.RenderDebugWindow)
    { // Debug display - we are rendering locally so don't allow fullscreen and let the window resize
      Viewer.m_fullscreen = false;
      Viewer.m_resizable = true;
    }

    if (DeviceConfig.RemoteDisplay)
    { // Remote display - we are rendering on a device so force fullscreen and not resizeable
      Viewer.m_fullscreen = true;
      Viewer.m_resizable = false;
    }

    if (!Application.isEditor)
    { // Final builds - rendering on the device so force fullscreen and not resizeable
      Viewer.m_fullscreen = true;
      Viewer.m_resizable = false;
      Viewer.m_quality = 1;
    }

    Viewer.Init(DeviceConfig.TargetDeviceType, createDisplay, DeviceConfig.RemoteDisplay, DeviceConfig.DeviceIP, DeviceConfig.DevicePort, DeviceConfig.ProjectorMode);

    m_rendererInitialised = true;
    return true;
  }

  public void Initialise()
  {
    if (m_initialized)
      return;

    if (!InitialiseRenderCave())
    {
      Debug.Log("Holo Device: Failed to initialise the Render Cave");
      return;
    }

    m_initialized = true;
  }
  
  void Update()
  {
    // Update render cave settings
    if (DeviceConfig.EnableDeviceRender && !m_rendererInitialised)
      InitialiseRenderCave();

    // HDR compensation is only needed if rendering locally as downloading HDR textures converts them to the rgb format requested.
    Viewer.m_hdr = !Viewer.IsRemote() && DeviceConfig.HDRCompensation;
  }

  public bool IsViewerActive()
  {
    return m_rendererInitialised;
  }

  public bool GetKeyDown(KeyCode key) { return m_viewer.Client != null && m_viewer.Client.KeyDown(key); }
  public bool GetKeyPressed(KeyCode key) { return m_viewer.Client != null && m_viewer.Client.KeyPressed(key); }
  public bool GetKeyReleased(KeyCode key) { return m_viewer.Client != null && m_viewer.Client.KeyReleased(key); }
  public double GetKeyDownTime(KeyCode key) { return m_viewer.Client != null ? m_viewer.Client.KeyDownTime(key) : 0; }

  public bool GetMouseDown(KeyCode mouse) { return m_viewer.Client != null && m_viewer.Client.MouseDown(mouse); }
  public bool GetMousePressed(KeyCode mouse) { return m_viewer.Client != null && m_viewer.Client.MousePressed(mouse); }
  public bool GetMouseReleased(KeyCode mouse) { return m_viewer.Client != null && m_viewer.Client.MouseReleased(mouse); }
  public double GetMouseDownTime(KeyCode mouse) { return m_viewer.Client != null ? m_viewer.Client.MouseDownTime(mouse) : 0; }

  public int GetMouseScroll() { return m_viewer.Client != null ? m_viewer.Client.MouseScroll() : 0; }
  public Vector2Int GetMousePosition() { return m_viewer.Client != null ? m_viewer.Client.MousePosition() : Vector2Int.zero; }
}

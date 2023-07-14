using System.Threading.Tasks;
using UnityEngine;

public class HoloViewer : MonoBehaviour
{
  // Network Settings
  public bool m_fullscreen = true;
  public bool m_resizable = true;
  public int m_quality = 50;
  public HoloView.API.Compression m_compression = HoloView.API.Compression.JPG;

  public int m_numConnectAttempts = 5;

  public bool m_hdr = false;

  public HoloView.Client Client { get { return m_client; } }

  public bool m_enableLog      = false;
  public string m_logDirectory = "./Logs/";

  private bool m_isRemote = false;
  private bool m_isActive = false;
  private bool m_connected = false;
  private HoloView.API.DeviceType m_targetDevice;
  private HoloView.API.ProjectorMode m_presentMode;
  private HoloView.Client m_client = null;

  private int m_lastQuality = 50;
  private HoloView.API.Compression m_lastCompression = HoloView.API.Compression.JPG;

  private Vector2Int m_externalDisplayPos;
  private Vector2Int m_externalDisplaySize;
  private bool m_hasExternalDisplay = false;

  private Task m_connectTask = null;

  private bool m_externalDisplayActive = false;
  static private bool m_cancelConnect = false;

  public void Update()
  {
    if (m_client != null && m_client.ExitFlag())
    {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
    }

    if (m_connectTask != null && m_connectTask.Status == TaskStatus.RanToCompletion)
    { // Once the connect task has completed, create the viewer

      if (m_connected)
      {
        m_client.SetCompressionMode((int)m_compression);
        m_client.SetCompressionQuality(m_quality);
        HoloView.API.ContextFlags createFlags = 0;
        createFlags |= m_fullscreen ? HoloView.API.ContextFlags.Fullscreen : 0;
        createFlags |= m_resizable ? HoloView.API.ContextFlags.Resizeable : 0;
        m_client.Create(m_targetDevice, "Viewer", -1, -1, createFlags);
        m_isRemote = true;
        m_isActive = true;
        FetchExternalDisplayInfo();
      }

      m_connectTask = null;
    }

    // Application.isPlaying cannot be used on a worker thread. This is a work around so that we
    // can stop trying to connect if Unity is no longer playing
    m_cancelConnect = !Application.isPlaying;

    if (!IsActive())
      return;

    m_client.SetHDREnabled(m_hdr);
    m_client.Update();

    // Update via the API if the values have changed
    if (m_lastCompression != m_compression) m_client.SetCompressionMode((int)m_compression);
    if (m_lastQuality != m_quality) m_client.SetCompressionQuality(m_quality);

    // Update the previous values
    m_lastQuality = m_quality;
    m_lastCompression = m_compression;
  }

  public bool IsRemote() { return m_isRemote && IsActive(); }
  public bool IsActive() { return m_isActive; }

  public void Init(HoloDevice.Devices deviceType, bool createDisplay, bool remoteDisplay, string serverIP, int serverPort, HoloView.API.ProjectorMode presentMode)
  {
    if (m_enableLog)
    {
      HoloView.API.SetLogFile(m_logDirectory + "Log - HoloViewer - " + System.DateTime.Now.ToString("s").Replace(':', '_') + ".txt");
      HoloView.API.SetLogEnabled(true);
    }

    HoloView.API.Startup();
    HoloView.API.SetShaderPath(Application.dataPath + "/EuclideonHoloDevice/Shaders/");

    m_targetDevice = (HoloView.API.DeviceType)deviceType;

    if (!createDisplay)
      return;

    if (remoteDisplay)
    { // Asynchronously connect so that unity doesn't freeze up
      m_client = new HoloView.NetClient();
      m_connectTask = Task.Run(() => ConnectTask(this, serverIP, serverPort));
    }
    else
    {
      m_client = new HoloView.LocalClient();
      HoloView.API.ContextFlags createFlags = 0;
      createFlags |= m_fullscreen ? HoloView.API.ContextFlags.Fullscreen : 0;
      createFlags |= m_resizable ? HoloView.API.ContextFlags.Resizeable : 0;
      m_client.Create(m_targetDevice, "Viewer", -1, -1, createFlags);
      m_isRemote = false;
      m_isActive = true;

      FetchExternalDisplayInfo();
    }
  }

  public bool ShouldRender()
  {
    return Client == null || (Client.FrameIndex() - Client.ServerFrameIndex() < 25);
  }

  public void Present(int surfaceID, HoloUtil.Eye eye, RenderTexture texture)
  {
    if (IsActive())
      m_client.SetSurfaceTex(surfaceID, (int)eye, texture);
  }

  // Returns true if the target device of this viewer has an external display attached
  public bool HasExternalDisplay() { return m_hasExternalDisplay; }

  // Returns true if the external display window has been created on the device
  public bool IsExternalDisplayOpen() { return m_externalDisplayActive; }

  // Set the texture being rendered to the external display
  public void SetExternalDisplayTex(RenderTexture texture)
  {
    if (!texture || !m_hasExternalDisplay)
      return;

    if (IsActive())
    {
      CreateExternalDisplay();
      m_client.ExternalDisplay_SetTex(texture);
    }
  }

  public void Swap()
  {
    if (IsActive())
      m_client.Swap();
  }

  public void OnDestroy()
  {
    m_cancelConnect = true;

    if (m_connectTask != null)
      m_connectTask.Wait();

    // Wait until the remaining events have been processed
    if (m_client != null)
    {
      CloseExternalDisplay();
      m_client.Close();
      m_client = null;
    }

    while (HoloView.API.hvClient_HasEvents()) ; // intentional empty while loop

    if (IsRemote())
      HoloTray.KillHoloView();
  }

  // Close the external display window
  public void CloseExternalDisplay()
  {
    if (m_externalDisplayActive)
      m_client.ExternalDisplay_Close();

    m_externalDisplayActive = false;
  }

  // Create the external display window
  protected void CreateExternalDisplay()
  {
    if (!m_externalDisplayActive && m_hasExternalDisplay)
    {
      m_client.ExternalDisplay_Create("External Display");
      m_client.ExternalDisplay_SetPosition(m_externalDisplayPos.x, m_externalDisplayPos.y);
      m_client.ExternalDisplay_SetSize(m_externalDisplaySize.x, m_externalDisplaySize.y);
      m_externalDisplayActive = true;
    }
  }

  // Get the size (in pixels) of the external display
  public Vector2Int GetExternalDisplaySize() { return m_externalDisplaySize; }

  // Get the position of the external display. This is in pixel coordinates on the virtual desktop.
  public Vector2Int GetExternalDisplayPosition() { return m_externalDisplayPos; }

  private void FetchExternalDisplayInfo()
  {
    m_hasExternalDisplay = false;
    int externalDisplayIdx = m_client.GetExternalDisplayIndex();

    if (externalDisplayIdx != -1)
    {
      int dispX, dispY, dispW, dispH;
      m_client.GetDisplayDetails(externalDisplayIdx, out dispX, out dispY, out dispW, out dispH);
      m_externalDisplayPos = new Vector2Int(dispX, dispY);
      m_externalDisplaySize = new Vector2Int(dispW, dispH);
      m_hasExternalDisplay = true;
    }
  }

  private static bool ConnectTask(HoloViewer holoView, string serverIP, int serverPort)
  {
    HoloTray.SetServer(serverIP);
    Debug.Log("HoloView: Launching server on the target device (" + serverIP + ")...");
    if (!HoloTray.LaunchHoloView(serverPort))
    {
      Debug.Log("HoloView: Failed to launch the server on the target device (" + serverIP + ")...");
      return false;
    }

    bool connected = false;
    Debug.Log("HoloView: Connecting to " + serverIP + ":" + serverPort);
    for (int i = 0; i < Mathf.Max(1, holoView.m_numConnectAttempts); ++i)
    {
      System.Threading.Thread.Sleep(100); // Give some time for the server to launch

      if ((holoView.m_client as HoloView.NetClient).Connect(serverIP, serverPort))
      {
        connected = true;
        break;
      }

      if (m_cancelConnect)
        break;

      Debug.Log("Attempt " + (i + 1) + " failed." + ((i == holoView.m_numConnectAttempts - 1) ? "" : " Trying again"));
    }

    if (!connected)
    {
      Debug.Log("HoloView: Failed to connect to " + serverIP + ":" + serverPort);
      HoloTray.KillHoloView();
      return false;
    }

    Debug.Log("HoloView: Successfully connected to " + serverIP + ":" + serverPort);

    holoView.m_connected = true;
    return true;
  }
}

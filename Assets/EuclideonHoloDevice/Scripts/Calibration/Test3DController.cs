using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test3DController
  : CalibrationStep
  , IHoloRenderPreRenderEyeHandler
  , IHoloRenderPostRenderEyeHandler
  , IHoloRenderPreRenderUserHandler
  , IHoloRenderPostRenderUserHandler
{
  [Serializable]
  public struct PerEyeObject
  {
    [SerializeField]
    public HoloUtil.Eye eye;

    [SerializeField]
    public GameObject go;
  }

  [Serializable]
  public struct PerUserObject
  {
    [SerializeField]
    public int UserID;

    [SerializeField]
    public GameObject go;
  }

  public List<PerEyeObject>  perEyeObjects;
  public List<PerUserObject> perUserObjects;

  public LineRenderer laserTestRenderer;
  public Vector3      laserHeadOffset = new Vector3(0, -0.5f, 0);

  private List<KeyValuePair<GameObject, bool>> m_lastActivePerEye  = new List<KeyValuePair<GameObject, bool>>();
  private List<KeyValuePair<GameObject, bool>> m_lastActivePerUser = new List<KeyValuePair<GameObject, bool>>();

  private int m_userInteracting                   = -1;
  private DateTime m_pressedTime                  = new DateTime();
  private HoloTrackDevice.Buttons m_pressedButton = HoloTrackDevice.Buttons.None;
  private double m_invert3DHoldTime = 1;
  void OnEnable()
  {
    HoloRenderCallbacks.Add(this);
  }

  void OnDisable()
  {
    HoloRenderCallbacks.Remove(this);
  }

  // Start is called before the first frame update
  void Start()
  {
    laserTestRenderer.positionCount = 2;
  }

  // Update is called once per frame
  void Update()
  {
    laserTestRenderer.SetPosition(0, transform.position);

    // Check if any button is pressed
    for (int userID = 0; m_userInteracting == -1 && userID < HoloDevice.active.GetUserCount(); ++userID)
    {
      for (int button = 0; button < HoloTrackDevice.ButtonsCount; ++button)
      {
        if (HoloDevice.active.GetUserWand(userID).IsButtonPressed((HoloTrackDevice.Buttons)button))
        {
          m_pressedTime = DateTime.Now;
          m_pressedButton = (HoloTrackDevice.Buttons)button;
          m_userInteracting = userID;
          break;
        }
      }
    }

    if (m_userInteracting != -1)
    {
      if (HoloDevice.active.GetUserWand(m_userInteracting).IsButtonReleased(m_pressedButton))
      {
        CompleteStep();
      }

      if (HoloDevice.active.GetUserWand(m_userInteracting).IsButtonDown(m_pressedButton) && (DateTime.Now - m_pressedTime).TotalSeconds > m_invert3DHoldTime)
      {
        HoloConfig config = HoloDevice.active.DeviceConfig;
        config.Invert3D = !config.Invert3D;
        m_userInteracting = -1;
        m_pressedButton = HoloTrackDevice.Buttons.None;
      }
    }
  }

  public void OnPreRenderEye(int userID, HoloUtil.Eye eye)
  {
    foreach (PerEyeObject perEye in perEyeObjects)
    {
      if (perEye.go == null)
        continue;

      bool isActive = perEye.go.activeInHierarchy;
      if (isActive)
      {
        m_lastActivePerEye.Add(new KeyValuePair<GameObject, bool>(perEye.go, isActive));
        perEye.go.SetActive(eye == perEye.eye);
      }
    }
  }

  public void OnPostRenderEye(int userID, HoloUtil.Eye eye)
  {
    foreach (KeyValuePair<GameObject, bool> kvp in m_lastActivePerEye)
      kvp.Key.SetActive(kvp.Value);
    m_lastActivePerEye.Clear();
  }

  public void OnPreRenderUser(int userID)
  {
    laserTestRenderer.SetPosition(1, HoloDevice.active.GetUserGlasses(userID).transform.position + laserHeadOffset);
    laserTestRenderer.startColor = HoloDevice.active.GetUserColour(userID);
    laserTestRenderer.endColor = laserTestRenderer.startColor;
    laserTestRenderer.startWidth = 0.01f;
    laserTestRenderer.endWidth = 0.01f;
    foreach (PerUserObject perUser in perUserObjects)
    {
      if (perUser.go == null)
        continue;

      bool isActive = perUser.go.activeInHierarchy;
      if (isActive)
      {
        m_lastActivePerUser.Add(new KeyValuePair<GameObject, bool>(perUser.go, isActive));
        perUser.go.SetActive(userID == perUser.UserID);
      }
    }
  }

  public void OnPostRenderUser(int userID)
  {
    foreach (KeyValuePair<GameObject, bool> kvp in m_lastActivePerUser)
      kvp.Key.SetActive(kvp.Value);
    m_lastActivePerUser.Clear();
  }
}

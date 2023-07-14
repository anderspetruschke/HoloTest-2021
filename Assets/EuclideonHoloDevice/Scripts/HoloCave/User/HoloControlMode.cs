using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloControlMode : MonoBehaviour
{
  // Active control mode
  public HoloDevice.ControlMode Mode = HoloDevice.ControlMode.None;
  public float FlySpeed = 1.0f;
  public HoloTrackDevice.Buttons ControlModeButton = HoloTrackDevice.Buttons.Trigger;
  public bool ShowLaser = true;
  public int UserID = -1;

  // Which user is currently controlling the device
  public static int ActiveUser = -1;

  // User devices
  protected HoloTrackWand m_wand;
  protected HoloTrackGlasses m_glasses;

  // Control mode scripts
  protected HoloControlModeBase[] m_implementations;

  // Setup the available control modes
  void Start()
  {
    m_wand = GetComponentInChildren<HoloTrackWand>();
    m_glasses = GetComponentInChildren<HoloTrackGlasses>();
    m_implementations = new HoloControlModeBase[(int)HoloDevice.ControlModeCount];

    m_implementations[(int)HoloDevice.ControlMode.Wand] = new HoloControlModeWand();
    // m_implementations[(int)HoloDevice.ControlMode.Rocket] = new HoloControlModeRocket();
    // m_implementations[(int)HoloDevice.ControlMode.Psychic] = new HoloControlModePsychic();
    // m_implementations[(int)HoloDevice.ControlMode.Helicopter] = new HoloControlModeHelicopter();
    m_implementations[(int)HoloDevice.ControlMode.Orbit] = new HoloControlModeOrbit();
    m_implementations[(int)HoloDevice.ControlMode.Table] = new HoloControlModeTable();
    // m_implementations[(int)HoloDevice.ControlMode.Jump] = new HoloControlModeJump();
  }

  // Apply the user controls
  void Update()
  {
    if (Mode == HoloDevice.ControlMode.None || (ActiveUser != -1 && ActiveUser != UserID))
      return;

    if (m_implementations == null || m_wand == null || m_glasses == null) // Ensure things still work if the scripts are recompiled
      Start();

    if (m_implementations[(int)Mode] != null)
    {
      HoloTrackDevice.ButtonState actionButtonState = new HoloTrackDevice.ButtonState();
      switch (ControlModeButton)
      {
        case HoloTrackDevice.Buttons.Primary:
          actionButtonState.down = m_wand.IsButtonADown();
          actionButtonState.pressed = m_wand.IsButtonAPressed();
          actionButtonState.released = m_wand.IsButtonAReleased();
          break;
        case HoloTrackDevice.Buttons.Trigger:
          actionButtonState.down = m_wand.IsTriggerDown();
          actionButtonState.pressed = m_wand.IsTriggerPressed();
          actionButtonState.released = m_wand.IsTriggerReleased();
          break;
        case HoloTrackDevice.Buttons.Secondary:
          actionButtonState.down = m_wand.IsButtonBDown();
          actionButtonState.pressed = m_wand.IsButtonBPressed();
          actionButtonState.released = m_wand.IsButtonBReleased();
          break;
        default:
          actionButtonState.down = false;
          actionButtonState.pressed = false;
          actionButtonState.released = false;
          break;
      }

      if (m_implementations[(int)Mode].Apply(m_glasses, m_wand, actionButtonState, FlySpeed, ShowLaser))
        ActiveUser = UserID;
      else
        ActiveUser = -1;
    }
  }
}

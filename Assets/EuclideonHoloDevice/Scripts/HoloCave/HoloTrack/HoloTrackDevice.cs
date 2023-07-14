using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloTrackDevice : HoloTrack
{
  public HoloTrackDevice()
  {
    m_buttonState = new ButtonState[ButtonsCount + 1];
    for (int i = 0; i < m_buttonState.Length; ++i)
      m_buttonState[i] = new ButtonState();
    m_buttonOverride = new bool[ButtonsCount + 1];
    m_buttonOverrideSet = new bool[ButtonsCount + 1];
  }

  public enum Buttons
  {
    None = -1,
    Primary = 0,
    Trigger = 1,
    Secondary = 2,
  };
  public const int ButtonsCount = 3;

  public class ButtonState
  {
    public bool down = false;
    public bool pressed = false;
    public bool released = false;
  }

  protected ButtonState[] m_buttonState = null;
  protected bool[] m_buttonOverride = null;
  protected bool[] m_buttonOverrideSet = null;

  protected bool m_tared = false;
  protected bool m_initialised = false;

  public void SetPosition(Vector3 position, bool localPosition = false)
  {
    if (m_enabled)
      return;
    if (localPosition)
      transform.localPosition = position;
    else
      transform.position = position;
  }

  public void SetOrientation(Quaternion orientation, bool localOrientation = false)
  {
    if (!m_enabled)
    {
      if (localOrientation)
        transform.localRotation = orientation;
      else
        transform.rotation = orientation;
    }
  }

  public void SetRotation(Vector3 euler, bool localRotation = false)
  {
    if (m_enabled)
      return;
    if (localRotation)
      transform.localEulerAngles = euler;
    else
      transform.eulerAngles = euler;
  }

  // Check if the wand has tared
  public bool IsTared()
  {
    return m_tared;
  }

  // Override button states provided from tracking
  public void OverrideButtonState(Buttons button, bool down)
  {
    m_buttonOverride[(int)button + 1] = down;
    m_buttonOverrideSet[(int)button + 1] = true;
  }

  public bool IsButtonDown(Buttons button) { return m_buttonState[(int)button + 1].down; }
  public bool IsButtonPressed(Buttons button) { return m_buttonState[(int)button + 1].pressed; }
  public bool IsButtonReleased(Buttons button) { return m_buttonState[(int)button + 1].released; }

  // Updates the button state every frame
  protected void Update()
  {
    m_tared = false;

    Position();

    for (int i = 0; i < m_buttonState.Length; ++i)
    {
      bool newDown = Button(i);

      // Check for an override state
      if (m_buttonOverrideSet[i])
      {
        newDown = m_buttonOverride[i];
        m_buttonOverrideSet[i] = false;
      }

      m_buttonState[i].pressed = !m_buttonState[i].down && newDown;
      m_buttonState[i].released = m_buttonState[i].down && !newDown;
      m_buttonState[i].down = newDown;
    }

    if (m_initialised)
      m_tared = m_buttonState[0].pressed || m_buttonState[0].released;

    m_initialised = true;
  }
}

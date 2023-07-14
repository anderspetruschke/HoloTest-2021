using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloTrackEvents : HoloTrack
{
  public enum Event
  {
    Reserved,
    Status,
    RFIDTap,
    Count,
  }

  public override string GetUser() { return "Events"; }

  protected void Start()
  {
    for (int e = 0; e < (int)Event.Count; ++e)
      m_eventLastState[e] = Button(e);
  }

  protected bool[] m_eventLastState = new bool[(int)Event.Count];
  protected bool[] m_eventsReceived = new bool[(int)Event.Count];
  protected bool m_initialised = false;

  public bool IsTapped() { return EventReceived(Event.RFIDTap); }

  public bool EventReceived(Event e)
  {
    bool state = m_eventsReceived[(int)e];
    m_eventsReceived[(int)e] = false;
    return state;
  }

  protected void Update()
  { // Check buttons to see if events have been fired
    for (int e = 0; e < (int)Event.Count; ++e)
    {
      bool newState = Button(e);
      if (m_initialised)
        m_eventsReceived[e] |= newState != m_eventLastState[e];
      m_eventLastState[e] = newState;
    }

    m_initialised = true;
  }
}

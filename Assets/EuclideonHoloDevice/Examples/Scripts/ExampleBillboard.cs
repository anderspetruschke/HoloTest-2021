using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Billboard style GameObject
 * 
 * This is an example of how to use the HoloRender.Callbacks system to render
 * a GameObject so that it always faces the user. This works for all Users.
 * 
 * To use this script:
 *   1. Attach this script to a GameObject.
 **/

public class ExampleBillboard
  : MonoBehaviour
  , IHoloRenderPreRenderUserHandler  // Inherit from the IPreRenderUserHandler
                                     // so we can implement the OnPreRenderUser()
                                     // function.
  , IHoloRenderPostRenderUserHandler // Inherit from the IPostRenderUserHandler so
                                     // we can implement the OnPostRenderUser()
                                     // function.
{
  private Vector3 m_lastFwd = Vector3.zero;
  
  public void OnEnable()
  {
    // ! IMPORTANT !
    // We must register this component instance with HoloRenderCallbacks so
    // that HoloRender is able to call the implemented handlers.
    //
    // In this example this component is registered with HoloRenderCallbacks in the 
    // OnEnable function so that the callbacks are always enabled on startup,
    // but callbacks can be registered at any time.
    HoloRenderCallbacks.Add(this);
  }

  public void OnPostRenderUser(int userID)
  {
    // Restore the forward direction
    transform.forward = m_lastFwd;
  }

  public void OnPreRenderUser(int userID)
  {
    // Get the glasses object for the user that is about to be rendered.
    HoloTrackGlasses glasses = HoloDevice.active.GetUserGlasses(userID);

    // Get the direction from this GameObject, to the users head position.
    Vector3 viewDir = (glasses.transform.position - transform.position).normalized;

    // Store the original forward direction so we can restore it after the render.
    m_lastFwd = transform.forward;

    // Set the forward direction of this GameObject.
    transform.forward = viewDir;
  }
}

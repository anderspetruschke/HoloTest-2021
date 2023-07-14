using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Render a billboard style GameObject
 * 
 * This is an example of how to use the HoloRenderCallbacks system to render
 * a GameObject in only a specific eye of a user.
 * 
 * To use this script:
 *   1. Attach this script to a GameObject.
 *   2. Select the eye you would like the GameObject to be rendered to in the
 *      inspector.
 **/

public class ExampleRenderPerEye
  : MonoBehaviour
  , IHoloRenderPreRenderEyeHandler
  , IHoloRenderPostRenderEyeHandler
{
  public HoloUtil.Eye renderInEye;   // This is the eye that we should render
                                     // the GameObject to.

  private bool lastActive = false; // Used to store the previous Active state
                                     // of the GameObject.

  public void OnEnable()
  {
    // ! IMPORTANT !
    // We must register this component instance with HoloRenderCallbacks so
    // that HoloRender is able to call the implemented handlers.
    //
    // I chose to do this in the OnEnable function so that the callbacks are
    // always enabled on startup, but callbacks can be registered at any time.
    HoloRenderCallbacks.Add(this);
  }

  public void OnPreRenderEye(int userID, HoloUtil.Eye eye)
  {
    // Store the previous active state so we can restore it after the render.
    lastActive = gameObject.activeInHierarchy;

    // Set the GameObject to active if we are about to render the correct eye,
    // otherwise set it to inactive.
    gameObject.SetActive(eye == renderInEye);
  }

  public void OnPostRenderEye(int userID, HoloUtil.Eye eye)
  {
    // Restore the active state of the object once the eye has been rendered.
    gameObject.SetActive(lastActive);
  }
}

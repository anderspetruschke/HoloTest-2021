using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoloWandInputEventData : PointerEventData
{
  public int UserID;
  public HoloTrackWand Wand;

  public Ray worldSpaceRay;

  public Vector3 hitWorldPosition;
  public Vector3 hitWandPosPressed;
  public Vector3 hitWorldPositionPressed;

  public HoloWandInputEventData(EventSystem eventSystem)
    : base(eventSystem)
  { }
}

public static class PointerEventDataExtension
{
  public static bool IsWandEvent(this PointerEventData pointerEventData)
  {
    return (pointerEventData is HoloWandInputEventData);
  }

  public static Ray GetRay(this PointerEventData pointerEventData)
  {
    HoloWandInputEventData wandEvent = pointerEventData as HoloWandInputEventData;
    return wandEvent == null ? new Ray() : wandEvent.worldSpaceRay;
  }

  public static int GetUserID(this PointerEventData pointerEventData)
  {
    HoloWandInputEventData wandEvent = pointerEventData as HoloWandInputEventData;
    return wandEvent == null ? -1 : wandEvent.UserID;
  }

  public static HoloTrackWand GetWand(this PointerEventData pointerEventData)
  {
    HoloWandInputEventData wandEvent = pointerEventData as HoloWandInputEventData;
    return wandEvent == null ? null : wandEvent.Wand;
  }

  public static HoloTrackDevice.Buttons GetWandButton(this PointerEventData pointerEventData)
  {
    HoloWandInputEventData wandEvent = pointerEventData as HoloWandInputEventData;

    if (wandEvent != null)
      return wandEvent.Wand.MouseToWandButton(wandEvent.button);

    throw new UnityException("PointerEventData reference is not of type HoloWandInputEventData");
  }
}

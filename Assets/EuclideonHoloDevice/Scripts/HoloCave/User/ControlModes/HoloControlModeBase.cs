using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HoloControlModeBase
{
  // Implement the control mode
  // This should return false if the control is inactive and true when it is active (i.e. a user is controlling the table)
  public abstract bool Apply(HoloTrackGlasses glasses, HoloTrackWand wand, HoloTrackDevice.ButtonState actionButtonState, float flySpeed, bool showLaser);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloTrackGlasses : HoloTrackDevice
{
  public int m_id = 0;

  public override string GetUser()
  {
    return "Glasses" + m_id;
  }

  // Update is called once per frame
  new protected void Update()
  {
    base.Update();
    transform.localPosition = Position();
    transform.localRotation = Rotation();
  }
}

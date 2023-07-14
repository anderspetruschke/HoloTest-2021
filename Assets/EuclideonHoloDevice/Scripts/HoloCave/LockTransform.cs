using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LockTransform : MonoBehaviour
{
  public bool m_Active              = false;
  public bool m_LockPosition        = true;
  public bool m_LockRotation        = true;
  public bool m_LockScale           = true;

  public Vector3 m_LockedPosition = new Vector3(0, 0, 0);
  public Vector3 m_LockedRotation = new Vector3(0, 0, 0);
  public Vector3 m_LockedScale    = new Vector3(1, 1, 1);

  // Update is called once per frame
  void Update()
  {
    if (!m_Active)
      return;

    transform.localPosition = m_LockedPosition;
    transform.localScale = m_LockedScale;
    transform.localEulerAngles = m_LockedRotation;
  }
}

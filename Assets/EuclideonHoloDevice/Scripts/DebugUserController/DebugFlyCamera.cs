using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugFlyCamera : MonoBehaviour
{
  public float MoveAcceleration = 1.0f;
  public float MoveSpeed = 1.0f;
  public float MoveDeceleration = 3.0f;
  public float MouseSensitivity = 0.5f;

  float ApplyAcceleration(float value, float accel, float decel)
  {
    if (accel != 0)
    {
      value += accel;
    }
    else if (value != 0)
    {
      float sign = Mathf.Sign(value);
      value -= sign * decel;

      if (sign != Mathf.Sign(value))
        value = 0;
    }

    return value;
  }

  // Update is called once per frame
  void Update()
  {
    float deviceScale = HoloDevice.active.GetWorldScale();

    float accel = MoveAcceleration * Time.deltaTime * deviceScale;
    float decel = MoveDeceleration * Time.deltaTime * deviceScale;

    // Get acceleration based on key presses
    Vector3 moveAccel = new Vector3();
    if (Input.GetKey(KeyCode.W)) moveAccel.z += accel;
    if (Input.GetKey(KeyCode.S)) moveAccel.z -= accel;
    if (Input.GetKey(KeyCode.A)) moveAccel.x -= accel;
    if (Input.GetKey(KeyCode.D)) moveAccel.x += accel;
    if (Input.GetKey(KeyCode.E)) moveAccel.y += accel;
    if (Input.GetKey(KeyCode.Q)) moveAccel.y -= accel;

    // Apply acceleration
    m_moveVel.z = ApplyAcceleration(m_moveVel.z, moveAccel.z, decel);
    m_moveVel.y = ApplyAcceleration(m_moveVel.y, moveAccel.y, decel);
    m_moveVel.x = ApplyAcceleration(m_moveVel.x, moveAccel.x, decel);

    // Clamp velocity to a magnitude of 2
    m_moveVel = Vector3.ClampMagnitude(m_moveVel, MoveSpeed * deviceScale);
    transform.position += transform.forward * m_moveVel.z + transform.right * m_moveVel.x + transform.up * m_moveVel.y;

    // Look
    if (Input.GetMouseButtonDown(1))
      m_mousePos = Input.mousePosition;

    if (Input.GetMouseButton(1))
    {
      Vector3 rot = (Input.mousePosition - m_mousePos) * MouseSensitivity;
      Vector3 eulerAngles = transform.localEulerAngles;
      eulerAngles.x -= rot.y;
      eulerAngles.y += rot.x;
      transform.localEulerAngles = eulerAngles;
      m_mousePos = Input.mousePosition;
    }
  }

  Vector3 m_moveVel = new Vector3();
  Vector3 m_mousePos = new Vector3();
}

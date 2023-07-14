using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/**
 * Draggable GameObject Implementation
 * 
 * This is an example of basic interaction with GameObjects using the Wand. using
 * this script, you can drag a GameObject around the scene with the Wand.
 * 
 * To use this script:
 *   1. Ensure the GameObject you want to interact with has a valid Collider.
 *   2. Set the GameObject's Layer to 'UI' (This is the default Layer that the
 *      Wand will interact with).
 *   3. Attach this script to GameObject.
 *   4. Add a HoloEventSystem Prefab to the scene if one does not exist. You will
 *      find it in 'Assets/EuclideonHoloDevice/HoloEventSystem.prefab'
 **/

[RequireComponent(typeof(Collider))] // To interact with a non-UI GameObject using
                                     // the wand, it needs a Collider.
public class ExampleDraggable
  : MonoBehaviour        // Inherit from MonoBehaviour as this is a component.
  , IPointerDownHandler  // Inherit from IPointerDownHandler to receive OnPointerDown
                         // events from the HoloEventSystem.
                         //   NOTE: We could use IBeginDragHandler/OnBeginDrag here,
                         //         but for this example, I want to store the
                         //         wand/object positions from when the wand button
                         //         is pressed.
  , IDragHandler         // Inherit from IDragHandler to receive OnDrag events from
                         // the HoloEventSystem.
{
  // The offset from the intersect point in world space to the GameObject position.
  private Vector3 m_objectIntersectOffset = Vector3.zero;
 
  // The position of the wand when the button was first pressed.
  private Vector3 m_wandInitialPosition = Vector3.zero;

  // The distance from the wand to the intersect point when the button was first pressed.
  private float m_objectDistance = 0.0f;

  // This function is called once when a button on the wand is pushed
  public void OnPointerDown(PointerEventData eventData)
  {
    if (eventData.GetWandButton() != HoloTrackWand.Buttons.Primary)
      return;

    // Get the world space position of the point that was clicked on the GameObject.
    Vector3 clickedPosition = eventData.pointerPressRaycast.worldPosition;

    // Record the offset from the clicked position to the current world space position of the object.
    m_objectIntersectOffset = transform.position - clickedPosition;

    // Record the current world space position of the wand.
    m_wandInitialPosition = eventData.GetWand().transform.position;

    // Get the distance to the point that was clicked on the GameObject. We will want to
    // keep the object at this distance when it is being dragged.
    m_objectDistance = (eventData.GetWand().transform.position - clickedPosition).magnitude;
  }

  // When the wand is dragged, we need to update this components GameObject position.
  public void OnDrag(PointerEventData eventData)
  {
    if (eventData.GetWandButton() != HoloTrackWand.Buttons.Primary)
      return;

    // Get the wand that is dragging this GameObject.
    HoloTrackWand wand = eventData.GetWand();

    // Get a point along the wands ray that is 'm_objectDistance' away.
    // This is where we want the click intersection point to be moved to.
    Vector3 newIntersectPosition = wand.GetRay().GetPoint(m_objectDistance);

    // To calculate the new position of this GameObject, we can take the difference between
    // newIntersectPostion and m_clickedPosition, and add that to m_objectInitialPosition.
    Vector3 newPosition = m_objectIntersectOffset + newIntersectPosition;

    // Update this GameObject transform
    transform.position = newPosition;
  }
}

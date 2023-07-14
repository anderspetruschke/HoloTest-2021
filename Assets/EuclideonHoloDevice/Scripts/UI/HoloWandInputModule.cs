using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.EventSystems
{
  public class HoloWandInputModule : PointerInputModule
  {
    protected int m_mouseButtonCount = 3;
    protected Dictionary<int, HoloWandInputEventData> m_wandEventData = new Dictionary<int, HoloWandInputEventData>();

    // Get the event data for a users wand's button
    // Each event data is unique per button.
    protected HoloWandInputEventData GetWandEvent(int userID, PointerEventData.InputButton wandButton)
    {
      int dataID = HoloTrackWand.ButtonsCount * userID + (int)wandButton;
      HoloWandInputEventData eventData;
      if (!m_wandEventData.TryGetValue(dataID, out eventData))
      {
        eventData = new HoloWandInputEventData(eventSystem);
        eventData.UserID = userID;
        m_wandEventData.Add(dataID, eventData);
      }

      eventData.button = wandButton;
      return eventData;
    }

    public override void Process()
    {
      HoloDevice device = HoloDevice.active;

      // Process events for each user
      for (int userID = 0; userID < device.GetUserCount(); ++userID)
      {
        HoloTrackWand wand = device.GetUserWand(userID);
        MouseState state = GetWandEventData(userID, wand);

        for (int button = 0; button < m_mouseButtonCount; ++button)
        {
          PointerEventData.InputButton inputButton = (PointerEventData.InputButton)button;
          ProcessMouseState(state.GetButtonState(inputButton).eventData);
          ProcessMove(state.GetButtonState(inputButton).eventData.buttonData);
          ProcessDrag(state.GetButtonState(inputButton).eventData.buttonData);
        }
      }
    }

    // Translate the current HoloTrackWand state into a 'Mouse' state to be
    // used with Unity's regular UI event handlers.
    protected MouseState GetWandEventData(int userID, HoloTrackWand wand)
    {
      MouseState wandState    = new MouseState();
      Vector3 hitScreenPos    = new Vector3();
      RaycastResult hitResult = new RaycastResult();
      bool hit = false;
      bool doneRaycast = false;

      for (int i = 0; i < HoloTrackWand.ButtonsCount; ++i)
      {
        HoloTrackWand.Buttons wandButton = (HoloTrackWand.Buttons)i;
        HoloWandInputEventData wandButtonEvent = GetWandEvent(userID, wand.WandToMouseButton(wandButton));
        wandButtonEvent.worldSpaceRay = wand.GetRay();
        wandButtonEvent.Wand = wand;

        if (!doneRaycast)
        { // Only raycast for the first button
          List<RaycastResult> hitCandidates = new List<RaycastResult>();
          eventSystem.RaycastAll(wandButtonEvent, hitCandidates);
          hitResult = FindFirstRaycast(hitCandidates);

          HoloGraphicsRayCaster graphicsCaster = hitResult.module as HoloGraphicsRayCaster;

          if (graphicsCaster != null)
          {
            hitScreenPos = graphicsCaster.WorldToScreen(hitResult.worldPosition);
            hit = true;
          }
          else
          {
            RaycastHit physicsHit;
            if (Physics.Raycast(wand.GetRay(), out physicsHit, float.MaxValue, wand.m_UIInteractMask))
            {
              hit = true;
              hitResult = new RaycastResult();
              hitResult.distance = physicsHit.distance;
              hitResult.gameObject = physicsHit.collider.gameObject;
              hitResult.worldPosition = physicsHit.point;
              hitResult.worldNormal = physicsHit.normal;
              hitResult.module = null;
              hitResult.screenPosition = wand.EventCamera.WorldToScreenPoint(hitResult.worldPosition);
              hitScreenPos = hitResult.screenPosition;
            }
          }

          if (wand.m_UICursor)
          { // Enable the wands cursor and set it's position
            wand.m_UICursor.gameObject.SetActive(hit);
            wand.m_UICursor.transform.position = hitResult.worldPosition;
            if (wand.m_AlignCursorToSurface)
              wand.m_UICursor.transform.up = hitResult.worldNormal;
            wand.SetLaserHitDist(hit ? hitResult.distance : float.MaxValue);
          }

          doneRaycast = true;
        }

        // Set button event data
        if (hit)
          wandButtonEvent.position = hitScreenPos; // Only update the position on a 'hit'

        wandButtonEvent.pointerCurrentRaycast = hitResult;
        wandButtonEvent.hitWorldPosition = hitResult.worldPosition;

        HandlePointerExitAndEnter(wandButtonEvent, hitResult.gameObject);

        // Set the button in the wand state
        wandState.SetButtonState(wandButtonEvent.button, GetWandButtonState(wand, wandButton), wandButtonEvent);
      }

      return wandState;
    }

    protected PointerEventData.FramePressState GetWandButtonState(HoloTrackWand wand, HoloTrackWand.Buttons button)
    {
      bool pressed = wand.IsButtonPressed(button);
      bool released = wand.IsButtonReleased(button);

      if (pressed && released)
        return PointerEventData.FramePressState.PressedAndReleased;
      else if (pressed)
        return PointerEventData.FramePressState.Pressed;
      else if (released)
        return PointerEventData.FramePressState.Released;
      return PointerEventData.FramePressState.NotChanged;
    }


    protected void ProcessMouseState(MouseButtonEventData buttonData)
    {
      HoloWandInputEventData wandEvent = buttonData.buttonData as HoloWandInputEventData;
      GameObject hoveredGameObject = wandEvent.pointerCurrentRaycast.gameObject;

      if (buttonData.PressedThisFrame())
      {
        wandEvent.eligibleForClick = true;
        wandEvent.delta = Vector2.zero;
        wandEvent.dragging = false;
        wandEvent.useDragThreshold = true;
        wandEvent.pressPosition = wandEvent.position;
        wandEvent.pointerPressRaycast = wandEvent.pointerCurrentRaycast;
        wandEvent.hitWorldPositionPressed = wandEvent.hitWorldPosition;
        wandEvent.hitWandPosPressed = wandEvent.worldSpaceRay.origin;

        DeselectIfSelectionChanged(hoveredGameObject, wandEvent);

        GameObject newPressed = ExecuteEvents.ExecuteHierarchy(hoveredGameObject, wandEvent, ExecuteEvents.pointerDownHandler);

        if (newPressed == null)
          newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hoveredGameObject);

        float time = Time.unscaledTime;

        if (newPressed == wandEvent.lastPress)
        {
          var diffTime = time - wandEvent.clickTime;
          if (diffTime < 0.3f)
            ++wandEvent.clickCount;
          else
            wandEvent.clickCount = 1;

          wandEvent.clickTime = time;
        }
        else
        {
          wandEvent.clickCount = 1;
        }

        wandEvent.pointerPress = newPressed;
        wandEvent.rawPointerPress = hoveredGameObject;

        wandEvent.clickTime = time;

        wandEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(hoveredGameObject);

        if (wandEvent.pointerDrag != null)
          ExecuteEvents.Execute(wandEvent.pointerDrag, wandEvent, ExecuteEvents.initializePotentialDrag);
      }

      if (buttonData.ReleasedThisFrame())
      {
        ExecuteEvents.Execute(wandEvent.pointerPress, wandEvent, ExecuteEvents.pointerUpHandler);

        GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hoveredGameObject);

        if (wandEvent.pointerPress == pointerUpHandler && wandEvent.eligibleForClick)
          ExecuteEvents.Execute(wandEvent.pointerPress, wandEvent, ExecuteEvents.pointerClickHandler);
        else if (wandEvent.pointerDrag != null)
          ExecuteEvents.ExecuteHierarchy(hoveredGameObject, wandEvent, ExecuteEvents.dropHandler);

        wandEvent.eligibleForClick = false;
        wandEvent.pointerPress = null;
        wandEvent.rawPointerPress = null;

        if (wandEvent.pointerDrag != null && wandEvent.dragging)
          ExecuteEvents.Execute(wandEvent.pointerDrag, wandEvent, ExecuteEvents.endDragHandler);

        wandEvent.dragging = false;
        wandEvent.pointerDrag = null;

        if (hoveredGameObject != wandEvent.pointerEnter)
        {
          HandlePointerExitAndEnter(wandEvent, null);
          HandlePointerExitAndEnter(wandEvent, hoveredGameObject);
        }
      }
    }

    protected override void ProcessDrag(PointerEventData pointerEvent)
    {
      if (!pointerEvent.IsWandEvent())
        return;

      HoloWandInputEventData wandEvent = pointerEvent as HoloWandInputEventData;
      Vector3 pressedToPos = Vector3.Normalize(wandEvent.hitWorldPositionPressed - wandEvent.hitWandPosPressed);
      Vector3 currentToPos = Vector3.Normalize(wandEvent.hitWorldPosition - wandEvent.hitWandPosPressed);
      bool startDrag = Vector3.Dot(pressedToPos, currentToPos) < 0.9995;

      if (pointerEvent.pointerDrag != null && !pointerEvent.dragging && startDrag)
      {
        ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
        pointerEvent.dragging = true;
      }

      if (pointerEvent.dragging && pointerEvent.pointerDrag != null)
      {
        if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
        {
          ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
          pointerEvent.eligibleForClick = false;
          pointerEvent.pointerPress = null;
          pointerEvent.rawPointerPress = null;
        }

        ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
      }
    }

  }
}

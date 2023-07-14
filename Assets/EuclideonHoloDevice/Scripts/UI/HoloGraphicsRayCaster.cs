using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(RectTransform))]
public class HoloGraphicsRayCaster : GraphicRaycaster
{
  private Canvas m_canvas;
  private Canvas TargetCanvas
  {
    get
    {
      if (!m_canvas)
        m_canvas = gameObject.GetComponent<Canvas>();
      return m_canvas;
    }
  }

  private RectTransform m_rect;
  private RectTransform TargetCanvasRect
  {
    get
    {
      if (!m_rect)
        m_rect = gameObject.GetComponent<RectTransform>();
      return m_rect;
    }
  }

  public struct GraphicRaycastHit
  {
    public Graphic graphic;
    public Vector3 worldPos;
    public Vector3 worldNormal;
    public float   hitDist;
  };

  // Start is called before the first frame update
  new void Start()
  {
    base.Start();
  }

  public Vector3 WorldToScreen(Vector3 worldPos)
  {
    return eventCamera.WorldToScreenPoint(worldPos);
  }

  public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
  {
    if (!eventData.IsWandEvent())
      return;

    // Set the event camera to the wands event camera
    TargetCanvas.worldCamera = eventData.GetWand().EventCamera;

    Ray wandRay = eventData.GetRay();
    HoloTrackWand wand = eventData.GetWand();

    // Ray cast onto potential blocking objects
    float blockingDist = float.MaxValue;
    bool blockAll = blockingObjects == BlockingObjects.All;
    if (blockingObjects == BlockingObjects.ThreeD || blockAll)
    {
      RaycastHit hit;
      if (Physics.Raycast(wandRay, out hit, blockingDist, m_BlockingMask, QueryTriggerInteraction.UseGlobal))
        blockingDist = Mathf.Min(blockingDist, hit.distance);
    }

    if (blockingObjects == BlockingObjects.TwoD || blockAll)
    {
      RaycastHit2D hit = Physics2D.GetRayIntersection(wandRay, blockingDist, m_BlockingMask);
      blockingDist = Mathf.Min(blockingDist, hit.fraction * hit.distance);
    }

    // Raycast the graphics in this canvas
    List<GraphicRaycastHit> rayCastResults = new List<GraphicRaycastHit>();
    GraphicRaycast(wandRay, rayCastResults, wand.m_UIInteractMask);

    // Add hit graphics that are not blocked
    for (var index = 0; index < rayCastResults.Count; ++index)
    {
      GraphicRaycastHit hit = rayCastResults[index];

      bool validHit = ignoreReversedGraphics || hit.hitDist > 0;

      if (validHit && hit.hitDist < blockingDist)
      {
        RaycastResult castResult = new RaycastResult();
        castResult.gameObject = hit.graphic.gameObject;
        castResult.module = this;
        castResult.distance = hit.hitDist;
        castResult.index = resultAppendList.Count;
        castResult.depth = hit.graphic.depth;
        castResult.worldPosition = hit.worldPos;
        castResult.worldNormal = hit.worldNormal;
        castResult.sortingOrder = hit.graphic.canvas.sortingOrder;
        resultAppendList.Add(castResult);
      }
    }
  }

  // Ray cast onto the canvas and get the world position and screen position
  // of a hit.
  // Returns true if screenPos is within the canvas
  public bool Raycast(Ray ray, out float hitDist, out Vector3 worldPos, out Vector3 screenPos)
  {
    return Raycast(TargetCanvasRect, ray, out hitDist, out worldPos, out screenPos);
  }

  public bool Raycast(RectTransform rect, Ray ray, out float hitDist, out Vector3 worldPos, out Vector3 screenPos)
  {
    hitDist = float.MaxValue;
    Vector3[] corners = new Vector3[4];
    rect.GetWorldCorners(corners);
    Plane plane = new Plane(corners[0], corners[1], corners[2]);
    if (!plane.Raycast(ray, out hitDist))
    {
      worldPos = Vector3.zero;
      screenPos = Vector3.zero;
      return false;
    }

    worldPos = ray.GetPoint(hitDist);
    screenPos = eventCamera.WorldToScreenPoint(worldPos);
    return rect.rect.Contains(rect.InverseTransformPoint(worldPos));
  }

  public Vector3 GetRectTransformNormal(RectTransform rect)
  {
    Vector3[] corners = new Vector3[4];
    rect.GetWorldCorners(corners);
    Plane plane = new Plane(corners[0], corners[1], corners[2]);
    return plane.normal;
  }

  public void GraphicRaycast(Ray ray, List<GraphicRaycastHit> results, LayerMask mask)
  {
    IList<Graphic> graphics = GraphicRegistry.GetGraphicsForCanvas(TargetCanvas);

    for (int i = 0; i < graphics.Count; ++i)
    {
      Graphic graphic = graphics[i];

      if (((1 << graphic.gameObject.layer) & mask.value) == 0 || graphic.depth == -1)
        continue;

      Vector3 worldPos;
      Vector3 screenPos;
      float hitDist;
      if (!Raycast(graphic.rectTransform, ray, out hitDist, out worldPos, out screenPos))
        continue;

      if (!graphic.Raycast(screenPos, eventCamera))
        continue;

      GraphicRaycastHit hit;
      hit.graphic = graphic;
      hit.worldPos = worldPos;
      hit.hitDist = hitDist;
      hit.worldNormal = GetRectTransformNormal(graphic.rectTransform);
      results.Add(hit);
    }

    results.Sort((a, b) => b.graphic.depth.CompareTo(a.graphic.depth));
  }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using HoloUtil;

// Render Callback Interfaces
public interface IHoloRenderRenderHandler { }

public interface IHoloRenderPreRenderEyeHandler : IHoloRenderRenderHandler
{
  void OnPreRenderEye(int userID, Eye eyeID);
}

public interface IHoloRenderPostRenderEyeHandler : IHoloRenderRenderHandler
{
  void OnPostRenderEye(int userID, Eye eyeID);
}

public interface IHoloRenderPreRenderUserHandler : IHoloRenderRenderHandler
{
  void OnPreRenderUser(int userID);
}

public interface IHoloRenderPostRenderUserHandler : IHoloRenderRenderHandler
{
  void OnPostRenderUser(int userID);
}

// Render Callback Registry
public class HoloRenderCallbacks
{
  private static List<WeakReference> m_instances = new List<WeakReference>();

  public static WeakReference Add<T>(T handler) where T
    : class
    , IHoloRenderRenderHandler
  {
    // Try find an existing handle for handler
    WeakReference handlerRef = FindHandle(handler);
    if (handlerRef == null)
    { // If no handle was found, add a new one
      handlerRef = new WeakReference(handler, false);
      m_instances.Add(handlerRef);
    }

    // Return the handle
    return handlerRef;
  }

  public static WeakReference FindHandle<T>(T handler) where T
    : class
    , IHoloRenderRenderHandler
  {
    // Search for the weak reference to the handler
    foreach (WeakReference handle in m_instances)
      if (handle.Target as T == handler)
        return handle; // Found reference, return it
    return null; // handler is not registered.
  }

  public static void Remove(WeakReference handlerRef)
  {
    if (handlerRef != null)
      m_instances.Remove(handlerRef);
  }

  public static void Remove<T>(T handler) where T
    : class
    , IHoloRenderRenderHandler
  {
    Remove(FindHandle(handler));
  }

  public delegate void CallbackDelegate<T>(T o);

  public static void InvokePreRenderEye(int userID, Eye eyeID)
  {
    InvokeDelegate<IHoloRenderPreRenderEyeHandler>((o) => { o.OnPreRenderEye(userID, eyeID); });
  }

  public static void InvokePostRenderEye(int userID, Eye eyeID)
  {
    InvokeDelegate<IHoloRenderPostRenderEyeHandler>((o) => { o.OnPostRenderEye(userID, eyeID); });
  }

  public static void InvokePreRenderUser(int userID)
  {
    InvokeDelegate<IHoloRenderPreRenderUserHandler>((o) => { o.OnPreRenderUser(userID); });
  }

  public static void InvokePostRenderUser(int userID)
  {
    InvokeDelegate<IHoloRenderPostRenderUserHandler>((o) => { o.OnPostRenderUser(userID); });
  }

  public static void InvokeDelegate<T>(CallbackDelegate<T> func) where T : class
  {
    List<WeakReference> expiredReferences = new List<WeakReference>();

    // Call the delegate on all callbacks
    foreach (WeakReference callbackRef in m_instances)
    {
      T handler = callbackRef.Target as T;
      if (handler != null)
      { // The registered callback has implemented the interface, invoke the delegate
        func(handler);
      }
      else if (!callbackRef.IsAlive)
      { // The reference has expired, we need to clean remove it from m_instances
        expiredReferences.Add(callbackRef);
      }
    }

    // Cleanup expired references
    foreach (WeakReference expired in expiredReferences)
      m_instances.Remove(expired);
  }
}

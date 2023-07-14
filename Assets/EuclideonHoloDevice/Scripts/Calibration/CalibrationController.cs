using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationController : MonoBehaviour
{
  [Tooltip("You can change this logo sprite to your own logo.")]
  public Sprite logo;

  public Transform logoObject;
  public CalibrationStep tapTareScreen;
  public CalibrationStep test3DScreen;

  public Material UI_IgnoreDepth;

  public enum Screen
  {
    None,
    TapTare,
    Test3D,
    Count
  }

  private static bool isCalibrated;

  private Screen activeScreen = Screen.None;
  private CalibrationStep activeStep = null;

  public void ResetIsCalibrated()
  {
    isCalibrated = false;
  }

  public void OnEnable()
  {
    if (isCalibrated)
      gameObject.SetActive(false);

    // Set all children materials to the Ignore Depth material so they draw on top of the scene
    Graphic[] childrenImages = GetComponentsInChildren<Graphic>(true);
    foreach (Graphic child in childrenImages)
      child.material = UI_IgnoreDepth;

    // Disable all steps on startup
    test3DScreen.gameObject.SetActive(false);
    tapTareScreen.gameObject.SetActive(false);

    // Set the logo
    logoObject.GetComponent<Image>().sprite = logo;

    // Go to the first step
    SetScreen(Screen.TapTare);
  }

  public void Update()
  {
    // When the current step is complete, goto the next one.
    if (activeStep != null && activeStep.IsComplete)
    {
      gameObject.SetActive(GotoNextStep()); // Disable calibration if we have completed all steps.
      isCalibrated = true;
    }
  }

  public bool GotoNextStep()
  {
    Screen nextScreen = activeScreen + 1;
    if (nextScreen >= Screen.Count)
      return false; // At the last screen already, return false

    SetScreen(nextScreen);
    return true;
  }

  public void SetScreen(Screen screen)
  {
    if (activeScreen == screen)
      return; // Same screen, don't do anything

    if (activeStep != null) // Disable current step
      activeStep.gameObject.SetActive(false);

    // Get the next step
    switch (screen)
    {
      case Screen.TapTare: activeStep = tapTareScreen; break;
      case Screen.Test3D:  activeStep = test3DScreen;  break;
      default:             activeStep = null;          break;
    }

    // Enable the step
    activeStep.gameObject.SetActive(true);
    activeScreen = screen;
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationStep : MonoBehaviour
{
  public bool IsComplete { get; private set; }

  // Call this function once the calibration step is complete
  protected void CompleteStep()
  {
    IsComplete = true;
  }
}

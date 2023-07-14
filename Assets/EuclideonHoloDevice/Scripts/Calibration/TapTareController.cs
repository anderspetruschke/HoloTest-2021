using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapTareController : CalibrationStep
{
  public Transform TapHolder;
  public Transform TareHolder;
  public Transform SeenHolder;
  public Transform Tare2UserHolder;
  public Transform Seen2UserHolder;

  public Transform RFIDTapObject;
  public List<Sprite> RFIDSprites;

  public bool DoneCalibrating { get { return m_doneCalibrating; } }

  TapTareStage m_currentTapTareStage = TapTareStage.Tap;
  bool m_doneCalibrating = false;
  const int m_devicesPerPerson = 2;
  List<bool> m_devicesTared = new List<bool>();

  void OnEnable()
  {
    ShowTareScreen();
  }

  enum TapTareStage
  {
    Tap,
    Tare,
    Seen,
    Count
  }

  public void ShowTareScreen()
  {
    m_currentTapTareStage = TapTareStage.Tap;
    m_doneCalibrating = false;

    FadeChildrenImages(TapHolder, false);
    FadeChildrenImages(TareHolder, true);
    FadeChildrenImages(Tare2UserHolder, true);
    FadeChildrenImages(SeenHolder, true);
    FadeChildrenImages(Seen2UserHolder, true);

    TapHolder.gameObject.SetActive(true);
    TareHolder.gameObject.SetActive(true);
    SeenHolder.gameObject.SetActive(true);
    Tare2UserHolder.gameObject.SetActive(true);
    Seen2UserHolder.gameObject.SetActive(true);
  }

  void FadeHolder(TapTareStage stage, bool fadeOut)
  {
    bool singleUser = HoloDevice.active.GetUserCount() == 1;
    switch (stage)
    {
      case TapTareStage.Tap:
        FadeChildrenImages(TapHolder, fadeOut);
        break;
      case TapTareStage.Tare:
        if (singleUser)
        {
          FadeChildrenImages(TareHolder, fadeOut);
        }
        else
        {
          FadeChildrenImages(Tare2UserHolder, fadeOut);
        }
        break;
      case TapTareStage.Seen:
        if (singleUser)
        {
          FadeChildrenImages(SeenHolder, fadeOut);
        }
        else
        {
          FadeChildrenImages(Seen2UserHolder, fadeOut);
        }
        break;
    }
  }

  public void IncrementStep()
  {
    FadeHolder(m_currentTapTareStage, true);

    m_currentTapTareStage += 1;
    m_currentTapTareStage = (TapTareStage)((int)m_currentTapTareStage % (int)TapTareStage.Count);

    FadeHolder(m_currentTapTareStage, false);

    // Special logic for proceeding to Tare stage
    if (m_currentTapTareStage == TapTareStage.Tare)
    {
      m_devicesTared.Clear();
      // Add devices per user for storing tare state
      for (int i = 0; i < HoloDevice.active.GetUserCount() * m_devicesPerPerson; ++i)
        m_devicesTared.Add(false);
    }
  }

  void FadeChildrenImages(Transform parent, bool fadeOut)
  {
    float fadeLength = 1.2f;
    if (fadeOut)
      fadeLength *= 0.1f;
    float toAlpha = fadeOut ? 0.0f : 1.0f;
    foreach (Image childImage in parent.GetComponentsInChildren<Image>(true))
      childImage.CrossFadeAlpha(toAlpha, fadeLength, false);
    foreach (Text childText in parent.GetComponentsInChildren<Text>(true))
      childText.CrossFadeAlpha(toAlpha, fadeLength, false);
  }

  void Update()
  {
    if (m_doneCalibrating)
    {
      CompleteStep();
      return;
    }

    switch (m_currentTapTareStage)
    {
      case TapTareStage.Tap:
        TapUpdate();
        break;
      case TapTareStage.Tare:
        TareUpdate();
        break;
      case TapTareStage.Seen:
        SeenUpdate();
        break;
    }
  }

  bool AnyUserAnyButtonPressed()
  {
    int userCount = HoloDevice.active.GetUserCount();
    for (int userIndex = 0; userIndex < userCount; ++userIndex)
    {
      if (HoloDevice.active.GetUserWand(userIndex).IsButtonAPressed()) return true;
      if (HoloDevice.active.GetUserWand(userIndex).IsButtonBPressed()) return true;
      if (HoloDevice.active.GetUserWand(userIndex).IsTriggerPressed()) return true;
    }
    return false;
  }

  void TapUpdate()
  {
    // Animate icon
    const float tapInterval = 0.6f;
    int imageCount = RFIDSprites.Count;
    int whichImage = (int)Mathf.Floor(Mathf.Repeat(Time.time, tapInterval * imageCount) / tapInterval);
    RFIDTapObject.GetComponent<Image>().sprite = RFIDSprites[whichImage];

    // Check for Tap
    if (HoloDevice.active.GetTrackingEvents().IsTapped())
      IncrementStep();
  }

  void TareUpdate()
  {
    int userCount = HoloDevice.active.GetUserCount();

    // Acquire information about the tare state of the users' equipment
    for (int userIndex = 0; userIndex < userCount; ++userIndex)
    {
      if (HoloDevice.active.GetUserGlasses(userIndex).IsTared())
        m_devicesTared[0 + userIndex * 2] = true;
      if (HoloDevice.active.GetUserWand(userIndex).IsTared())
        m_devicesTared[1 + userIndex * 2] = true;
    }

    // Determine if we should continue, sufficient devices have tared
    bool singleUser = userCount == 1;
    bool allTared = false;
    if (singleUser)
      allTared = m_devicesTared[0] & m_devicesTared[1];
    else
      allTared = (m_devicesTared[0] & m_devicesTared[1]) || (m_devicesTared[2] & m_devicesTared[3]);

    if (allTared)
      IncrementStep();

    // Update graphics
    if (singleUser)
    {
      Transform tareG1n = TareHolder.Find("TareGlassesNo");
      Transform tareG1y = TareHolder.Find("TareGlassesYes");
      Transform tareW1n = TareHolder.Find("TareWandNo");
      Transform tareW1y = TareHolder.Find("TareWandYes");

      tareG1n.gameObject.SetActive(!m_devicesTared[0]);
      tareG1y.gameObject.SetActive(m_devicesTared[0]);
      tareW1n.gameObject.SetActive(!m_devicesTared[1]);
      tareW1y.gameObject.SetActive(m_devicesTared[1]);
    }
    else
    {
      Transform tareG1n = Tare2UserHolder.Find("TareGlassesNo");
      Transform tareG1y = Tare2UserHolder.Find("TareGlassesYes");
      Transform tareW1n = Tare2UserHolder.Find("TareWandNo");
      Transform tareW1y = Tare2UserHolder.Find("TareWandYes");
      Transform tareG2n = Tare2UserHolder.Find("TareGlasses2No");
      Transform tareG2y = Tare2UserHolder.Find("TareGlasses2Yes");
      Transform tareW2n = Tare2UserHolder.Find("TareWand2No");
      Transform tareW2y = Tare2UserHolder.Find("TareWand2Yes");

      tareG1n.gameObject.SetActive(!m_devicesTared[0]);
      tareG1y.gameObject.SetActive(m_devicesTared[0]);
      tareW1n.gameObject.SetActive(!m_devicesTared[1]);
      tareW1y.gameObject.SetActive(m_devicesTared[1]);

      tareG2n.gameObject.SetActive(!m_devicesTared[2]);
      tareG2y.gameObject.SetActive(m_devicesTared[2]);
      tareW2n.gameObject.SetActive(!m_devicesTared[3]);
      tareW2y.gameObject.SetActive(m_devicesTared[3]);
    }
  }

  void SeenUpdate()
  {
    // Determine if devices can be seen
    int userCount = HoloDevice.active.GetUserCount();
    List<bool> devicesValid = new List<bool>();
    for (int userIndex = 0; userIndex < userCount; ++userIndex)
    {
      devicesValid.Add(HoloDevice.active.GetUserGlasses(userIndex).IsPositionValid());
      devicesValid.Add(HoloDevice.active.GetUserWand(userIndex).IsPositionValid());
    }

    // Determine if we should continue, calibration complete
    bool singleUser = userCount == 1;
    bool allSeen = false;
    if (singleUser)
      allSeen = devicesValid[0] & devicesValid[1];
    else
      allSeen = (devicesValid[0] & devicesValid[1]) || (devicesValid[2] & devicesValid[3]);

    Transform pressAnyButtonLabel = SeenHolder.Find("ContinueLabel");
    pressAnyButtonLabel.gameObject.SetActive(false);
    Transform pressAnyButtonLabel2User = Seen2UserHolder.Find("ContinueLabel");
    pressAnyButtonLabel2User.gameObject.SetActive(false);

    if (allSeen)
    {
      if (AnyUserAnyButtonPressed())
        m_doneCalibrating = true;

      if (singleUser)
        pressAnyButtonLabel.gameObject.SetActive(true);
      else
        pressAnyButtonLabel2User.gameObject.SetActive(true);
    }

    // Update graphics
    if (singleUser)
    {
      Transform seenG1n = SeenHolder.Find("SeenGlassesNo");
      Transform seenG1y = SeenHolder.Find("SeenGlassesYes");
      Transform seenW1n = SeenHolder.Find("SeenWandNo");
      Transform seenW1y = SeenHolder.Find("SeenWandYes");

      seenG1n.gameObject.SetActive(!devicesValid[0]);
      seenG1y.gameObject.SetActive(devicesValid[0]);
      seenW1n.gameObject.SetActive(!devicesValid[1]);
      seenW1y.gameObject.SetActive(devicesValid[1]);
    }
    else
    {
      Transform seenG1n = Seen2UserHolder.Find("SeenGlassesNo");
      Transform seenG1y = Seen2UserHolder.Find("SeenGlassesYes");
      Transform seenW1n = Seen2UserHolder.Find("SeenWandNo");
      Transform seenW1y = Seen2UserHolder.Find("SeenWandYes");
      Transform seenG2n = Seen2UserHolder.Find("SeenGlasses2No");
      Transform seenG2y = Seen2UserHolder.Find("SeenGlasses2Yes");
      Transform seenW2n = Seen2UserHolder.Find("SeenWand2No");
      Transform seenW2y = Seen2UserHolder.Find("SeenWand2Yes");

      seenG1n.gameObject.SetActive(!devicesValid[0]);
      seenG1y.gameObject.SetActive(devicesValid[0]);
      seenW1n.gameObject.SetActive(!devicesValid[1]);
      seenW1y.gameObject.SetActive(devicesValid[1]);

      seenG2n.gameObject.SetActive(!devicesValid[2]);
      seenG2y.gameObject.SetActive(devicesValid[2]);
      seenW2n.gameObject.SetActive(!devicesValid[3]);
      seenW2y.gameObject.SetActive(devicesValid[3]);
    }
  }
}

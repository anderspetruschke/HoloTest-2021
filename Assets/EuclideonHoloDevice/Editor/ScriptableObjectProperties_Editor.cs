using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ScriptableObject), true)]
public class ScriptableObjectDrawer : PropertyDrawer
{
  private Editor myEditor = null;

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    // Draw label
    EditorGUI.PropertyField(position, property, label, true);

    // Draw foldout arrow
    if (property.objectReferenceValue != null)
    {
      property.isExpanded = true;
    }
    else
    {
      Object[] configs = Resources.FindObjectsOfTypeAll(typeof(HoloConfig));
      if (configs.Length > 0)
        property.objectReferenceValue = configs[0];
      if (property.objectReferenceValue != null)
      {
        property.isExpanded = true;
      }
      else
      {
        Debug.LogWarning("The EuclideonHoloCave GameObject needs a HoloDeviceConfig defined in order to work. Please drag and drop the HoloDeviceConfig provided in the Unity Toolkit Assets/Prefab/ folder.");
        property.isExpanded = false;
      }
    }
    
    try
    {
      // Draw foldout properties
      if (property.isExpanded)
      {
        // Make child fields be indented
        EditorGUI.indentLevel++;

        // Make a new editor on the layout step
        Editor.CreateCachedEditor(property.objectReferenceValue, null, ref myEditor);

        // Draw object properties
        myEditor.OnInspectorGUI();

        // Set indent back to what it was
        EditorGUI.indentLevel--;
      }
    }
    catch(System.Exception e)
    {
      Debug.Log(e);
      property.isExpanded = false;
    }
  }
}

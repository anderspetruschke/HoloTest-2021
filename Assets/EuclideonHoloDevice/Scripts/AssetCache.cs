using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is used to list dynamically loaded assets that should be
// included in the built Project.
//
// It can be added to a GameObject and assets can be assigned through the editor.

public class AssetCache : MonoBehaviour
{
  public List<Object> Assets;
}

using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BuildScript
{
  static private string[] m_extraAssetDirs = {
    "EuclideonHoloDevice/Shaders"
  };

  [InitializeOnLoadMethod]
  private static void Initialize()
  {
    BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayerHandler);
  }

  private static void BuildPlayerHandler(BuildPlayerOptions option)
  {
    // Get build path.
    string exeName = Path.GetFileName(option.locationPathName);
    string exeNameNoExt = Path.GetFileNameWithoutExtension(option.locationPathName);
    string path = Path.GetDirectoryName(option.locationPathName);
    if (path == "")
      return;
    path += "/";
    option.locationPathName = path + "bin/" + exeName;
    BuildPipeline.BuildPlayer(option);

    // Copy the extra asset directorys
    foreach (string dir in m_extraAssetDirs)
    {
      string targetPath = path + "bin/" + exeNameNoExt + "_Data/" + dir;
      if (Directory.Exists(targetPath))
        Directory.Delete(targetPath, true);

      // Make sure the directories in the path exist
      Directory.CreateDirectory(targetPath);
      Directory.Delete(targetPath); // Bit hacky but this makes sure the final directory doesn't exist so that the CopyFileOrDirectory call succeeds
      FileUtil.CopyFileOrDirectory("Assets/" + dir, targetPath);
    }

    string runFile = "@echo off\n";
    runFile += "cd %~dp0bin\n";
    runFile += "\"" + exeName + "\" -batchmode\n";
    File.WriteAllText(path + Application.productName + ".bat", runFile);
  }
}

using UnityEngine;
using UnityEditor;
using System.IO;

public class ConfigureProject_Editor : EditorWindow
{
  class ConfigOption
  {
    public static bool ApplyAll { get; set; }

    public delegate bool IsConfiguredHandler();
    public delegate void ConfigureHandler();

    public IsConfiguredHandler IsConfigured;
    private ConfigureHandler configure;
    private string title;

    public ConfigOption(string title, IsConfiguredHandler isConfigured, ConfigureHandler configure)
    {
      this.IsConfigured = isConfigured;
      this.configure = configure;
      this.title = title;
    }

    public void Draw()
    {
      bool isApplied = IsConfigured();
      EditorGUI.BeginDisabledGroup(isApplied);
      bool tryApply = GUILayout.Button(title);
      if ((tryApply || ApplyAll) && !isApplied)
        configure();
      EditorGUI.EndDisabledGroup();
    }
  }

  static private BuildTargetGroup requiredGroup = BuildTargetGroup.Standalone;
  static private BuildTarget requiredTarget = BuildTarget.StandaloneWindows64;
  static private bool requiredAutoAPI = false;
  static private UnityEngine.Rendering.GraphicsDeviceType[] requiredAPIs = new UnityEngine.Rendering.GraphicsDeviceType[1] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore };
  static private bool restartRequired = false;

  private ConfigOption[] options;

  ConfigureProject_Editor()
  {
    options = new ConfigOption[]
    {
      new ConfigOption("Set build target to Windows x64",
        () => EditorUserBuildSettings.activeBuildTarget == requiredTarget,
        () => EditorUserBuildSettings.SwitchActiveBuildTarget(requiredGroup, requiredTarget)),

      new ConfigOption("Set graphics API to OpenGL Core",
        () => {
          var apiList = PlayerSettings.GetGraphicsAPIs(requiredTarget);
          return apiList.Length > 0 && apiList[0] == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore &&
          PlayerSettings.GetUseDefaultGraphicsAPIs(requiredTarget) == requiredAutoAPI;
        },
        () => {
          PlayerSettings.SetUseDefaultGraphicsAPIs(requiredTarget, requiredAutoAPI);
          PlayerSettings.SetGraphicsAPIs(requiredTarget, requiredAPIs);
          restartRequired = true;
        })
    };
  }

  static void HorizontalLine(Color color, float height = 2.0f)
  {
    GUIStyle horizontalLine;
    horizontalLine = new GUIStyle();
    horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
    horizontalLine.margin = new RectOffset(0, 0, 4, 4);
    horizontalLine.fixedHeight = height;

    var c = GUI.color;
    GUI.color = color;
    GUILayout.Box(GUIContent.none, horizontalLine);
    GUI.color = c;
  }

  [MenuItem("Euclideon Holo Device/Configure Project")]
  static void ConfigureProject() { EditorWindow.GetWindow<ConfigureProject_Editor>(true, "Configure Project for Hologram Device"); }

  void OnGUI()
  {
    bool lastWordWrap = EditorStyles.label.wordWrap;
    EditorStyles.label.wordWrap = true;

    bool everythingIsConfigured = true;
    foreach (ConfigOption option in options)
      everythingIsConfigured &= option.IsConfigured();

    BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

    EditorGUILayout.LabelField("Use the button below to configure your project so that it is compatible with the Hologram Devices. This will apply all the options below.");

    EditorGUI.BeginDisabledGroup(everythingIsConfigured);
    if (everythingIsConfigured)
      EditorGUILayout.LabelField(" *The Project is already configured correctly.");
    ConfigOption.ApplyAll = GUILayout.Button("Configure All");
    EditorGUI.EndDisabledGroup();

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Project Settings to Configure.");
    HorizontalLine(Color.grey);
    EditorGUILayout.LabelField(" *Note: Disabled options are already configured correctly.");
    foreach (ConfigOption option in options)
      option.Draw();
    ConfigOption.ApplyAll = false;

    if (restartRequired)
    {
      if (EditorUtility.DisplayDialog("Restart Required", "The Graphics API has been changed. For this to take affect, the Editor needs to be restarted.", "Restart Now", "No"))
        EditorApplication.OpenProject(Directory.GetCurrentDirectory());
      restartRequired = false;
    }

    EditorStyles.label.wordWrap = lastWordWrap;
  }
}

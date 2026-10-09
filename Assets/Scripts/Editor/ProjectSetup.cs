using System.IO;
using Saiyan.Boss;
using Saiyan.Level;
using Saiyan.Player;
using Saiyan.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Saiyan.Editor
{
    /// <summary>
    /// One-click project setup (also runs headless): URP asset, input actions asset, boss tuning asset, the two scenes, build settings, player settings.
    /// Run from the menu "Saiyan/Setup Project" or: Unity -batchmode -executeMethod Saiyan.Editor.ProjectSetup.Run
    /// </summary>
    public static class ProjectSetup
    {
        public const string MenuScene = "Assets/Scenes/MainMenu.unity", LevelScene = "Assets/Scenes/Level01_FrostingFields.unity";
        const string TuningPath = "Assets/Settings/KingCakezillaTuning.asset", InputPath = "Assets/Resources/Input/SaiyanControls.inputactions", UrpPath = "Assets/Settings/SaiyanURP.asset", RendererPath = "Assets/Settings/SaiyanURP_Renderer.asset";

        [MenuItem("Saiyan/Setup Project")]
        public static void Run()
        {
            Directory.CreateDirectory("Assets/Settings"); Directory.CreateDirectory("Assets/Resources/Input"); Directory.CreateDirectory("Assets/Scenes");
            SetupUrp();
            SetupInput();
            var tuning = SetupTuning();
            SetupScenes(tuning);
            SetupPlayerSettings();
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("[Saiyan] project setup complete");
        }

        static void SetupUrp()
        {
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpPath);
            if (!asset)
            {
                var rd = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rd, RendererPath);
                asset = UniversalRenderPipelineAsset.Create(rd);
                AssetDatabase.CreateAsset(asset, UrpPath);
            }
            GraphicsSettings.defaultRenderPipeline = asset;
            for (int i = 0; i < QualitySettings.names.Length; i++) { QualitySettings.SetQualityLevel(i, false); QualitySettings.renderPipeline = asset; }
            Debug.Log("[Saiyan] URP asset assigned");
        }

        static void SetupInput()
        {
            var a = InputActionsIntent.BuildDefaultAsset();
            File.WriteAllText(InputPath, a.ToJson());
            Object.DestroyImmediate(a);
            AssetDatabase.ImportAsset(InputPath);
            Debug.Log("[Saiyan] input actions written: " + InputPath);
        }

        static BossTuning SetupTuning()
        {
            var t = AssetDatabase.LoadAssetAtPath<BossTuning>(TuningPath);
            if (!t) { t = ScriptableObject.CreateInstance<BossTuning>(); AssetDatabase.CreateAsset(t, TuningPath); }
            return t;
        }

        static void SetupScenes(BossTuning tuning)
        {
            var menu = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AddCamera(); new GameObject("MainMenu").AddComponent<MainMenu>();
            EditorSceneManager.SaveScene(menu, MenuScene);

            var level = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AddCamera(); var flow = new GameObject("LevelFlow").AddComponent<LevelFlow>(); flow.Tuning = tuning;
            EditorSceneManager.SaveScene(level, LevelScene);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MenuScene, true), new EditorBuildSettingsScene(LevelScene, true) };
            Debug.Log("[Saiyan] scenes created and added to build settings");
        }

        static void AddCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var c = go.AddComponent<Camera>(); c.orthographic = true; c.orthographicSize = LevelLayout.CameraSize; c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = new Color(0.78f, 0.9f, 1f);
            go.AddComponent<AudioListener>(); go.transform.position = new Vector3(0, 0, -10);
        }

        static void SetupPlayerSettings()
        {
            PlayerSettings.companyName = "Gamebreak Labs"; PlayerSettings.productName = "Saiyan vs. Everybody!";
            PlayerSettings.defaultScreenWidth = 1920; PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            // the game reads the NEW Input System only: make sure the player uses it (a project created from scratch defaults to the old Input Manager, which silently disables all input)
            var ps = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var ih = ps.FindProperty("activeInputHandler");
            if (ih != null && ih.intValue != 1) { ih.intValue = 1; ps.ApplyModifiedPropertiesWithoutUndo(); Debug.Log("[Saiyan] active input handler set to Input System Package (restart the editor once)"); }
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.gamebreaklabs.saiyanvseverybody");
        }

        [MenuItem("Saiyan/Build Windows Player")]
        public static void BuildWindows()
        {
            Run();
            Directory.CreateDirectory("Builds/Windows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { MenuScene, LevelScene }, locationPathName = "Builds/Windows/SaiyanVsEverybody.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            Debug.Log($"[Saiyan] build {report.summary.result}: errors {report.summary.totalErrors}, size {report.summary.totalSize / (1024 * 1024)} MB");
            if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Saiyan.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Saiyan.Tests
{
    /// <summary>Leaves the test runner with exactly one empty scene: unloads this test's scene, any level or menu scene a test loaded (Retry does), and old keep scenes, and WAITS.</summary>
    public static class TestScenes
    {
        public static IEnumerator Cleanup(Scene current)
        {
            Time.timeScale = 1f; Time.captureFramerate = 0;
            var keep = SceneManager.CreateScene("keep_" + Time.frameCount);
            SceneManager.SetActiveScene(keep);
            var list = new List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s == keep) continue;
                if (s == current || s.name == LevelFlow.SceneName || s.name == LevelFlow.MenuSceneName || s.name == Saiyan.Art.AnimationPreview.SceneName || s.name.StartsWith("keep_")) list.Add(s);
            }
            foreach (var s in list) if (s.isLoaded) { var op = SceneManager.UnloadSceneAsync(s); while (op != null && !op.isDone) yield return null; }
            yield return null;
        }
    }
}

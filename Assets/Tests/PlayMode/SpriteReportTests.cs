using System.Collections;
using System.IO;
using NUnit.Framework;
using Saiyan.Art;
using Saiyan.Boss;
using Saiyan.Core;
using Saiyan.Level;
using Saiyan.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Saiyan.Tests
{
    /// <summary>Visual report for the sprite-art milestone: real renders of the running game with the generated Saiyan sprites (and the placeholder for comparison).</summary>
    public class SpriteReportTests
    {
        public const string Tag = "MA_2026-10-09";
        static string Dir => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "AI_HANDOFF", "SCREENSHOTS"));
        static string FramesDir => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "frames"));
        LevelFlow m_Flow; ScriptedIntent m_In; Scene m_Scene;

        [SetUp] public void SetUp() { Time.timeScale = 1f; Time.captureFramerate = 0; GameSession.Reset(); Directory.CreateDirectory(Dir); m_Scene = SceneManager.CreateScene("SR_" + TestContext.CurrentContext.Test.Name); SceneManager.SetActiveScene(m_Scene); }
        [UnityTearDown] public IEnumerator TearDown() { PlayerPrefs.DeleteKey(SaiyanSpriteVisual.PrefKey); yield return TestScenes.Cleanup(m_Scene); }

        LevelFlow Spawn(bool sprites)
        {
            PlayerPrefs.SetInt(SaiyanSpriteVisual.PrefKey, sprites ? 1 : 0);
            var go = new GameObject("flow"); go.SetActive(false);
            m_Flow = go.AddComponent<LevelFlow>(); m_In = go.AddComponent<ScriptedIntent>(); m_Flow.InputOverride = m_In; go.SetActive(true); return m_Flow;
        }

        static IEnumerator Seconds(float s) { float t = 0f; while (t < s) { t += Time.deltaTime; yield return null; } }

        static Texture2D Render(Camera cam, int w, int h)
        {
            var rt = new RenderTexture(w, h, 24); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
            RenderTexture.active = null; cam.targetTexture = null; rt.Release(); Object.Destroy(rt); return tex;
        }

        static void UiToCamera(Camera cam)
        {
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                if (c.renderMode == RenderMode.ScreenSpaceOverlay) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
        }

        IEnumerator Still(string name, Vector2? centre = null, float size = 2f)
        {
            yield return null;
            var main = Camera.main; Camera cam = main; GameObject tmp = null;
            if (centre.HasValue) { tmp = new GameObject("shotcam"); cam = tmp.AddComponent<Camera>(); cam.CopyFrom(main); cam.transform.position = new Vector3(centre.Value.x, centre.Value.y, -10f); cam.orthographicSize = size; }
            else UiToCamera(main);
            Canvas.ForceUpdateCanvases();
            var tex = Render(cam, 1920, 1080); File.WriteAllBytes(Path.Combine(Dir, Tag + "_" + name + ".png"), tex.EncodeToPNG()); Object.Destroy(tex); if (tmp) Object.Destroy(tmp);
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator Report_BeforeAfter_And_InGame()
        {
            // before: procedural placeholder; after: generated sprite art. Same camera, same moment.
            Spawn(false); yield return Seconds(1.0f);
            var p = m_Flow.Player; var focus = (Vector2)p.Transform.position + new Vector2(0f, 1.1f);
            yield return Still("A1_before-placeholder-closeup", focus, 1.7f);
            p.SpriteVisual.SetActive(true, false); yield return Seconds(0.5f);
            yield return Still("A2_after-sprite-closeup", focus, 1.7f);
            yield return Still("A3_sprite-in-tutorial");
            // the sprite against hazards and the boss
            m_Flow.Player.Controller.Teleport(new Vector2(LevelLayout.ArenaLeft + 4f, 0.3f)); m_Flow.Player.Health.GrantInvulnerability(9999f);
            yield return null; m_Flow.BeginBossIntro(true);
            float g = 0; while (m_Flow.State != LevelState.Fight && g < 5f) { g += Time.deltaTime; yield return null; }
            yield return Seconds(0.6f);
            yield return Still("A4_sprite-in-boss-fight");
            g = 0; while (m_Flow.Boss.State != BossState.Telegraph && g < 8f) { g += Time.deltaTime; yield return null; }
            yield return Seconds(0.6f);
            yield return Still("A5_sprite-with-boss-attack-warning");
            m_In.Current.shootHeld = true; yield return Seconds(0.5f); m_In.Current.shootHeld = false;
            yield return Still("A6_sprite-shooting-no-shoot-art-yet");
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator Report_PreviewScene()
        {
            SceneManager.LoadScene(AnimationPreview.SceneName); yield return null; yield return Seconds(0.6f);
            var cam = Camera.main; var tex = Render(cam, 1920, 1080);
            File.WriteAllBytes(Path.Combine(Dir, Tag + "_A7_animation-preview-scene-world-view-ui-panel-not-captured.png"), tex.EncodeToPNG()); Object.Destroy(tex);
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator Report_Video_SpriteInGame()
        {
            Spawn(true); yield return Seconds(0.8f);
            string dir = Path.Combine(FramesDir, "sprite-idle-in-game");
            if (Directory.Exists(dir)) Directory.Delete(dir, true); Directory.CreateDirectory(dir);
            UiToCamera(Camera.main); Time.captureFramerate = 30;
            for (int f = 0; f < 240; f++)
            {
                float t = f / 30f;
                m_In.Current.move = (t > 2.5f && t < 5f) ? 1f : (t > 5.5f && t < 7f ? -1f : 0f);
                if (Mathf.Abs(t - 3.5f) < 0.017f || Mathf.Abs(t - 6.2f) < 0.017f) m_In.Current.jumpPressed = true;
                m_In.Current.jumpHeld = (t > 3.5f && t < 4.0f) || (t > 6.2f && t < 6.6f);
                yield return null;
                var tex = Render(Camera.main, 1280, 720); File.WriteAllBytes(Path.Combine(dir, "f" + f.ToString("D4") + ".png"), tex.EncodeToPNG()); Object.Destroy(tex);
            }
            Time.captureFramerate = 0;
        }
    }
}

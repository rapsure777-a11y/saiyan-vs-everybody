using System.Collections;
using System.IO;
using NUnit.Framework;
using Saiyan.Boss;
using Saiyan.Core;
using Saiyan.Level;
using Saiyan.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Saiyan.Tests
{
    /// <summary>
    /// Visual progress report: runs the REAL game (scripted input, no mock-ups) and writes actual rendered frames to AI_HANDOFF/SCREENSHOTS.
    /// Stills go straight there; video clips are written as frame sequences to Logs/frames and encoded by Tools/encode-clips.ps1.
    /// Change <see cref="Tag"/> for each milestone so older reports are kept.
    /// </summary>
    public class VisualReportTests
    {
        public const string Tag = "M1_2026-10-09";
        static string Dir => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "AI_HANDOFF", "SCREENSHOTS"));
        static string FramesDir => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "frames"));

        LevelFlow m_Flow; ScriptedIntent m_In; Scene m_Scene;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f; Time.captureFramerate = 0; GameSession.Reset(); GameSettings.AssistMode = false;
            Directory.CreateDirectory(Dir);
            m_Scene = SceneManager.CreateScene("V_" + TestContext.CurrentContext.Test.Name); SceneManager.SetActiveScene(m_Scene);
        }

        [UnityTearDown]
        public IEnumerator TearDown() { yield return TestScenes.Cleanup(m_Scene); }

        LevelFlow Spawn()
        {
            var go = new GameObject("flow"); go.SetActive(false);
            m_Flow = go.AddComponent<LevelFlow>(); m_In = go.AddComponent<ScriptedIntent>(); m_Flow.InputOverride = m_In;
            go.SetActive(true); return m_Flow;
        }

        static IEnumerator Seconds(float s) { float t = 0f; while (t < s) { t += Time.deltaTime; yield return null; } }

        IEnumerator EnterFight(float x = LevelLayout.ArenaLeft + 5f, bool invulnerable = true)
        {
            m_Flow.Player.Controller.Teleport(new Vector2(x, 0.3f));
            if (invulnerable) m_Flow.Player.Health.GrantInvulnerability(9999f);
            yield return null; m_Flow.BeginBossIntro(true);
            float g = 0; while (m_Flow.State != LevelState.Fight && g < 5f) { g += Time.deltaTime; yield return null; }
        }

        IEnumerator WaitAttack(string name, BossState state, float extra, float timeout = 40f)
        {
            float g = 0;
            while (!(m_Flow.Boss.CurrentAttackName == name && m_Flow.Boss.State == state) && g < timeout) { g += Time.deltaTime; yield return null; }
            Assert.Less(g, timeout, "boss never reached " + name + " / " + state);
            yield return Seconds(extra);
        }

        // ---- capture ----

        static Texture2D Render(Camera cam, int w, int h)
        {
            var rt = new RenderTexture(w, h, 24); cam.targetTexture = rt; cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
            RenderTexture.active = null; cam.targetTexture = null; rt.Release(); Object.Destroy(rt);
            return tex;
        }

        static void UiToCamera(Camera cam)
        {
            foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                if (c.renderMode == RenderMode.ScreenSpaceOverlay) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
        }

        /// <summary>A still of the whole screen (with the HUD), or a close-up of a world point (no HUD) when <paramref name="centre"/> is given.</summary>
        IEnumerator Still(string name, Vector2? centre = null, float size = 2f)
        {
            Debug.Log("[VR] still " + name);
            yield return null;
            var main = Camera.main; Camera cam = main; GameObject tmp = null;
            if (centre.HasValue)
            {
                tmp = new GameObject("shotcam"); cam = tmp.AddComponent<Camera>(); cam.CopyFrom(main);
                cam.transform.position = new Vector3(centre.Value.x, centre.Value.y, -10f); cam.orthographicSize = size;
            }
            else UiToCamera(main);
            Canvas.ForceUpdateCanvases();
            var tex = Render(cam, 1920, 1080);
            File.WriteAllBytes(Path.Combine(Dir, Tag + "_" + name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex); if (tmp) Object.Destroy(tmp);
        }

        /// <summary>Records frames at a fixed 30 fps into Logs/frames/name (deterministic: the game runs on a 1/30 s clock while recording).</summary>
        IEnumerator Clip(string name, float seconds, System.Action<float> script = null)
        {
            string dir = Path.Combine(FramesDir, name);
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);
            UiToCamera(Camera.main);
            Time.captureFramerate = 30;
            int frames = Mathf.RoundToInt(seconds * 30f);
            for (int f = 0; f < frames; f++)
            {
                script?.Invoke(f / 30f);
                yield return null;
                var tex = Render(Camera.main, 1280, 720);
                File.WriteAllBytes(Path.Combine(dir, "f" + f.ToString("D4") + ".png"), tex.EncodeToPNG()); Object.Destroy(tex);
            }
            Time.captureFramerate = 0;
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator Report_Tutorial_Saiyan_Environment()
        {
            Spawn(); yield return Seconds(1.0f);
            var p = m_Flow.Player.Transform;
            yield return Still("01_saiyan-closeup-idle", (Vector2)p.position + new Vector2(0f, 1.0f), 1.6f);
            yield return Still("02_tutorial-start-run-and-jump-signs");
            m_Flow.Player.Controller.Teleport(new Vector2(17f, 0.3f)); yield return Seconds(0.6f);
            yield return Still("03_tutorial-blocks-and-dash-sign");
            m_Flow.Player.Controller.Teleport(new Vector2(31f, 0.3f)); yield return Seconds(0.6f);
            m_In.Current.move = 1f; m_In.Current.dashPressed = true; yield return Seconds(0.1f);
            yield return Still("04_saiyan-dashing-closeup", (Vector2)p.position + new Vector2(0f, 1.0f), 1.8f);
            m_In.Current.move = 0f; yield return Seconds(0.6f);
            m_Flow.Player.Controller.Teleport(new Vector2(47f, 0.3f)); yield return Seconds(0.5f);
            m_In.Current.shootHeld = true; yield return Seconds(0.45f);
            yield return Still("05_tutorial-shooting-targets");
            m_In.Current.shootHeld = false; yield return Seconds(0.3f);
            m_Flow.Player.Controller.Teleport(new Vector2(LevelLayout.CheckpointX - 1f, 0.3f)); yield return Seconds(0.7f);
            yield return Still("06_checkpoint-boss-ahead");
            // jump close-up
            m_In.Current.jumpPressed = true; m_In.Current.jumpHeld = true; yield return Seconds(0.35f);
            yield return Still("07_saiyan-jumping-closeup", (Vector2)p.position + new Vector2(0f, 0.6f), 2.4f);
            m_In.Current.jumpHeld = false;
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator Report_Boss_Attacks_Phases_Victory()
        {
            Spawn(); yield return Seconds(0.5f);
            m_Flow.Player.Controller.Teleport(new Vector2(LevelLayout.ArenaLeft + 1f, 0.3f)); m_Flow.Player.Health.GrantInvulnerability(9999f);
            yield return null; m_Flow.BeginBossIntro(false);
            yield return Seconds(1.4f);
            yield return Still("10_boss-intro-banner");
            m_Flow.Boss.SkipIntro();
            float g = 0; while (m_Flow.State != LevelState.Fight && g < 5f) { g += Time.deltaTime; yield return null; }
            m_Flow.Player.Controller.Teleport(new Vector2(LevelLayout.ArenaLeft + 5f, 0.3f));
            yield return Seconds(0.8f);
            yield return Still("11_king-cakezilla-closeup", new Vector2(LevelLayout.BossRootX, 5.2f), 4.6f);
            yield return Still("12_fight-overview-idle-hud");
            m_Flow.Player.Meter.Add(100f); yield return Seconds(0.3f);
            yield return Still("13_hud-super-ready");
            // each attack: warning, then the attack itself
            yield return WaitAttack("Cupcake Toss", BossState.Telegraph, 0.5f); yield return Still("20_attack-cupcake-toss-warning");
            yield return WaitAttack("Cupcake Toss", BossState.Attack, 0.7f); yield return Still("21_attack-cupcake-toss-in-flight");
            yield return WaitAttack("Hand Slam", BossState.Telegraph, 0.7f); yield return Still("22_attack-hand-slam-warning");
            yield return WaitAttack("Hand Slam", BossState.Attack, 0.45f); yield return Still("23_attack-hand-slam-shockwave");
            yield return WaitAttack("Frost Blob", BossState.Telegraph, 0.6f); yield return Still("24_attack-frost-blob-warning");
            yield return WaitAttack("Frost Blob", BossState.Attack, 1.7f); yield return Still("25_attack-frost-blob-puddle");
            // player firing and the Super
            m_In.Current.shootHeld = true; yield return Seconds(0.8f); yield return Still("30_player-shooting-at-boss"); m_In.Current.shootHeld = false;
            m_Flow.Player.Meter.Add(100f); m_In.Current.superPressed = true; yield return Seconds(0.45f); yield return Still("31_super-burst");
            // phases (shortened transition so the capture is quick)
            var h = m_Flow.Boss.Health;
            h.TakeDamage(h.Current - Mathf.RoundToInt(h.Max * 0.70f) + 1); yield return Seconds(0.7f); yield return Still("40_phase-2-transition");
            yield return Seconds(m_Flow.Tuning.transitionSeconds); yield return Seconds(1.2f); yield return Still("41_phase-2-fight");
            h.TakeDamage(h.Current - Mathf.RoundToInt(h.Max * 0.35f) + 1); yield return Seconds(0.8f); yield return Still("42_phase-3-supreme-transition");
            yield return Seconds(m_Flow.Tuning.transitionSeconds); yield return Seconds(1.5f); yield return Still("43_phase-3-cakezilla-supreme");
            // victory
            h.Invulnerable = false; h.TakeDamage(9999);
            yield return Seconds(1.2f); yield return Still("50_boss-defeat-sequence");
            yield return Seconds(4.5f); yield return Still("51_ui-sweet-victory");
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator Report_Pause_GameOver_Menu()
        {
            Debug.Log("[VR] spawn"); Spawn(); yield return Seconds(0.8f); Debug.Log("[VR] spawned");
            m_Flow.SetPaused(true); yield return null; yield return null; yield return Still("60_ui-pause-menu"); m_Flow.SetPaused(false);   // (time is frozen while paused, so the waits above are in frames)
            yield return EnterFight(LevelLayout.ArenaLeft + 5f, false); Debug.Log("[VR] in fight");
            m_Flow.Player.Health.PostHitInvulnerability = 0f;
            for (int i = 0; i < 3; i++) { m_Flow.Player.Health.TakeHit(1, Vector2.zero); yield return null; }
            Debug.Log("[VR] died ts=" + Time.timeScale + " dt=" + Time.deltaTime + " state=" + m_Flow.State); yield return Seconds(1.8f); Debug.Log("[VR] waited"); yield return Still("61_ui-game-over-retry"); Debug.Log("[VR] game over shot");
            SceneManager.LoadScene(LevelFlow.MenuSceneName); yield return null; yield return Seconds(1.0f);
            yield return Still("62_ui-main-menu"); Debug.Log("[VR] menu shot");
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator Report_Video_Movement_And_Combat()
        {
            Spawn(); yield return Seconds(0.8f);
            m_Flow.Player.Controller.Teleport(new Vector2(25f, 0.3f)); yield return Seconds(0.4f);
            yield return Clip("movement-and-combat", 8f, t =>
            {
                m_In.Current.move = t < 6.5f ? 1f : 0f;
                if (Mathf.Abs(t - 0.6f) < 0.017f || Mathf.Abs(t - 3.0f) < 0.017f) { m_In.Current.jumpPressed = true; }
                m_In.Current.jumpHeld = (t > 0.6f && t < 1.3f) || (t > 3.0f && t < 3.5f);
                if (Mathf.Abs(t - 1.8f) < 0.017f) m_In.Current.dashPressed = true;
                m_In.Current.shootHeld = t > 3.8f && t < 7.5f;
            });
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator Report_Video_Boss_Fight_And_Phases()
        {
            Spawn(); yield return Seconds(0.5f);
            yield return EnterFight(LevelLayout.ArenaLeft + 5f);
            m_Flow.Player.Meter.Add(100f);
            bool p2 = false, p3 = false, supered = false;
            yield return Clip("boss-fight-and-phases", 26f, t =>
            {
                m_In.Current.shootHeld = true;
                m_In.Current.move = Mathf.Sin(t * 0.9f) * 0.8f;
                if (!supered && t > 5f) { supered = true; m_In.Current.superPressed = true; }
                var h = m_Flow.Boss.Health;
                if (!p2 && t > 9f) { p2 = true; h.TakeDamage(h.Current - Mathf.RoundToInt(h.Max * 0.70f) + 1); }
                if (!p3 && t > 17f) { p3 = true; h.TakeDamage(h.Current - Mathf.RoundToInt(h.Max * 0.35f) + 1); }
            });
        }
    }
}

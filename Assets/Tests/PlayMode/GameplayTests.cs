using System.Collections;
using System.Collections.Generic;
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
    public class GameplayTests
    {
        LevelFlow m_Flow; ScriptedIntent m_In; Scene m_Scene;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f; GameSession.Reset(); AudioHooks.Log.Clear(); PlayerPrefs.DeleteKey("saiyan.assist"); PlayerPrefs.DeleteKey("saiyan.reducedshake");
            m_Scene = SceneManager.CreateScene("T_" + TestContext.CurrentContext.Test.Name);
            SceneManager.SetActiveScene(m_Scene);
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            foreach (var h in Object.FindObjectsByType<Hazard>(FindObjectsSortMode.None)) if (h) Object.Destroy(h.gameObject);
            if (m_Scene.isLoaded) SceneManager.UnloadSceneAsync(m_Scene);
        }

        LevelFlow Spawn(BossTuning tuning = null)
        {
            var go = new GameObject("flow"); go.SetActive(false);
            m_Flow = go.AddComponent<LevelFlow>(); m_In = go.AddComponent<ScriptedIntent>(); m_Flow.InputOverride = m_In;
            if (tuning) m_Flow.Tuning = tuning;
            go.SetActive(true);
            return m_Flow;
        }

        static IEnumerator Seconds(float s) { float t = 0f; while (t < s) { t += Time.deltaTime; yield return null; } }

        IEnumerator EnterFight(float playerX = LevelLayout.ArenaLeft + 6f, bool invulnerable = true)
        {
            m_Flow.Player.Controller.Teleport(new Vector2(playerX, 0.3f));
            if (invulnerable) m_Flow.Player.Health.GrantInvulnerability(9999f);
            yield return null;
            m_Flow.BeginBossIntro(true);
            float guard = 0f; while (m_Flow.State != LevelState.Fight && guard < 5f) { guard += Time.deltaTime; yield return null; }
            Assert.AreEqual(LevelState.Fight, m_Flow.State, "fight did not start");
        }

        [UnityTest]
        public IEnumerator Level_Builds_WithPlayerBossAndHud()
        {
            Spawn(); yield return null; yield return null;
            Assert.IsNotNull(m_Flow.Player); Assert.IsNotNull(m_Flow.Boss); Assert.IsNotNull(m_Flow.HudUi);
            Assert.AreEqual(3, m_Flow.Player.Health.Current);
            Assert.AreEqual(120, m_Flow.Boss.Health.Max);
            Assert.AreEqual(LevelState.Tutorial, m_Flow.State);
        }

        [UnityTest]
        public IEnumerator Player_LandsOnTheGround()
        {
            Spawn(); yield return Seconds(1f);
            Assert.IsTrue(m_Flow.Player.Controller.Grounded, "not grounded");
            Assert.AreEqual(0f, m_Flow.Player.Transform.position.y, 0.08f);
        }

        float MaxHeight(float seconds) { return 0f; }

        [UnityTest]
        public IEnumerator Jump_FullHeight_VersusShortHop()
        {
            Spawn(); yield return Seconds(1f);
            float floor = m_Flow.Player.Transform.position.y, max = floor;
            m_In.Current.jumpPressed = true; m_In.Current.jumpHeld = true;
            for (float t = 0; t < 1.4f; t += Time.deltaTime) { max = Mathf.Max(max, m_Flow.Player.Transform.position.y); if (t > 0.9f) m_In.Current.jumpHeld = false; yield return null; }
            float full = max - floor;
            Assert.That(full, Is.InRange(2.2f, 2.9f), "full jump height");
            yield return Seconds(1f);
            max = floor = m_Flow.Player.Transform.position.y;
            m_In.Current.jumpPressed = true; m_In.Current.jumpHeld = true; yield return null; yield return null; yield return null; m_In.Current.jumpHeld = false;
            for (float t = 0; t < 1.2f; t += Time.deltaTime) { max = Mathf.Max(max, m_Flow.Player.Transform.position.y); yield return null; }
            float hop = max - floor;
            Assert.That(hop, Is.InRange(0.2f, full * 0.75f), "short hop must be clearly lower than the full jump");
        }

        [UnityTest]
        public IEnumerator Dash_IsFast_OnTheGround_AndHorizontalInTheAir()
        {
            Spawn(); yield return Seconds(1f);
            float x0 = m_Flow.Player.Transform.position.x;
            m_In.Current.move = 1f; m_In.Current.dashPressed = true;
            yield return null; yield return null;
            Assert.IsTrue(m_Flow.Player.Controller.Dashing, "should be dashing");
            yield return Seconds(0.2f);
            float dist = m_Flow.Player.Transform.position.x - x0;
            Assert.Greater(dist, 2.2f, "dash distance");
            m_In.Current.move = 0f; yield return Seconds(1f);
            // in the air
            m_In.Current.jumpPressed = true; m_In.Current.jumpHeld = true; yield return Seconds(0.25f);
            float y0 = m_Flow.Player.Transform.position.y;
            m_In.Current.move = 1f; m_In.Current.dashPressed = true; yield return null; yield return null;
            float yMid = m_Flow.Player.Transform.position.y; yield return Seconds(0.1f);
            Assert.That(Mathf.Abs(m_Flow.Player.Transform.position.y - yMid), Is.LessThan(0.2f), "air dash should stay level");
        }

        [UnityTest]
        public IEnumerator Shooting_FiresRepeatedly_Bounded_AndCleansUp()
        {
            Spawn(); yield return Seconds(1f);
            m_In.Current.shootHeld = true;
            int maxActive = 0;
            for (float t = 0; t < 1f; t += Time.deltaTime) { maxActive = Mathf.Max(maxActive, m_Flow.Player.Shooter.ActiveShots); yield return null; }
            m_In.Current.shootHeld = false;
            Assert.That(m_Flow.Player.Shooter.ShotsFired, Is.InRange(6, 12), "shots in one second");
            Assert.LessOrEqual(maxActive, m_Flow.Player.Shooter.MaxShots + 4);
            yield return Seconds(2.5f);
            Assert.AreEqual(0, m_Flow.Player.Shooter.ActiveShots, "stars must despawn");
        }

        [UnityTest]
        public IEnumerator Stars_DamageTheBoss_AndChargeSuper()
        {
            Spawn(); yield return Seconds(0.5f);
            yield return EnterFight(LevelLayout.ArenaLeft + 8f);
            int before = m_Flow.Boss.Health.Current; float meter0 = m_Flow.Player.Meter.Value;
            m_In.Current.shootHeld = true; yield return Seconds(3f); m_In.Current.shootHeld = false;
            Assert.Less(m_Flow.Boss.Health.Current, before - 10, "boss should have taken damage");
            Assert.Greater(m_Flow.Player.Meter.Value, meter0 + 5f, "meter should charge");
        }

        [UnityTest]
        public IEnumerator Super_FiresBigStar_DamagesBoss_AndSpendsMeter()
        {
            Spawn(); yield return Seconds(0.5f);
            yield return EnterFight(LevelLayout.ArenaLeft + 6f);
            m_Flow.Player.Meter.Add(100f);
            int before = m_Flow.Boss.Health.Current;
            m_In.Current.superPressed = true; yield return Seconds(1.6f);
            Assert.AreEqual(0f, m_Flow.Player.Meter.Value, 0.01f);
            Assert.LessOrEqual(m_Flow.Boss.Health.Current, before - 12, "super should deal about 12");
        }

        [UnityTest]
        public IEnumerator Phases_ProtectTheBoss_AndTheFightCanBeWon()
        {
            var t = BossTuning.CreateDefault(); t.transitionSeconds = 0.4f;
            Spawn(t); yield return Seconds(0.3f);
            yield return EnterFight();
            var h = m_Flow.Boss.Health;
            h.TakeDamage(40);                                             // 120 -> 80 would pass the 70 percent line (84): clamped to 84
            Assert.AreEqual(84, h.Current); Assert.IsTrue(h.Invulnerable, "invulnerable the moment the line is crossed");
            Assert.IsFalse(h.TakeDamage(12), "no cheap hits during the transition");
            float g = 0; while (m_Flow.Boss.State != BossState.Transition && g < 1f) { g += Time.deltaTime; yield return null; }
            yield return Seconds(0.8f);
            Assert.AreEqual(2, m_Flow.Boss.Phase); Assert.IsFalse(h.Invulnerable, "vulnerable again after the transition");
            h.TakeDamage(80);                                             // past the 35 percent line (42): clamped
            Assert.AreEqual(42, h.Current); Assert.IsTrue(h.Invulnerable);
            yield return Seconds(1.0f);
            Assert.AreEqual(3, m_Flow.Boss.Phase);
            h.TakeDamage(500);
            float w = 0; while (m_Flow.State != LevelState.Victory && w < 8f) { w += Time.deltaTime; yield return null; }
            Assert.AreEqual(LevelState.Victory, m_Flow.State);
            Assert.AreEqual(0, m_Flow.BossCtx.LiveHazards, "no hazards after the boss is defeated");
        }

        [UnityTest]
        public IEnumerator AttackCycle_UsesAllAttacks_NeverTriples_RespectsHazardCap()
        {
            Spawn(); yield return Seconds(0.3f);
            yield return EnterFight(LevelLayout.ArenaLeft + 4f);
            Time.timeScale = 4f;
            int maxLive = 0; float end = Time.time + 50f;
            while (Time.time < end && m_Flow.State == LevelState.Fight) { maxLive = Mathf.Max(maxLive, m_Flow.BossCtx.LiveHazards); yield return null; }
            Time.timeScale = 1f;
            var hist = m_Flow.Boss.AttackHistory;
            Assert.GreaterOrEqual(hist.Count, 8, "boss should have attacked many times");
            Assert.Contains("Cupcake Toss", hist); Assert.Contains("Hand Slam", hist); Assert.Contains("Frost Blob", hist);
            for (int i = 2; i < hist.Count; i++) Assert.IsFalse(hist[i] == hist[i - 1] && hist[i] == hist[i - 2], "triple repeat at " + i);
            Assert.LessOrEqual(maxLive, m_Flow.Tuning.maxLiveHazards);
        }

        [UnityTest]
        public IEnumerator StandingStill_EventuallyGetsHit_ButNeverBeforeTheFirstWarning()
        {
            Spawn(); yield return Seconds(0.3f);
            yield return EnterFight(LevelLayout.ArenaLeft + 8f, false);
            m_Flow.Player.Health.PostHitInvulnerability = 1f;
            float firstWarn = -1f, firstHit = -1f; float start = Time.time;
            System.Action<Cue> on = c => { if (c == Cue.Telegraph && firstWarn < 0) firstWarn = Time.time - start; if (c == Cue.PlayerHit && firstHit < 0) firstHit = Time.time - start; };
            AudioHooks.Played += on;
            Time.timeScale = 3f;
            while (Time.time - start < 60f && !m_Flow.Player.Health.Dead) yield return null;
            Time.timeScale = 1f; AudioHooks.Played -= on;
            Assert.GreaterOrEqual(firstWarn, 0f, "boss must warn");
            Assert.Greater(firstHit, 0f, "a player who never moves should get hit (otherwise the attacks do nothing)");
            Assert.GreaterOrEqual(firstHit, firstWarn + BossTuning.MinTelegraphSeconds, "damage came before the minimum warning time");
        }

        [UnityTest]
        public IEnumerator Death_ShowsRetry_AndRetryRestartsAtTheCheckpoint()
        {
            Spawn(); yield return Seconds(0.3f);
            yield return EnterFight(LevelLayout.ArenaLeft + 6f, false);
            m_Flow.Player.Health.PostHitInvulnerability = 0f;
            for (int i = 0; i < 3; i++) { m_Flow.Player.Health.TakeHit(1, Vector2.zero); yield return null; }
            Assert.AreEqual(LevelState.Dead, m_Flow.State);
            yield return Seconds(0.2f);
            Assert.AreEqual(0, m_Flow.BossCtx.LiveHazards);
            m_Flow.Retry();
            yield return null; yield return null; yield return Seconds(1.5f);
            var again = LevelFlow.Current;
            Assert.IsNotNull(again, "level reloaded"); Assert.AreNotSame(m_Flow, again);
            Assert.GreaterOrEqual(again.Player.Transform.position.x, LevelLayout.ArenaLeft, "retry starts at the boss checkpoint");
            Assert.AreEqual(3, again.Player.Health.Current);
            Assert.AreEqual(LevelState.Fight, again.State);
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator Pause_FreezesTime_AndResumeRestoresIt()
        {
            Spawn(); yield return null;
            m_Flow.SetPaused(true); Assert.AreEqual(0f, Time.timeScale); Assert.IsTrue(m_Flow.HudUi.PauseOpen);
            m_Flow.SetPaused(false); Assert.AreEqual(1f, Time.timeScale); Assert.IsFalse(m_Flow.HudUi.PauseOpen);
        }

        [UnityTest]
        public IEnumerator AssistMode_GivesMoreHeartsAndLessBossHealth()
        {
            GameSettings.AssistMode = true;
            Spawn(); yield return null;
            Assert.AreEqual(5, m_Flow.Player.Health.Max); Assert.Less(m_Flow.Boss.Health.Max, 120);
            GameSettings.AssistMode = false;
        }

        [UnityTest]
        public IEnumerator Screenshots_ForVisualReview()
        {
            string dir = Path.Combine(Application.dataPath, "..", "Logs", "shots"); Directory.CreateDirectory(dir);
            Spawn(); yield return Seconds(1.0f);
            yield return Shot(Path.Combine(dir, "01_tutorial_start.png"));
            m_Flow.Player.Controller.Teleport(new Vector2(22f, 0.3f)); yield return Seconds(0.6f);
            yield return Shot(Path.Combine(dir, "02_tutorial_blocks.png"));
            m_Flow.Player.Controller.Teleport(new Vector2(50f, 0.3f)); yield return Seconds(0.6f);
            yield return Shot(Path.Combine(dir, "03_tutorial_targets.png"));
            yield return EnterFight(LevelLayout.ArenaLeft + 4f);
            yield return Seconds(1.0f);
            yield return Shot(Path.Combine(dir, "04_fight_idle.png"));
            float g = 0; while (m_Flow.Boss.State != BossState.Telegraph && g < 6f) { g += Time.deltaTime; yield return null; }
            yield return Seconds(0.5f);
            yield return Shot(Path.Combine(dir, "05_fight_telegraph_" + m_Flow.Boss.CurrentAttackName.Replace(' ', '_') + ".png"));
            m_In.Current.shootHeld = true; yield return Seconds(1.0f); m_In.Current.shootHeld = false;
            yield return Shot(Path.Combine(dir, "06_fight_shooting.png"));
            Assert.IsTrue(File.Exists(Path.Combine(dir, "01_tutorial_start.png")));
        }

        IEnumerator Shot(string path)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.Destroy(tex);
        }
    }
}

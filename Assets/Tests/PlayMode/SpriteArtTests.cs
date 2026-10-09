using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Saiyan.Art;
using Saiyan.Core;
using Saiyan.Level;
using Saiyan.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Saiyan.Tests
{
    /// <summary>Tests for the real generated Saiyan sprite art and its Unity integration (import settings, library, visual, preview scene).</summary>
    public class SpriteArtTests
    {
        Scene m_Scene; LevelFlow m_Flow; ScriptedIntent m_In;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f; GameSession.Reset(); PlayerPrefs.DeleteKey(SaiyanSpriteVisual.PrefKey); SpriteLibrary.ClearCache();
            m_Scene = SceneManager.CreateScene("S_" + TestContext.CurrentContext.Test.Name); SceneManager.SetActiveScene(m_Scene);
        }

        [UnityTearDown]
        public IEnumerator TearDown() { PlayerPrefs.DeleteKey(SaiyanSpriteVisual.PrefKey); yield return TestScenes.Cleanup(m_Scene); }

        LevelFlow Spawn(bool sprites)
        {
            PlayerPrefs.SetInt(SaiyanSpriteVisual.PrefKey, sprites ? 1 : 0);
            var go = new GameObject("flow"); go.SetActive(false);
            m_Flow = go.AddComponent<LevelFlow>(); m_In = go.AddComponent<ScriptedIntent>(); m_Flow.InputOverride = m_In; go.SetActive(true);
            return m_Flow;
        }

        static IEnumerator Seconds(float s) { float t = 0f; while (t < s) { t += Time.deltaTime; yield return null; } }

        [Test]
        public void Library_LoadsSaiyanIdle_WithTheImportSettingsFromAnimJson()
        {
            Assert.IsTrue(SpriteLibrary.HasCharacter("Saiyan"), "library.json for Saiyan");
            Assert.IsFalse(SpriteLibrary.HasCharacter("KingCakezilla"), "Cakezilla has no art yet");
            Assert.IsTrue(SpriteLibrary.TryGet("Saiyan", "idle", out var clip));
            Assert.AreEqual(8, clip.Frames.Length); Assert.AreEqual(8f, clip.Fps); Assert.IsTrue(clip.Loop);
            for (int i = 0; i < clip.Frames.Length; i++)
            {
                var s = clip.Frames[i];
                Assert.AreEqual(200f, s.pixelsPerUnit, 0.01f, "ppu frame " + i);
                Assert.AreEqual(512f, s.rect.width); Assert.AreEqual(512f, s.rect.height);
                Assert.AreEqual(0.515f, s.pivot.x / s.rect.width, 0.01f, "pivot x frame " + i);       // feet centre
                Assert.AreEqual(0.033f, s.pivot.y / s.rect.height, 0.01f, "pivot y frame " + i);      // feet baseline, just above the bottom edge
                StringAssert.StartsWith("saiyan_idle_", s.name);
            }
            Assert.IsNotNull(clip.Frames[0].texture);
            Assert.IsFalse(clip.Frames[0].texture.mipmapCount > 1, "no mipmaps");
        }

        [UnityTest]
        public IEnumerator SpriteVisual_ShowsInTheLevel_Animates_AndHidesThePlaceholder()
        {
            Spawn(true); yield return Seconds(0.4f);
            var sv = m_Flow.Player.SpriteVisual;
            Assert.IsTrue(sv.HasArt); Assert.IsTrue(sv.Active, "sprite art on");
            Assert.IsFalse(m_Flow.Player.Visual.Visible, "placeholder hidden while sprites show");
            Assert.AreEqual("idle", sv.Flipbook.CurrentName); Assert.IsNotNull(sv.Renderer.sprite);
            var frames = new HashSet<int>();
            for (float t = 0; t < 1.3f; t += Time.deltaTime) { frames.Add(sv.Flipbook.Frame); yield return null; }
            Assert.GreaterOrEqual(frames.Count, 6, "the idle loop should cycle through its frames");
        }

        [UnityTest]
        public IEnumerator SpriteVisual_ToggleSwitchesBackToThePlaceholder()
        {
            Spawn(false); yield return Seconds(0.3f);
            var sv = m_Flow.Player.SpriteVisual;
            Assert.IsFalse(sv.Active); Assert.IsTrue(m_Flow.Player.Visual.Visible);
            sv.Toggle(); yield return null;
            Assert.IsTrue(sv.Active); Assert.IsFalse(m_Flow.Player.Visual.Visible); Assert.AreEqual(1, PlayerPrefs.GetInt(SaiyanSpriteVisual.PrefKey));
            sv.Toggle(); yield return null;
            Assert.IsFalse(sv.Active); Assert.IsTrue(m_Flow.Player.Visual.Visible);
        }

        [UnityTest]
        public IEnumerator Sprite_StandsOnTheGround_AndTheHitboxIsIndependentOfTheArt()
        {
            Spawn(true); yield return Seconds(1f);
            var p = m_Flow.Player;
            Assert.IsTrue(p.Controller.Grounded); Assert.AreEqual(0f, p.Transform.position.y, 0.08f, "feet on the floor");
            var cap = p.Go.GetComponent<CapsuleCollider2D>();
            Assert.AreEqual(0.6f, cap.size.x, 1e-4f); Assert.AreEqual(1.3f, cap.size.y, 1e-4f);        // the art is about 2.3 units tall, the hitbox stays small and fair
            var sprite = p.SpriteVisual.Renderer.sprite;
            float feetWorld = p.SpriteVisual.Renderer.transform.position.y;                           // pivot (feet baseline) sits on the player origin
            Assert.AreEqual(p.Transform.position.y, feetWorld, 0.02f);
            Assert.Greater(sprite.bounds.size.y, 2.4f, "512 px canvas at 200 ppu");
            // gameplay still works with the sprite on: a jump reaches the normal height
            m_In.Current.jumpPressed = true; m_In.Current.jumpHeld = true; float max = 0f;
            for (float t = 0; t < 1.0f; t += Time.deltaTime) { max = Mathf.Max(max, p.Transform.position.y); yield return null; }
            Assert.That(max, Is.InRange(2.2f, 2.9f));
        }

        [UnityTest]
        public IEnumerator FacingLeft_MirrorsTheSprite()
        {
            Spawn(true); yield return Seconds(0.6f);
            var sv = m_Flow.Player.SpriteVisual;
            m_In.Current.move = -1f; yield return Seconds(0.4f);
            Assert.Less(sv.Renderer.transform.localScale.x, 0f, "facing left flips the drawing");
            m_In.Current.move = 1f; yield return Seconds(0.4f);
            Assert.Greater(sv.Renderer.transform.localScale.x, 0f);
        }

        [UnityTest]
        public IEnumerator PreviewScene_LoadsAndPlaysTheIdle()
        {
            SceneManager.LoadScene(AnimationPreview.SceneName); yield return null; yield return null;
            var prev = Object.FindFirstObjectByType<AnimationPreview>();
            Assert.IsNotNull(prev, "preview component"); CollectionAssert.Contains(new List<string>(prev.Animations), "idle");
            Assert.AreEqual("idle", prev.Flipbook.CurrentName);
            var seen = new HashSet<int>();
            for (float t = 0; t < 1.2f; t += Time.deltaTime) { seen.Add(prev.Flipbook.Frame); yield return null; }
            Assert.GreaterOrEqual(seen.Count, 5);
            prev.Paused = true; yield return null; int f = prev.Flipbook.Frame; yield return Seconds(0.4f);
            Assert.AreEqual(f, prev.Flipbook.Frame, "paused");
            prev.Flipbook.StepFrame(1); Assert.AreEqual((f + 1) % 8, prev.Flipbook.Frame, "frame step");
        }
    }
}

using System.Collections;
using NUnit.Framework;
using Saiyan.Core;
using Saiyan.Level;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Saiyan.Tests
{
    /// <summary>
    /// The other tests inject input through ScriptedIntent. These go through the REAL Input System: virtual keyboard, gamepad and mouse devices, the shipped
    /// Input Actions asset, and the uGUI input module. (A first headset-style playtest found dead buttons because the project used the old Input Manager; these tests exist so that cannot happen silently.)
    /// </summary>
    public class RealInputTests
    {
        Keyboard m_Kb; Gamepad m_Pad; Mouse m_Mouse; Scene m_Scene; LevelFlow m_Flow;
        InputSettings.BackgroundBehavior m_OldBg; InputSettings.EditorInputBehaviorInPlayMode m_OldEd;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f; GameSession.Reset();
            m_OldBg = InputSystem.settings.backgroundBehavior; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            // in the editor, keyboard and mouse input normally only reaches a FOCUSED Game view; the test has no focus, so route everything to the game
            m_OldEd = InputSystem.settings.editorInputBehaviorInPlayMode; InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            m_Kb = InputSystem.AddDevice<Keyboard>(); m_Pad = InputSystem.AddDevice<Gamepad>(); m_Mouse = InputSystem.AddDevice<Mouse>();
            m_Scene = SceneManager.CreateScene("R_" + TestContext.CurrentContext.Test.Name); SceneManager.SetActiveScene(m_Scene);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Release();
            foreach (var d in new InputDevice[] { m_Kb, m_Pad, m_Mouse }) if (d != null && d.added) InputSystem.RemoveDevice(d);
            InputSystem.settings.backgroundBehavior = m_OldBg; InputSystem.settings.editorInputBehaviorInPlayMode = m_OldEd;
            yield return TestScenes.Cleanup(m_Scene);
        }

        LevelFlow SpawnLevel()
        {
            var go = new GameObject("flow"); go.SetActive(false);
            m_Flow = go.AddComponent<LevelFlow>();            // no InputOverride: uses the real Input Actions
            go.SetActive(true); return m_Flow;
        }

        static IEnumerator Seconds(float s) { float t = 0f; while (t < s) { t += Time.unscaledDeltaTime; yield return null; } }

        void Keys(params Key[] keys) { InputSystem.QueueStateEvent(m_Kb, new KeyboardState(keys)); }
        void Release() { if (m_Kb != null && m_Kb.added) Keys(); if (m_Pad != null && m_Pad.added) InputSystem.QueueStateEvent(m_Pad, new GamepadState()); if (m_Mouse != null && m_Mouse.added) InputSystem.QueueStateEvent(m_Mouse, new MouseState { position = m_Mouse.position.ReadValue() }); }
        void Pad(Vector2 stick, params GamepadButton[] buttons) { var s = new GamepadState(buttons); s.leftStick = stick; InputSystem.QueueStateEvent(m_Pad, s); }

        [UnityTest]
        public IEnumerator Keyboard_MovesJumpsShootsAndDashes()
        {
            SpawnLevel(); yield return Seconds(1f);
            var p = m_Flow.Player; float x0 = p.Transform.position.x;
            Keys(Key.D); yield return Seconds(0.5f);
            Assert.Greater(p.Transform.position.x - x0, 1.5f, "D should run right");
            Keys(Key.D, Key.Space); yield return Seconds(0.25f);
            Assert.Greater(p.Transform.position.y, 0.6f, "Space should jump");
            Keys(); yield return Seconds(1.2f);
            Keys(Key.J); yield return Seconds(0.6f); Keys();
            Assert.GreaterOrEqual(p.Shooter.ShotsFired, 3, "J should shoot");
            bool dashed = false; Keys(Key.D, Key.K);
            for (float t = 0; t < 0.3f; t += Time.unscaledDeltaTime) { dashed |= p.Controller.Dashing; yield return null; }
            Assert.IsTrue(dashed, "K should dash");
        }

        [UnityTest]
        public IEnumerator ArrowKeys_Move()
        {
            SpawnLevel(); yield return Seconds(1f);
            float x0 = m_Flow.Player.Transform.position.x;
            Keys(Key.RightArrow); yield return Seconds(0.5f);
            Assert.Greater(m_Flow.Player.Transform.position.x - x0, 1.5f);
        }

        [UnityTest]
        public IEnumerator Gamepad_MovesJumpsShootsAndDashes()
        {
            SpawnLevel(); yield return Seconds(1f);
            var p = m_Flow.Player; float x0 = p.Transform.position.x;
            Pad(new Vector2(1f, 0f)); yield return Seconds(0.5f);
            Assert.Greater(p.Transform.position.x - x0, 1.5f, "left stick should run right");
            Pad(new Vector2(1f, 0f), GamepadButton.South); yield return Seconds(0.25f);
            Assert.Greater(p.Transform.position.y, 0.6f, "A should jump");
            Pad(Vector2.zero); yield return Seconds(1.2f);
            Pad(Vector2.zero, GamepadButton.West); yield return Seconds(0.6f); Pad(Vector2.zero);
            Assert.GreaterOrEqual(p.Shooter.ShotsFired, 3, "X should shoot");
            bool dashed = false; Pad(new Vector2(1f, 0f), GamepadButton.East);
            for (float t = 0; t < 0.3f; t += Time.unscaledDeltaTime) { dashed |= p.Controller.Dashing; yield return null; }
            Assert.IsTrue(dashed, "B should dash");
            Pad(Vector2.zero); yield return Seconds(0.8f);
            dashed = false; Pad(new Vector2(1f, 0f), GamepadButton.RightShoulder);
            for (float t = 0; t < 0.3f; t += Time.unscaledDeltaTime) { dashed |= p.Controller.Dashing; yield return null; }
            Assert.IsTrue(dashed, "RB should dash");
        }

        [UnityTest]
        public IEnumerator Gamepad_DpadMoves_AndSuperButtonFires()
        {
            SpawnLevel(); yield return Seconds(1f);
            float x0 = m_Flow.Player.Transform.position.x;
            var s = new GamepadState(GamepadButton.DpadRight); InputSystem.QueueStateEvent(m_Pad, s); yield return Seconds(0.5f);
            Assert.Greater(m_Flow.Player.Transform.position.x - x0, 1.5f, "d-pad should run right");
            InputSystem.QueueStateEvent(m_Pad, new GamepadState()); yield return Seconds(0.3f);
            m_Flow.Player.Meter.Add(100f);
            InputSystem.QueueStateEvent(m_Pad, new GamepadState(GamepadButton.North)); yield return Seconds(0.2f);
            Assert.AreEqual(0f, m_Flow.Player.Meter.Value, 0.01f, "Y should fire the Super");
        }

        [UnityTest]
        public IEnumerator Keyboard_SuperAndPauseWork()
        {
            SpawnLevel(); yield return Seconds(1f);
            m_Flow.Player.Meter.Add(100f);
            Keys(Key.L); yield return Seconds(0.2f); Keys(); yield return Seconds(0.2f);
            Assert.AreEqual(0f, m_Flow.Player.Meter.Value, 0.01f, "L should fire the Super");
            Keys(Key.Escape); yield return Seconds(0.2f); Keys(); yield return Seconds(0.2f);
            Assert.IsTrue(m_Flow.Paused, "Esc should pause");
            Keys(Key.Escape); yield return Seconds(0.2f); Keys(); yield return Seconds(0.2f);
            Assert.IsFalse(m_Flow.Paused, "Esc again should resume");
            Pad(Vector2.zero, GamepadButton.Start); yield return Seconds(0.2f); Pad(Vector2.zero); yield return Seconds(0.2f);
            Assert.IsTrue(m_Flow.Paused, "Start should pause");
            m_Flow.SetPaused(false);
        }

        IEnumerator LoadMenu()
        {
            SceneManager.LoadScene(LevelFlow.MenuSceneName); yield return null; yield return Seconds(0.5f);
        }

        [UnityTest]
        public IEnumerator Menu_GamepadSubmitStartsTheGame()
        {
            yield return LoadMenu();
            Assert.IsNotNull(EventSystem.current, "menu needs an EventSystem");
            Assert.IsNotNull(EventSystem.current.currentSelectedGameObject, "a button should be pre-selected for gamepad users");
            InputSystem.QueueStateEvent(m_Pad, new GamepadState(GamepadButton.South)); yield return Seconds(0.15f);
            InputSystem.QueueStateEvent(m_Pad, new GamepadState()); yield return Seconds(1.5f);
            Assert.IsNotNull(LevelFlow.Current, "pressing A on Play should load the level");
        }

        [UnityTest]
        public IEnumerator Menu_MouseClickOnPlayStartsTheGame()
        {
            yield return LoadMenu();
            var play = GameObject.Find("Btn_Play"); Assert.IsNotNull(play, "Play button");
            var screen = RectTransformUtility.WorldToScreenPoint(null, play.transform.position);
            InputSystem.QueueStateEvent(m_Mouse, new MouseState { position = screen }); yield return Seconds(0.3f);
            InputSystem.QueueStateEvent(m_Mouse, new MouseState { position = screen, buttons = 1 }); yield return Seconds(0.2f);
            InputSystem.QueueStateEvent(m_Mouse, new MouseState { position = screen }); yield return Seconds(1.5f);
            Assert.IsNotNull(LevelFlow.Current, "clicking Play should load the level");
        }

        [UnityTest]
        public IEnumerator Menu_GamepadNavigatesToControlsAndBack()
        {
            yield return LoadMenu();
            InputSystem.QueueStateEvent(m_Pad, new GamepadState(GamepadButton.DpadDown)); yield return Seconds(0.15f);
            InputSystem.QueueStateEvent(m_Pad, new GamepadState()); yield return Seconds(0.2f);
            Assert.AreEqual("Btn_Controls", EventSystem.current.currentSelectedGameObject.name, "d-pad down should move to Controls");
            InputSystem.QueueStateEvent(m_Pad, new GamepadState(GamepadButton.South)); yield return Seconds(0.15f);
            InputSystem.QueueStateEvent(m_Pad, new GamepadState()); yield return Seconds(0.3f);
            Assert.AreEqual("Btn_Back", EventSystem.current.currentSelectedGameObject.name, "A should open Controls and select Back");
        }

        [UnityTest]
        public IEnumerator PauseMenu_ResumeButtonWorksWithGamepad()
        {
            SpawnLevel(); yield return Seconds(0.8f);
            m_Flow.SetPaused(true); yield return Seconds(0.3f);
            Assert.AreEqual(0f, Time.timeScale);
            InputSystem.QueueStateEvent(m_Pad, new GamepadState(GamepadButton.South)); yield return Seconds(0.15f);
            InputSystem.QueueStateEvent(m_Pad, new GamepadState()); yield return Seconds(0.3f);
            Assert.IsFalse(m_Flow.Paused, "A on the selected Resume button should resume");
            Assert.AreEqual(1f, Time.timeScale);
        }
    }
}

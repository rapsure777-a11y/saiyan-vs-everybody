using System.Collections;
using Saiyan.Boss;
using Saiyan.Core;
using Saiyan.Player;
using Saiyan.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Saiyan.Level
{
    public enum LevelState { Tutorial, BossIntro, Fight, Victory, Dead }

    /// <summary>
    /// Owns Level01_FrostingFields: builds the world, the player, the boss and the HUD, and runs the flow
    /// Tutorial -> BossIntro (skippable) -> Fight -> Victory, plus death/retry from the checkpoint and pause.
    /// The scene only contains this component; everything else is created here (see AI_HANDOFF/PROJECT_STATUS.md for how that was tested).
    /// </summary>
    public sealed class LevelFlow : MonoBehaviour
    {
        public const string SceneName = "Level01_FrostingFields", MenuSceneName = "MainMenu";

        public BossTuning Tuning;                         // optional: assign the asset; a default is created when empty
        /// <summary>Tests can replace the device input with a script before Start.</summary>
        public IIntentSource InputOverride;

        public LevelState State { get; private set; } = LevelState.Tutorial;
        public PlayerFactory.Player Player { get; private set; }
        public BossController Boss { get; private set; }
        public BossContext BossCtx { get; private set; }
        public Hud HudUi { get; private set; }
        public CameraRig Rig { get; private set; }
        public ArenaBuilder Arena { get; private set; }
        public bool Paused { get; private set; }
        public static LevelFlow Current { get; private set; }

        IIntentSource m_Input; float m_StateTime;

        void Awake()
        {
            Current = this;
            Time.timeScale = 1f;
            Physics2D.gravity = new Vector2(0f, -9.81f);
            BuildWorld();
        }

        void OnDestroy() { if (Current == this) Current = null; Time.timeScale = 1f; }

        void BuildWorld()
        {
            // camera
            var cam = Camera.main;
            if (!cam) { var cg = new GameObject("Main Camera") { tag = "MainCamera" }; cam = cg.AddComponent<Camera>(); cg.AddComponent<AudioListener>(); }
            Rig = cam.GetComponent<CameraRig>(); if (!Rig) Rig = cam.gameObject.AddComponent<CameraRig>();
            // input
            if (InputOverride != null) m_Input = InputOverride;
            else m_Input = gameObject.AddComponent<InputActionsIntent>();
            // player (needs the meter before the level so pickups and dummies can charge it)
            bool retry = GameSession.StartAtCheckpoint;
            float startX = retry ? LevelLayout.RetrySpawnX : LevelLayout.PlayerStartX;
            Player = PlayerFactory.Create(new Vector2(startX, 0.3f), m_Input, GameSettings.PlayerHearts);
            Player.Health.Died += OnPlayerDied;
            Arena = ArenaBuilder.Build(Rig, Player.Meter);
            // boss
            if (!Tuning) Tuning = BossTuning.CreateDefault();
            BuildBoss();
            // hud
            HudUi = Hud.Create(Player.Health, Player.Meter, Boss.Health);
            HudUi.OnResume = () => SetPaused(false); HudUi.OnRetry = Retry; HudUi.OnMenu = ToMenu; HudUi.OnPlayAgain = PlayAgain;
            // camera follows through the tutorial; clamps so it never shows past the level
            Rig.Target = Player.Transform; Rig.MinX = LevelLayout.LevelStartX + LevelLayout.CameraWidth * 0.5f; Rig.MaxX = LevelLayout.ArenaCentreX;
            Rig.SnapTo(Mathf.Clamp(startX + 1.5f, Rig.MinX, Rig.MaxX));
            if (retry) { GameSession.Attempts++; BeginBossIntro(true); }
        }

        void BuildBoss()
        {
            var root = new GameObject("KingCakezilla"); root.transform.position = new Vector3(LevelLayout.BossRootX, LevelLayout.FloorY, 0f);
            var visual = root.AddComponent<BossVisual>(); visual.Build(); visual.LookAt(Player.Transform);
            var health = root.AddComponent<BossHealth>();
            health.Configure(Mathf.RoundToInt(Tuning.maxHealth * GameSettings.BossHealthScale), Tuning.phase2At, Tuning.phase3At);
            visual.Hurtbox.Health = health;
            BossCtx = new BossContext { Tuning = Tuning, Player = Player.Transform, PlayerHealth = Player.Health, Visual = visual, ArenaLeft = LevelLayout.ArenaLeft, FloorY = LevelLayout.FloorY, WallX = LevelLayout.BossWallX, Phase = 1 };
            Boss = root.AddComponent<BossController>(); Boss.Tuning = Tuning; Boss.Ctx = BossCtx; Boss.Health = health; Boss.Visual = visual;
            Boss.StateChanged += OnBossState; Boss.PhaseChanged += OnPhaseChanged; Boss.Defeated += OnBossDefeated;
            health.Hit += () => CameraShake.Shake(0.03f, 0.05f);
        }

        void Update()
        {
            m_StateTime += Time.unscaledDeltaTime;
            var i = m_Input.Read();
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.f9Key.wasPressedThisFrame && Player.SpriteVisual) { Player.SpriteVisual.Toggle(); HudUi.Banner(Player.SpriteVisual.Active ? "SPRITE ART (F9)" : "PLACEHOLDER ART (F9)", 1.2f); }
            if (i.pausePressed && (State == LevelState.Tutorial || State == LevelState.Fight || State == LevelState.BossIntro) && !Player.Health.Dead) SetPaused(!Paused);
            if (Paused) return;
            if (State == LevelState.Tutorial && Player.Transform.position.x >= LevelLayout.FightTriggerX) BeginBossIntro(false);
            if (State == LevelState.BossIntro && m_StateTime > 0.5f && i.anyPressed) Boss.SkipIntro();
        }

        void Enter(LevelState s) { State = s; m_StateTime = 0f; }

        public void BeginBossIntro(bool skip)
        {
            if (State != LevelState.Tutorial) return;
            Enter(LevelState.BossIntro);
            Arena.LeftWall.SetActive(true);
            Rig.Locked = true; Rig.LockX = LevelLayout.ArenaCentreX;
            Player.Controller.InputLocked = true; Player.Controller.Stop();
            HudUi.HideHint(); HudUi.ShowBossBar(true);
            HudUi.Banner(skip ? "" : "KING CAKEZILLA!", 2.4f);
            Boss.Begin(skip);
            if (!skip) StartCoroutine(IntroHint());
        }

        IEnumerator IntroHint() { yield return new WaitForSeconds(0.2f); HudUi.Banner("KING CAKEZILLA!\n<size=40>press any button to skip</size>", 2.6f); }

        void OnBossState(BossState s)
        {
            if (State == LevelState.BossIntro && s != BossState.Intro)
            {
                Enter(LevelState.Fight);
                if (!Player.Health.Dead) Player.Controller.InputLocked = false;
                HudUi.Banner("FIGHT!", 1.2f);
            }
        }

        void OnPhaseChanged(int phase) { HudUi.Banner(phase == 2 ? "PHASE 2!" : "CAKEZILLA SUPREME!", 1.8f); }

        void OnPlayerDied()
        {
            if (State == LevelState.Victory) return;
            Enter(LevelState.Dead);
            StartCoroutine(DeathRoutine());
        }

        IEnumerator DeathRoutine()
        {
            Player.Controller.Stop();
            Boss.Halt();                                              // the boss and every hazard stop the moment you fall
            BossCtx.ClearHazards();
            yield return new WaitForSecondsRealtime(1.2f);
            HudUi.ShowDead();
        }

        void OnBossDefeated()
        {
            Enter(LevelState.Victory);
            Player.Controller.InputLocked = true; Player.Controller.Stop();
            Player.Health.GrantInvulnerability(60f);
            StartCoroutine(VictoryRoutine());
        }

        IEnumerator VictoryRoutine()
        {
            HudUi.Banner("SWEET VICTORY!", 2f);
            yield return new WaitForSecondsRealtime(2.2f);
            HudUi.ShowWin();
        }

        public void SetPaused(bool on)
        {
            Paused = on; Time.timeScale = on ? 0f : 1f; HudUi.ShowPause(on);
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            GameSession.StartAtCheckpoint = true;
            SceneManager.LoadScene(SceneName);
        }

        void PlayAgain() { GameSession.Reset(); Time.timeScale = 1f; SceneManager.LoadScene(SceneName); }
        void ToMenu() { GameSession.Reset(); Time.timeScale = 1f; SceneManager.LoadScene(MenuSceneName); }
    }
}

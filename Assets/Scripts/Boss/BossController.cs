using System;
using System.Collections;
using System.Collections.Generic;
using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Boss
{
    public enum BossState { Intro, Idle, Telegraph, Attack, Recovery, Transition, Defeated }

    /// <summary>
    /// King Cakezilla's brain: Intro, Idle, Telegraph, Attack, Recovery, Transition, Defeated. Picks attacks with <see cref="AttackSelector"/>, never lets damage land
    /// during a transition, and clears every hazard on a phase change or defeat. Phase 2 and 3 currently reuse the phase 1 attack pool with a faster rhythm (see GAME_DESIGN.md).
    /// </summary>
    public sealed class BossController : MonoBehaviour
    {
        public BossTuning Tuning; public BossContext Ctx; public BossHealth Health; public BossVisual Visual;
        public BossState State { get; private set; } = BossState.Intro;
        public int Phase => Ctx != null ? Ctx.Phase : 1;
        public string CurrentAttackName { get; private set; } = "";
        public readonly List<string> AttackHistory = new List<string>();

        public event Action<BossState> StateChanged;
        public event Action<int> PhaseChanged;
        public event Action Defeated;

        readonly List<BossAttack> m_Attacks = new List<BossAttack> { new CupcakeToss(), new HandSlam(), new FrostBlob() };
        AttackSelector m_Selector = new AttackSelector();
        bool m_Interrupt, m_SkipIntro; int m_PendingPhase; Coroutine m_Loop; bool m_FightStarted;
        public bool FightStarted => m_FightStarted;
        public IReadOnlyList<BossAttack> Attacks => m_Attacks;

        /// <summary>Fixed seed for repeatable tests; 0 = random.</summary>
        public void SetSeed(int seed) { m_Selector = new AttackSelector(seed); }

        public void Begin(bool skipIntro)
        {
            if (m_FightStarted) return;
            m_FightStarted = true; m_SkipIntro = skipIntro;
            Health.ThresholdReached += OnThreshold;
            Health.DefeatedEvent += OnDefeated;
            m_Loop = StartCoroutine(Run());
        }

        public void SkipIntro() { m_SkipIntro = true; }

        void OnDestroy() { if (Health) { Health.ThresholdReached -= OnThreshold; Health.DefeatedEvent -= OnDefeated; } }

        void SetState(BossState s) { State = s; StateChanged?.Invoke(s); }

        void OnThreshold(int phase)
        {
            Health.Invulnerable = true;                   // no cheap hits during the cinematic, starting this very frame
            m_PendingPhase = phase; m_Interrupt = true;
        }

        void OnDefeated() { Health.Invulnerable = true; m_Interrupt = true; m_PendingPhase = 0; }

        IEnumerator Run()
        {
            SetState(BossState.Intro);
            Health.Invulnerable = true;
            AudioHooks.Play(Cue.PhaseChange, 0.6f);
            yield return Visual.SlideIn(m_SkipIntro ? 0.05f : 3.0f, () => m_SkipIntro);
            Health.Invulnerable = false;
            while (!Health.Defeated)
            {
                m_Interrupt = false;
                if (m_PendingPhase != 0) { yield return Transition(); continue; }
                SetState(BossState.Idle);
                yield return Wait(BossTuning.At(Tuning.idleSeconds, Ctx.Phase - 1));
                if (m_Interrupt) { yield return AfterInterrupt(); continue; }

                var weights = new List<float>(); foreach (var a in m_Attacks) weights.Add(a.Weight(Ctx));
                int idx = m_Selector.Pick(weights);
                if (idx < 0) continue;
                var attack = m_Attacks[idx];
                CurrentAttackName = attack.Name; AttackHistory.Add(attack.Name);
                SetState(BossState.Telegraph);
                yield return Drive(attack.Telegraph(Ctx));
                if (!m_Interrupt) { SetState(BossState.Attack); yield return Drive(attack.Execute(Ctx)); }
                if (m_Interrupt) { yield return AfterInterrupt(); continue; }
                SetState(BossState.Recovery);
                yield return Wait(BossTuning.At(Tuning.recoverySeconds, Ctx.Phase - 1));
                if (m_Interrupt) { yield return AfterInterrupt(); continue; }
            }
        }

        IEnumerator AfterInterrupt()
        {
            Ctx.ClearHazards(); Visual.ResetPose();
            if (Health.Defeated) { yield return Finish(); yield break; }
            if (m_PendingPhase != 0) yield return Transition();
        }

        IEnumerator Transition()
        {
            Ctx.ClearHazards(); Visual.ResetPose();
            SetState(BossState.Transition);
            int p = m_PendingPhase; m_PendingPhase = 0;
            Health.Invulnerable = true;
            AudioHooks.Play(Cue.PhaseChange);
            CameraShake.Shake(0.5f, Tuning.transitionSeconds * 0.8f);
            Visual.SetPhase(p);
            Fx.Burst((Vector2)Visual.transform.position + new Vector2(0f, 5f), p >= 3 ? Palette.Gold : Palette.Frosting, 24, 8f, 0.4f, 1.0f);
            yield return new WaitForSeconds(Tuning.transitionSeconds);
            Ctx.Phase = p;
            m_Interrupt = false;
            if (Health.Defeated) { yield return Finish(); yield break; }
            Health.Invulnerable = false;
            PhaseChanged?.Invoke(p);
        }

        IEnumerator Finish()
        {
            SetState(BossState.Defeated);
            Ctx.ClearHazards();
            AudioHooks.Play(Cue.Victory);
            yield return Visual.DefeatSequence(2.6f);
            Defeated?.Invoke();
        }

        /// <summary>Steps an attack coroutine but stops stepping the moment something interrupts (phase change, defeat).</summary>
        IEnumerator Drive(IEnumerator it)
        {
            while (!m_Interrupt && it.MoveNext()) yield return it.Current;
        }

        IEnumerator Wait(float seconds)
        {
            float t = 0f;
            while (t < seconds && !m_Interrupt) { t += Time.deltaTime; yield return null; }
        }

        /// <summary>Wipes the fight (retry): hazards gone, boss stops.</summary>
        public void Halt() { if (m_Loop != null) StopCoroutine(m_Loop); StopAllCoroutines(); Ctx?.ClearHazards(); }
    }
}

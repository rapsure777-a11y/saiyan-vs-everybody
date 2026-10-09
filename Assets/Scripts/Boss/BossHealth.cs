using System;
using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Boss
{
    /// <summary>
    /// Boss hit points and phase thresholds. A hit that would cross a threshold is clamped so the boss lands exactly on it, then the transition makes him
    /// invulnerable (<see cref="Invulnerable"/>); cheap hits during a cinematic are simply ignored.
    /// </summary>
    public sealed class BossHealth : MonoBehaviour
    {
        public int Max { get; private set; } = 120;
        public int Current { get; private set; } = 120;
        public bool Invulnerable;
        public bool Defeated { get; private set; }
        public float Fraction => Max <= 0 ? 0f : (float)Current / Max;

        /// <summary>1, 2 or 3.</summary>
        public int Phase { get; private set; } = 1;
        int m_Threshold2, m_Threshold3;

        public event Action<float> Changed;
        /// <summary>Raised when the boss reaches the 70 percent or 35 percent line; argument is the NEW phase (2 or 3).</summary>
        public event Action<int> ThresholdReached;
        public event Action Hit, DefeatedEvent;

        public void Configure(int max, float phase2At, float phase3At)
        {
            Max = Mathf.Max(1, max); Current = Max; Defeated = false; Phase = 1; Invulnerable = false;
            m_Threshold2 = Mathf.RoundToInt(Max * phase2At); m_Threshold3 = Mathf.RoundToInt(Max * phase3At);
            Changed?.Invoke(Fraction);
        }

        public static int PhaseFor(int current, int max, float phase2At, float phase3At)
        {
            int t2 = Mathf.RoundToInt(max * phase2At), t3 = Mathf.RoundToInt(max * phase3At);
            return current <= t3 ? 3 : current <= t2 ? 2 : 1;
        }

        /// <summary>Returns true when the damage was accepted.</summary>
        public bool TakeDamage(int amount)
        {
            if (Defeated || Invulnerable || amount <= 0) return false;
            int before = Current;
            int next = Mathf.Max(0, Current - amount);
            // stop exactly on a threshold so the transition always happens, never skipped by a big hit (the Super)
            int newPhase = next <= 0 ? Phase : (next <= m_Threshold3 ? 3 : next <= m_Threshold2 ? 2 : 1);
            if (newPhase > Phase && next > 0) { next = Phase == 1 ? m_Threshold2 : m_Threshold3; }
            Current = next;
            Hit?.Invoke(); Changed?.Invoke(Fraction);
            if (Current <= 0) { Defeated = true; DefeatedEvent?.Invoke(); return true; }
            int reached = Current <= m_Threshold3 ? 3 : Current <= m_Threshold2 ? 2 : 1;
            if (reached > Phase) { Phase = reached; ThresholdReached?.Invoke(reached); }
            return true;
        }
    }

    /// <summary>The part of the boss that stars hit. Forwards damage to <see cref="BossHealth"/> and gives hit feedback.</summary>
    public sealed class BossHurtbox : MonoBehaviour, IShootable
    {
        public BossHealth Health; public BossVisual Visual;
        float m_NextSound;

        public bool OnStarHit(int damage, Vector2 point)
        {
            if (!Health || Health.Defeated) return false;
            bool ok = Health.TakeDamage(damage);
            if (ok)
            {
                Visual?.Flash();
                if (Time.time >= m_NextSound) { AudioHooks.Play(Cue.BossHit, 0.6f); m_NextSound = Time.time + 0.06f; }
                Fx.Burst(point, Palette.Frosting, 3, 3f, 0.16f, 0.35f);
            }
            else Fx.Puff(point, 0.3f, new Color(1f, 1f, 1f, 0.8f), 0.2f);     // shot bounces off during transitions
            return true;
        }
    }

    /// <summary>Marks solid scenery that player stars fly through (the invisible wall in front of the boss, so shots reach his hurtbox).</summary>
    public sealed class ShotPassThrough : MonoBehaviour { }
}

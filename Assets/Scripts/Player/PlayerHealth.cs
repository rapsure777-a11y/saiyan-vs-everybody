using System;
using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Player
{
    /// <summary>
    /// Hearts, post-hit invulnerability (blinking) and death. Invulnerable sources: the ~1 s after a hit, the first part of a dash, and while the Super burst is firing.
    /// </summary>
    public sealed class PlayerHealth : MonoBehaviour
    {
        public int Max = 3;
        public int Current { get; private set; }
        public float PostHitInvulnerability = 1f;
        public bool Dead { get; private set; }
        public bool Blinking => m_Invuln > 0f;
        public bool Invulnerable => Dead || m_Invuln > 0f || m_Forced > 0f || (m_Controller && m_Controller.DashInvulnerable);

        public event Action<int, int> Changed;      // current, max
        public event Action Damaged, Died;

        PlayerController m_Controller;
        float m_Invuln, m_Forced;

        void Awake() { m_Controller = GetComponent<PlayerController>(); }
        void Start() { if (Current == 0 && !Dead) ResetHearts(Max); }

        public void ResetHearts(int max)
        {
            Max = max; Current = max; Dead = false; m_Invuln = 0f; m_Forced = 0f; Changed?.Invoke(Current, Max);
        }

        /// <summary>Returns true if the hit landed.</summary>
        public bool TakeHit(int damage, Vector2 sourcePosition)
        {
            if (Invulnerable) return false;
            Current = Mathf.Max(0, Current - Mathf.Max(1, damage));
            m_Invuln = PostHitInvulnerability;
            AudioHooks.Play(Cue.PlayerHit);
            Changed?.Invoke(Current, Max); Damaged?.Invoke();
            if (m_Controller) m_Controller.Knockback(sourcePosition);
            if (Current <= 0) { Dead = true; if (m_Controller) { m_Controller.InputLocked = true; } Died?.Invoke(); AudioHooks.Play(Cue.Defeat); }
            return true;
        }

        /// <summary>Extra protection that ignores post-hit rules (used by the Super burst and cutscenes).</summary>
        public void GrantInvulnerability(float seconds) { m_Forced = Mathf.Max(m_Forced, seconds); }

        void Update()
        {
            if (m_Invuln > 0f) m_Invuln -= Time.deltaTime;
            if (m_Forced > 0f) m_Forced -= Time.deltaTime;
        }
    }
}

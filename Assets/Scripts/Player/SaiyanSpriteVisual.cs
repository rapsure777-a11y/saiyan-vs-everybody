using Saiyan.Art;
using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Player
{
    /// <summary>
    /// Saiyan drawn with the generated sprite animations (Resources/Art/Saiyan). Plays the clip that matches the player's state, falling back to "idle" for any
    /// animation that has not been drawn yet, and adds light procedural squash/stretch, lean and hit blink on top. Gameplay hitboxes live on the player body and
    /// are independent of this visual. If no art exists, Build returns false and the placeholder stays.
    /// Animations still missing (so they fall back to idle): run, jump, fall, land, dash, shoot, hurt, super, victory, defeat.
    /// </summary>
    public sealed class SaiyanSpriteVisual : MonoBehaviour
    {
        public const string Character = "Saiyan", PrefKey = "saiyan.sprites";
        public bool Active { get; private set; }
        public SpriteFlipbook Flipbook { get; private set; }
        public SpriteRenderer Renderer { get; private set; }
        public bool HasArt { get; private set; }

        PlayerController m_C; PlayerHealth m_H; PlayerShooter m_S; PlayerVisual m_Placeholder; Transform m_Root;
        float m_Squash; float m_Pulse;

        /// <summary>Returns true when sprite art exists. The saved preference decides whether it is shown (default: placeholder until the full set exists).</summary>
        public bool Build(PlayerVisual placeholder)
        {
            m_Placeholder = placeholder; m_C = GetComponent<PlayerController>(); m_H = GetComponent<PlayerHealth>(); m_S = GetComponent<PlayerShooter>();
            HasArt = SpriteLibrary.HasCharacter(Character) && SpriteLibrary.TryGet(Character, "idle", out _);
            if (!HasArt) return false;
            m_Root = new GameObject("SpriteVisual").transform; m_Root.SetParent(transform, false);
            Renderer = m_Root.gameObject.AddComponent<SpriteRenderer>(); Renderer.sortingOrder = 40;
            Flipbook = m_Root.gameObject.AddComponent<SpriteFlipbook>(); Flipbook.Target = Renderer; Flipbook.Character = Character; Flipbook.Play("idle");
            if (m_C) { m_C.Landed += () => m_Squash = 0.2f; m_C.Jumped += () => m_Squash = -0.16f; }
            if (m_S) m_S.Fired += () => m_Pulse = 0.1f;
            if (m_H) m_H.Damaged += OnDamaged;
            SetActive(PlayerPrefs.GetInt(PrefKey, 0) == 1, false);
            return true;
        }

        public void Toggle() { SetActive(!Active, true); }

        public void SetActive(bool on, bool save = true)
        {
            if (!HasArt) on = false;
            Active = on;
            if (m_Root) m_Root.gameObject.SetActive(on);
            if (m_Placeholder) m_Placeholder.SetVisible(!on);
            if (save) { PlayerPrefs.SetInt(PrefKey, on ? 1 : 0); PlayerPrefs.Save(); }
        }

        /// <summary>The clip to play for the current state; the first one that exists, ending with idle.</summary>
        string[] WantedClips()
        {
            if (!m_C) return new[] { "idle" };
            if (m_H && m_H.Dead) return new[] { "defeat", "idle" };
            if (m_H && m_H.Blinking && m_H.Current < m_H.Max && m_Hurt > 0f) return new[] { "hurt", "idle" };
            if (m_C.Dashing) return new[] { "dash", "idle" };
            if (!m_C.Grounded) return m_C.Velocity.y > 0.5f ? new[] { "jump_rise", "jump", "idle" } : new[] { "fall", "jump", "idle" };
            if (m_Pulse > 0f) return new[] { Mathf.Abs(m_C.Velocity.x) > 0.5f ? "run_shoot" : "shoot", "shoot", "idle" };
            if (Mathf.Abs(m_C.Velocity.x) > 0.5f) return new[] { "run", "idle" };
            return new[] { "idle" };
        }

        float m_Hurt;

        void OnDestroy() { if (m_H) m_H.Damaged -= OnDamaged; }
        void OnDamaged() { m_Hurt = 0.4f; }

        void Update()
        {
            if (!Active || !m_Root || !m_C) return;
            m_Squash = Mathf.MoveTowards(m_Squash, 0f, 2.2f * Time.deltaTime); m_Pulse = Mathf.Max(0f, m_Pulse - Time.deltaTime); m_Hurt = Mathf.Max(0f, m_Hurt - Time.deltaTime);
            foreach (var c in WantedClips()) if (Flipbook.Has(c)) { Flipbook.Play(c); break; }
            var v = m_C.Velocity; bool air = !m_C.Grounded;
            float stretch = air ? Mathf.Clamp(v.y * 0.012f, -0.1f, 0.14f) : 0f;
            float sx = 1f - stretch * 0.6f + m_Squash * 0.5f, sy = 1f + stretch - m_Squash * 0.5f;
            if (m_C.Dashing && !Flipbook.Has("dash")) { sx = 1.2f; sy = 0.85f; }
            m_Root.localScale = new Vector3(m_C.Facing * sx, sy, 1f);
            // the drawn pose faces right; lean into the run only while no run animation exists
            bool needsLean = !Flipbook.Has("run") && Mathf.Abs(v.x) > 0.5f;
            m_Root.localRotation = Quaternion.Euler(0, 0, needsLean ? -Mathf.Sign(v.x) * Mathf.Min(Mathf.Abs(v.x) * 0.9f, 6f) : 0f);
            bool hide = m_H && m_H.Blinking && ((int)(Time.time * 14f) % 2 == 0);
            var col = Renderer.color; col.a = hide ? 0.3f : 1f; Renderer.color = col;
        }
    }
}

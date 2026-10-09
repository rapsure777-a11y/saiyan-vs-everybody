using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Player
{
    /// <summary>
    /// Placeholder Saiyan built from shapes (no sprite sheets exist yet): tousled brown hair, blue eyes, blue hoodie with a gold crown badge, white gloves, blue shoes, dark pants,
    /// plus a flapping blue scarf from the key art. Animated in code (run bob, jump stretch, landing squash, glove swing, blink on hit, dash puffs).
    /// Replace by swapping this component for an Animator-driven sprite rig once production art exists; gameplay does not depend on it.
    /// </summary>
    public sealed class PlayerVisual : MonoBehaviour
    {
        Transform m_Root, m_Scarf, m_GloveF, m_GloveB, m_Head, m_LegF, m_LegB;
        SpriteRenderer[] m_All; PlayerController m_C; PlayerHealth m_H; PlayerShooter m_S;
        float m_Squash, m_Phase, m_ShootPulse;

        public void Build()
        {
            m_C = GetComponent<PlayerController>(); m_H = GetComponent<PlayerHealth>(); m_S = GetComponent<PlayerShooter>();
            m_Root = new GameObject("Visual").transform; m_Root.SetParent(transform, false);
            var ink = PlaceholderArt.Ink;
            // scarf first (behind everything)
            m_Scarf = new GameObject("Scarf").transform; m_Scarf.SetParent(m_Root, false); m_Scarf.localPosition = new Vector3(-0.22f, 1.02f, 0f);
            PlaceholderArt.Part(m_Scarf, "ScarfA", PlaceholderArt.RoundRect(0.55f, 0.2f, Palette.Hoodie, 0.1f), new Vector2(-0.27f, 0f), 1);
            PlaceholderArt.Part(m_Scarf, "ScarfB", PlaceholderArt.RoundRect(0.35f, 0.16f, new Color(0.08f, 0.24f, 0.65f), 0.08f), new Vector2(-0.6f, -0.05f), 1, -12f);
            // back glove + back leg
            m_GloveB = new GameObject("GloveBack").transform; m_GloveB.SetParent(m_Root, false); m_GloveB.localPosition = new Vector3(-0.38f, 0.78f, 0f);
            PlaceholderArt.Part(m_GloveB, "g", PlaceholderArt.Circle(0.3f, Palette.Glove), Vector2.zero, 2);
            m_LegB = new GameObject("LegBack").transform; m_LegB.SetParent(m_Root, false); m_LegB.localPosition = new Vector3(-0.14f, 0.3f, 0f);
            PlaceholderArt.Part(m_LegB, "pants", PlaceholderArt.RoundRect(0.26f, 0.36f, Palette.Pants, 0.1f), new Vector2(0f, 0.04f), 3);
            PlaceholderArt.Part(m_LegB, "shoe", PlaceholderArt.RoundRect(0.4f, 0.2f, Palette.Hoodie, 0.1f), new Vector2(0.05f, -0.16f), 4);
            // torso
            PlaceholderArt.Part(m_Root, "Hoodie", PlaceholderArt.RoundRect(0.66f, 0.62f, Palette.Hoodie, 0.2f), new Vector2(0f, 0.8f), 5);
            PlaceholderArt.Part(m_Root, "Badge", PlaceholderArt.RoundRect(0.16f, 0.11f, Palette.Gold, 0.04f, 2f), new Vector2(0.12f, 0.82f), 6);
            m_LegF = new GameObject("LegFront").transform; m_LegF.SetParent(m_Root, false); m_LegF.localPosition = new Vector3(0.16f, 0.3f, 0f);
            PlaceholderArt.Part(m_LegF, "pants", PlaceholderArt.RoundRect(0.26f, 0.36f, Palette.Pants, 0.1f), new Vector2(0f, 0.04f), 6);
            PlaceholderArt.Part(m_LegF, "shoe", PlaceholderArt.RoundRect(0.42f, 0.22f, Palette.Hoodie, 0.1f), new Vector2(0.06f, -0.16f), 7);
            PlaceholderArt.Part(m_LegF, "sole", PlaceholderArt.RoundRect(0.42f, 0.07f, Color.white, 0.03f, 1.5f), new Vector2(0.06f, -0.25f), 7);
            // head
            m_Head = new GameObject("Head").transform; m_Head.SetParent(m_Root, false); m_Head.localPosition = new Vector3(0.02f, 1.3f, 0f);
            PlaceholderArt.Part(m_Head, "face", PlaceholderArt.Circle(0.66f, Palette.Skin), Vector2.zero, 8);
            PlaceholderArt.Part(m_Head, "eyeF", PlaceholderArt.Ellipse(0.16f, 0.2f, Color.white, 2f), new Vector2(0.17f, 0.02f), 9);
            PlaceholderArt.Part(m_Head, "irisF", PlaceholderArt.Circle(0.1f, new Color(0.2f, 0.5f, 0.95f), 1.5f), new Vector2(0.2f, 0.02f), 10);
            PlaceholderArt.Part(m_Head, "eyeB", PlaceholderArt.Ellipse(0.14f, 0.18f, Color.white, 2f), new Vector2(-0.05f, 0.02f), 9);
            PlaceholderArt.Part(m_Head, "irisB", PlaceholderArt.Circle(0.09f, new Color(0.2f, 0.5f, 0.95f), 1.5f), new Vector2(-0.02f, 0.02f), 10);
            PlaceholderArt.Part(m_Head, "smile", PlaceholderArt.RoundRect(0.14f, 0.045f, ink, 0.02f, 0.5f), new Vector2(0.14f, -0.15f), 10, -8f);
            // tousled hair
            PlaceholderArt.Part(m_Head, "hairTop", PlaceholderArt.Ellipse(0.74f, 0.42f, Palette.Hair), new Vector2(-0.02f, 0.25f), 11);
            PlaceholderArt.Part(m_Head, "hairBack", PlaceholderArt.Circle(0.4f, Palette.Hair), new Vector2(-0.26f, 0.08f), 7);
            PlaceholderArt.Part(m_Head, "tuft1", PlaceholderArt.Ellipse(0.22f, 0.3f, Palette.Hair, 2.5f), new Vector2(0.12f, 0.5f), 12, -25f);
            PlaceholderArt.Part(m_Head, "tuft2", PlaceholderArt.Ellipse(0.2f, 0.28f, Palette.Hair, 2.5f), new Vector2(-0.14f, 0.5f), 12, 22f);
            PlaceholderArt.Part(m_Head, "fringe", PlaceholderArt.Ellipse(0.3f, 0.2f, Palette.Hair, 2.5f), new Vector2(0.2f, 0.2f), 12, -15f);
            // front glove
            m_GloveF = new GameObject("GloveFront").transform; m_GloveF.SetParent(m_Root, false); m_GloveF.localPosition = new Vector3(0.4f, 0.78f, 0f);
            PlaceholderArt.Part(m_GloveF, "g", PlaceholderArt.Circle(0.34f, Palette.Glove), Vector2.zero, 13);
            m_All = GetComponentsInChildren<SpriteRenderer>(true);
            if (m_C) { m_C.Landed += () => m_Squash = 0.28f; m_C.Jumped += () => m_Squash = -0.22f; m_C.DashStarted += OnDash; }
            if (m_S) m_S.Fired += () => m_ShootPulse = 0.12f;
        }

        void OnDash() { AudioHooks.Play(Cue.Dash); Fx.Puff(transform.position + new Vector3(-0.3f * m_C.Facing, 0.25f, 0), 0.7f, null, 0.4f, new Vector2(-m_C.Facing * 1.2f, 0.4f)); }

        void Update()
        {
            if (!m_Root || !m_C) return;
            var v = m_C.Velocity; bool air = !m_C.Grounded;
            m_Squash = Mathf.MoveTowards(m_Squash, 0f, 2.2f * Time.deltaTime);
            m_ShootPulse = Mathf.Max(0f, m_ShootPulse - Time.deltaTime);
            float stretch = air ? Mathf.Clamp(v.y * 0.012f, -0.1f, 0.14f) : 0f;
            float sx = 1f - stretch * 0.6f + m_Squash * 0.5f, sy = 1f + stretch - m_Squash * 0.5f;
            if (m_C.Dashing) { sx = 1.25f; sy = 0.8f; }
            m_Root.localScale = new Vector3(m_C.Facing * sx, sy, 1f);
            float speed = Mathf.Abs(v.x);
            bool running = m_C.Grounded && speed > 0.5f;
            m_Phase += (running ? speed * 2.1f : 0f) * Time.deltaTime;
            float bob = running ? Mathf.Abs(Mathf.Sin(m_Phase * 1.0f)) * 0.07f : 0f;
            m_Root.localPosition = new Vector3(0f, bob, 0f);
            float swing = running ? Mathf.Sin(m_Phase) : (air ? 0.6f : 0f);
            m_LegF.localRotation = Quaternion.Euler(0, 0, swing * 28f); m_LegB.localRotation = Quaternion.Euler(0, 0, -swing * 28f);
            m_GloveB.localPosition = new Vector3(-0.38f - swing * 0.08f, 0.78f + (air ? 0.15f : 0f), 0f);
            m_GloveF.localPosition = new Vector3(0.4f + swing * 0.08f + (m_ShootPulse > 0 ? 0.22f : 0f), 0.78f + (air ? 0.15f : 0f) + (m_ShootPulse > 0 ? 0.05f : 0f), 0f);
            float lean = Mathf.Clamp(-v.x * 1.2f, -8f, 8f) * m_C.Facing * -1f;
            m_Head.localRotation = Quaternion.Euler(0, 0, lean * 0.6f);
            float flap = Mathf.Sin(Time.time * 18f) * (4f + speed * 1.2f);
            m_Scarf.localRotation = Quaternion.Euler(0, 0, 8f + flap + Mathf.Clamp(v.y * -1.2f, -14f, 14f));
            // blink while invulnerable after a hit
            bool hide = m_H && m_H.Blinking && ((int)(Time.time * 14f) % 2 == 0);
            foreach (var sr in m_All) if (sr) { var c = sr.color; c.a = hide ? 0.25f : 1f; sr.color = c; }
        }
    }
}

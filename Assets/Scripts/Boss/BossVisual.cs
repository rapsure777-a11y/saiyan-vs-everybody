using System.Collections;
using System.Collections.Generic;
using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Boss
{
    /// <summary>
    /// Placeholder King Cakezilla built from shapes after the concept art: three pink-frosted cake tiers, angry brows, red nose, big teeth, two giant white gloves and a candle crown.
    /// The root sits at floor level; local -X points toward the player. Gameplay only uses the public methods, so real art can replace this.
    /// </summary>
    public sealed class BossVisual : MonoBehaviour
    {
        Transform m_Body, m_Crown, m_PupilL, m_PupilR, m_Mouth; Transform[] m_Hand = new Transform[2]; Vector2[] m_HandHomeLocal = { new Vector2(-4.2f, 4.7f), new Vector2(-3.7f, 2.7f) };
        SpriteRenderer[] m_Body_Srs, m_HandSrs0, m_HandSrs1; SpriteRenderer m_MouthSr; GameObject m_Aura;
        float m_Flash, m_Age; int m_TeleIdx = -1; int m_Phase = 1; Vector2[] m_HandPunch = new Vector2[2]; Transform m_Player; Color m_PhaseTint = Color.white;
        readonly List<SpriteRenderer> m_Tinted = new List<SpriteRenderer>();

        public Vector2 HandHome(int i) => (Vector2)transform.position + m_HandHomeLocal[i];
        public Vector2 HandPosition(int i) => m_Hand[i].position;
        public Vector2 MouthPosition() => m_Mouth ? (Vector2)m_Mouth.position : (Vector2)transform.position + new Vector2(-2f, 5f);
        public void MoveHand(int i, Vector2 worldPos) { m_Hand[i].position = worldPos; }
        public void LookAt(Transform t) { m_Player = t; }

        public void Build()
        {
            var ink = PlaceholderArt.Ink;
            m_Body = new GameObject("Body").transform; m_Body.SetParent(transform, false);
            T(0, "tierBottom", PlaceholderArt.RoundRect(6.6f, 2.9f, Palette.Cream, 0.35f, 4f), new Vector2(0f, 1.45f), 3);
            T(0, "sponge1", PlaceholderArt.RoundRect(6.2f, 0.5f, Palette.Sponge, 0.2f, 0f), new Vector2(0f, 0.6f), 4);
            T(0, "frost1", PlaceholderArt.RoundRect(7.0f, 0.75f, Palette.Frosting, 0.35f, 4f), new Vector2(0f, 2.9f), 5);
            T(0, "tierMid", PlaceholderArt.RoundRect(5.8f, 2.7f, Palette.Cream, 0.35f, 4f), new Vector2(0f, 4.2f), 6);
            T(0, "sponge2", PlaceholderArt.RoundRect(5.4f, 0.45f, Palette.Sponge, 0.2f, 0f), new Vector2(0f, 3.4f), 7);
            T(0, "frost2", PlaceholderArt.RoundRect(6.2f, 0.75f, Palette.Frosting, 0.35f, 4f), new Vector2(0f, 5.5f), 8);
            T(0, "tierTop", PlaceholderArt.RoundRect(5.0f, 2.5f, Palette.Cream, 0.35f, 4f), new Vector2(0f, 6.75f), 9);
            T(0, "frost3", PlaceholderArt.RoundRect(5.4f, 0.8f, Palette.Frosting, 0.35f, 4f), new Vector2(0f, 8.0f), 10);
            // frosting drips down the front (toward the player, -X)
            float[] dx = { -2.6f, -1.6f, -0.5f, 0.7f, 1.8f, 2.6f };
            for (int i = 0; i < dx.Length; i++)
            {
                T(0, "drip" + i, PlaceholderArt.Ellipse(0.5f, 0.9f + 0.3f * (i % 3), Palette.Frosting, 3f), new Vector2(dx[i], 2.45f - 0.12f * (i % 3)), 5);
                T(0, "drip2_" + i, PlaceholderArt.Ellipse(0.45f, 0.8f + 0.25f * ((i + 1) % 3), Palette.Frosting, 3f), new Vector2(dx[i] * 0.9f + 0.2f, 5.15f - 0.1f * (i % 2)), 8);
            }
            // sprinkles
            Color[] sp = { new Color(1f, 0.85f, 0.2f), new Color(0.3f, 0.7f, 1f), new Color(0.4f, 0.9f, 0.5f), Color.white, new Color(1f, 0.3f, 0.4f) };
            var rng = new System.Random(7);
            for (int i = 0; i < 26; i++)
            {
                float x = (float)(rng.NextDouble() * 5f - 2.5f), y = i < 10 ? 2.9f + (float)rng.NextDouble() * 0.2f : i < 18 ? 5.5f + (float)rng.NextDouble() * 0.2f : 8.0f + (float)rng.NextDouble() * 0.2f;
                T(0, "sprinkle" + i, PlaceholderArt.RoundRect(0.2f, 0.07f, sp[i % sp.Length], 0.03f, 0f), new Vector2(x, y), 11, (float)rng.NextDouble() * 180f);
            }
            // face on the top tier
            T(0, "eyeL", PlaceholderArt.Ellipse(1.05f, 1.2f, Color.white, 4f), new Vector2(-1.15f, 6.9f), 12);
            T(0, "eyeR", PlaceholderArt.Ellipse(1.0f, 1.15f, Color.white, 4f), new Vector2(0.45f, 6.9f), 12);
            m_PupilL = T(0, "pupilL", PlaceholderArt.Circle(0.42f, new Color(0.25f, 0.12f, 0.08f), 3f), new Vector2(-1.2f, 6.9f), 13).transform;
            m_PupilR = T(0, "pupilR", PlaceholderArt.Circle(0.4f, new Color(0.25f, 0.12f, 0.08f), 3f), new Vector2(0.4f, 6.9f), 13).transform;
            T(0, "browL", PlaceholderArt.RoundRect(1.4f, 0.33f, Palette.Chocolate, 0.12f, 3f), new Vector2(-1.25f, 7.6f), 14, -24f);
            T(0, "browR", PlaceholderArt.RoundRect(1.4f, 0.33f, Palette.Chocolate, 0.12f, 3f), new Vector2(0.55f, 7.6f), 14, 24f);
            T(0, "nose", PlaceholderArt.Circle(0.85f, new Color(0.95f, 0.1f, 0.12f), 4f), new Vector2(-0.3f, 6.25f), 14);
            T(0, "noseShine", PlaceholderArt.Circle(0.2f, new Color(1f, 0.8f, 0.8f), 0f), new Vector2(-0.5f, 6.45f), 15);
            m_MouthSr = T(0, "mouth", PlaceholderArt.Ellipse(3.3f, 1.35f, new Color(0.55f, 0.08f, 0.15f), 4f), new Vector2(-0.3f, 5.0f), 12);
            m_Mouth = m_MouthSr.transform;
            for (int i = 0; i < 6; i++) T(0, "tooth" + i, PlaceholderArt.RoundRect(0.42f, 0.5f, Color.white, 0.1f, 3f), new Vector2(-1.5f + i * 0.58f, 5.28f), 13);
            for (int i = 0; i < 5; i++) T(0, "toothB" + i, PlaceholderArt.RoundRect(0.4f, 0.4f, Color.white, 0.1f, 3f), new Vector2(-1.25f + i * 0.58f, 4.75f), 13);
            // crown and candles
            m_Crown = new GameObject("Crown").transform; m_Crown.SetParent(m_Body, false); m_Crown.localPosition = new Vector3(0f, 8.5f, 0f);
            for (int i = -1; i <= 1; i++)
            {
                var c = PlaceholderArt.Part(m_Crown, "candle" + i, PlaceholderArt.RoundRect(0.28f, 1.0f, i == 0 ? new Color(0.5f, 0.75f, 1f) : Color.white, 0.1f, 3f), new Vector2(i * 1.05f, 1.1f), 14);
                var f = PlaceholderArt.Part(m_Crown, "flame" + i, PlaceholderArt.Ellipse(0.34f, 0.52f, new Color(1f, 0.8f, 0.2f), 2f), new Vector2(i * 1.05f, 1.85f), 15);
                m_Tinted.Add(c); m_Tinted.Add(f);
            }
            m_Tinted.Add(PlaceholderArt.Part(m_Crown, "crownBase", PlaceholderArt.RoundRect(3.6f, 0.9f, Palette.Gold, 0.15f, 4f), new Vector2(0f, 0.25f), 16));
            for (int i = -1; i <= 1; i++) PlaceholderArt.Part(m_Crown, "gem" + i, PlaceholderArt.Circle(0.38f, i == 0 ? new Color(0.2f, 0.8f, 0.4f) : new Color(0.95f, 0.2f, 0.3f), 3f), new Vector2(i * 1.1f, 0.25f), 17);
            for (int i = -1; i <= 1; i += 2) PlaceholderArt.Part(m_Crown, "spike" + i, PlaceholderArt.Star(0.7f, Palette.Gold, 3, 0.4f, 3f), new Vector2(i * 1.7f, 0.85f), 16);
            // gloves (two giant white hands, floating in front of the cake)
            for (int h = 0; h < 2; h++)
            {
                var hand = new GameObject(h == 0 ? "HandThrow" : "HandSlam").transform; hand.SetParent(transform, false); hand.localPosition = m_HandHomeLocal[h];
                m_Hand[h] = hand;
                var list = new List<SpriteRenderer>();
                list.Add(PlaceholderArt.Part(hand, "palm", PlaceholderArt.Circle(2.0f, Palette.Glove, 4f), Vector2.zero, 20));
                float[] fx = { -0.75f, -0.25f, 0.25f, 0.75f }; float[] fy = { 0.8f, 1.0f, 1.0f, 0.8f };
                for (int i = 0; i < 4; i++) list.Add(PlaceholderArt.Part(hand, "finger" + i, PlaceholderArt.Circle(0.78f, Palette.Glove, 4f), new Vector2(fx[i], fy[i]), 21));
                list.Add(PlaceholderArt.Part(hand, "thumb", PlaceholderArt.Circle(0.8f, Palette.Glove, 4f), new Vector2(-1.0f, -0.15f), 21));
                list.Add(PlaceholderArt.Part(hand, "cuff", PlaceholderArt.RoundRect(1.6f, 0.5f, Palette.Frosting, 0.2f, 3f), new Vector2(0.9f, -0.9f), 19, -20f));
                if (h == 0) m_HandSrs0 = list.ToArray(); else m_HandSrs1 = list.ToArray();
            }
            m_Body_Srs = m_Body.GetComponentsInChildren<SpriteRenderer>(true);
            var hurt = new GameObject("Hurtbox"); hurt.transform.SetParent(transform, false);
            var box = hurt.AddComponent<BoxCollider2D>(); box.isTrigger = true; box.size = new Vector2(6.0f, 8.6f); box.offset = new Vector2(0f, 4.3f);
            var rb = hurt.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic;
            var hb = hurt.AddComponent<BossHurtbox>(); hb.Visual = this; Hurtbox = hb;
        }

        public BossHurtbox Hurtbox { get; private set; }

        SpriteRenderer T(int unused, string name, Sprite s, Vector2 pos, int order, float rot = 0f)
        {
            var sr = PlaceholderArt.Part(m_Body, name, s, pos, order + 0, rot);
            return sr;
        }

        public void Telegraph(bool on, int idx) { m_TeleIdx = on ? idx : -1; if (!on) ResetColours(); }
        public void Flash() { m_Flash = 0.12f; }
        public void Throw(int hand) { StartCoroutine(Punch(hand, new Vector2(-1.0f, 0.4f))); }
        public void Spit() { StartCoroutine(MouthBulge()); }

        IEnumerator Punch(int h, Vector2 dir)
        {
            float t = 0; while (t < 0.15f) { t += Time.deltaTime; m_HandPunch[h] = dir * Mathf.Sin(t / 0.15f * Mathf.PI); yield return null; }
            m_HandPunch[h] = Vector2.zero;
        }
        IEnumerator MouthBulge()
        {
            float t = 0; while (t < 0.3f) { t += Time.deltaTime; m_Mouth.localScale = Vector3.one * (1f + 0.35f * Mathf.Sin(t / 0.3f * Mathf.PI)); yield return null; }
            m_Mouth.localScale = Vector3.one;
        }

        void ResetColours()
        {
            foreach (var s in m_HandSrs0) if (s) s.color = Color.white;
            foreach (var s in m_HandSrs1) if (s) s.color = Color.white;
            if (m_MouthSr) m_MouthSr.color = Color.white;
        }

        public void ResetPose() { m_TeleIdx = -1; ResetColours(); for (int i = 0; i < 2; i++) { m_Hand[i].position = HandHome(i); m_HandPunch[i] = Vector2.zero; } if (m_Mouth) m_Mouth.localScale = Vector3.one; }

        public void SetPhase(int phase)
        {
            m_Phase = phase;
            m_PhaseTint = phase == 1 ? Color.white : phase == 2 ? new Color(1f, 0.93f, 0.88f) : new Color(1f, 0.88f, 0.55f);
            if (phase >= 3 && !m_Aura)
            {
                m_Aura = new GameObject("SupremeAura"); m_Aura.transform.SetParent(transform, false); m_Aura.transform.localPosition = new Vector3(0f, 4.6f, 0f);
                var sr = m_Aura.AddComponent<SpriteRenderer>(); sr.sprite = PlaceholderArt.Circle(10f, new Color(1f, 0.85f, 0.2f, 0.28f), 0f); sr.sortingOrder = 1;
                m_Crown.localScale = Vector3.one * 1.25f;
            }
            foreach (var s in m_Body_Srs) if (s && s.sortingOrder < 20) s.color = m_PhaseTint;
        }

        /// <summary>Slides in from off-screen. Returns when done or when <paramref name="skip"/> says so.</summary>
        public IEnumerator SlideIn(float seconds, System.Func<bool> skip)
        {
            var final = transform.position; var start = final + new Vector3(10f, 0f, 0f);
            float t = 0f;
            while (t < seconds && !(skip != null && skip())) { t += Time.deltaTime; transform.position = Vector3.Lerp(start, final, Mathf.SmoothStep(0, 1, t / seconds)); if (Mathf.Repeat(t, 0.5f) < 0.03f) CameraShake.Shake(0.12f, 0.15f); yield return null; }
            transform.position = final;
        }

        public IEnumerator DefeatSequence(float seconds)
        {
            ResetPose();
            float t = 0f; var start = transform.position;
            while (t < seconds)
            {
                t += Time.deltaTime; float k = t / seconds;
                transform.position = start + new Vector3(Mathf.Sin(t * 60f) * 0.1f * (1f - k), -k * 1.2f, 0f);
                if (Random.value < 0.25f) Fx.Burst(start + new Vector3(Random.Range(-3f, 3f), Random.Range(1f, 8f), 0), Random.value < 0.5f ? Palette.Frosting : Palette.Gold, 5, 5f, 0.3f, 0.7f);
                foreach (var s in m_Body_Srs) if (s) { var c = s.color; c.a = Mathf.Clamp01(1.4f - k * 1.4f); s.color = c; }
                foreach (var s in m_HandSrs0) if (s) { var c = s.color; c.a = Mathf.Clamp01(1.4f - k * 1.4f); s.color = c; }
                foreach (var s in m_HandSrs1) if (s) { var c = s.color; c.a = Mathf.Clamp01(1.4f - k * 1.4f); s.color = c; }
                yield return null;
            }
        }

        void Update()
        {
            m_Age += Time.deltaTime;
            if (m_Body) m_Body.localScale = new Vector3(1f + 0.01f * Mathf.Sin(m_Age * 2.1f), 1f + 0.018f * Mathf.Sin(m_Age * 2.1f + 1f), 1f);
            // pupils follow the player
            if (m_Player && m_PupilL)
            {
                float dx = Mathf.Clamp((m_Player.position.x - transform.position.x) * 0.03f, -0.22f, 0.22f), dy = Mathf.Clamp((m_Player.position.y + 1f - (transform.position.y + 6.9f)) * 0.05f, -0.2f, 0.1f);
                m_PupilL.localPosition = new Vector3(-1.15f + dx - 0.05f, 6.9f + dy, 0f); m_PupilR.localPosition = new Vector3(0.45f + dx - 0.05f, 6.9f + dy, 0f);
            }
            if (m_Aura) { float s = 1f + 0.06f * Mathf.Sin(m_Age * 5f); m_Aura.transform.localScale = new Vector3(s, s, 1f); }
            // telegraph pulse: yellow then red on the glove (or the mouth for the spit)
            if (m_TeleIdx >= 0)
            {
                bool bright = Mathf.Sin(m_Age * 22f) > 0f;
                var col = bright ? new Color(1f, 0.95f, 0.3f) : new Color(1f, 0.35f, 0.3f);
                SpriteRenderer[] arr = m_TeleIdx == 0 ? m_HandSrs0 : m_TeleIdx == 1 ? m_HandSrs1 : null;
                if (arr != null) { foreach (var s in arr) { if (s) s.color = col; } }
                else if (m_MouthSr) m_MouthSr.color = col;
            }
            // hit flash
            if (m_Flash > 0f)
            {
                m_Flash -= Time.deltaTime; var f = new Color(1f, 0.7f, 0.7f);
                foreach (var s in m_Body_Srs) if (s && s.sortingOrder < 20) s.color = m_Flash > 0f ? f : m_PhaseTint;
            }
            // hands: idle bob + punch offset (only when no attack is moving them: attacks set position directly each frame, so the bob is added to home only)
            for (int i = 0; i < 2; i++)
            {
                if (m_HandPunch[i] != Vector2.zero) m_Hand[i].position = (Vector2)m_Hand[i].position + m_HandPunch[i] * Time.deltaTime * 6f;
            }
        }

        void LateUpdate()
        {
            if (m_TeleIdx < 0 && m_Hand[0] && m_Hand[1])
            {
                // gentle bob while idle (attacks that move a hand overwrite this each frame)
                for (int i = 0; i < 2; i++)
                {
                    var home = HandHome(i);
                    if (((Vector2)m_Hand[i].position - home).sqrMagnitude < 0.04f) m_Hand[i].position = home + new Vector2(0f, Mathf.Sin(m_Age * 2f + i) * 0.12f);
                }
            }
        }
    }
}

using System.Collections;
using UnityEngine;

namespace Saiyan.Core
{
    /// <summary>Small procedural effects: puffs, crumbs, camera shake. All placeholder-quality but readable.</summary>
    public static class Fx
    {
        public static void Puff(Vector2 pos, float size = 0.5f, Color? color = null, float life = 0.35f, Vector2? drift = null)
        {
            var go = new GameObject("Puff"); go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = PlaceholderArt.Circle(size, color ?? new Color(1f, 1f, 1f, 0.9f), 2f); sr.sortingOrder = 40;
            go.AddComponent<FxLife>().Begin(life, drift ?? new Vector2(0f, 0.6f), 1.8f);
        }

        public static void Burst(Vector2 pos, Color color, int count = 8, float speed = 4f, float size = 0.22f, float life = 0.5f)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Crumb"); go.transform.position = pos;
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = PlaceholderArt.Circle(size, color, 2f); sr.sortingOrder = 41;
                float a = (i / (float)count) * Mathf.PI * 2f + Random.value * 0.4f;
                go.AddComponent<FxLife>().Begin(life, new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.8f + 0.5f) * speed * (0.5f + Random.value * 0.7f), 0.2f, true);
            }
        }
    }

    public sealed class FxLife : MonoBehaviour
    {
        float m_Life, m_Age, m_End; Vector2 m_Vel; SpriteRenderer m_Sr; bool m_Gravity; Vector3 m_Scale0;
        public void Begin(float life, Vector2 vel, float endScale, bool gravity = false)
        { m_Life = life; m_Vel = vel; m_End = endScale; m_Gravity = gravity; m_Sr = GetComponent<SpriteRenderer>(); m_Scale0 = transform.localScale; }
        void Update()
        {
            m_Age += Time.deltaTime; float t = m_Age / m_Life;
            if (m_Gravity) m_Vel += Vector2.down * 14f * Time.deltaTime;
            transform.position += (Vector3)(m_Vel * Time.deltaTime);
            transform.localScale = m_Scale0 * Mathf.Lerp(1f, m_End, t);
            if (m_Sr) { var c = m_Sr.color; c.a = 1f - t; m_Sr.color = c; }
            if (t >= 1f) Destroy(gameObject);
        }
    }

    /// <summary>Camera shake that respects the Reduced Shake accessibility option.</summary>
    public sealed class CameraShake : MonoBehaviour
    {
        float m_Time, m_Strength; Vector3 m_Base; bool m_Active;
        public static CameraShake Instance { get; private set; }
        void Awake() { Instance = this; }
        public static void Shake(float strength, float duration)
        {
            if (!Instance) return;
            if (GameSettings.ReducedShake) strength *= 0.15f;
            if (strength > Instance.m_Strength || Instance.m_Time <= 0f) { Instance.m_Strength = strength; }
            Instance.m_Time = Mathf.Max(Instance.m_Time, duration);
        }
        /// <summary>Called by the camera rig after it positions the camera; returns the offset to add.</summary>
        public Vector3 Offset()
        {
            if (m_Time <= 0f) { m_Strength = 0f; return Vector3.zero; }
            m_Time -= Time.deltaTime;
            return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * m_Strength * Mathf.Clamp01(m_Time * 3f);
        }
    }
}

using Saiyan.Core;
using Saiyan.Player;
using UnityEngine;

namespace Saiyan.Level
{
    /// <summary>A wooden sign with text, used for the tutorial.</summary>
    public static class SignFactory
    {
        public static GameObject Create(Transform parent, float x, string text, float width = 3.6f)
        {
            var go = new GameObject("Sign"); go.transform.SetParent(parent, false); go.transform.position = new Vector3(x, LevelLayout.FloorY, 0f);
            int lines = text.Split((char)10).Length; float boardH = 0.55f + 0.42f * lines, boardY = 1.5f + boardH * 0.5f;
            PlaceholderArt.Part(go.transform, "post", PlaceholderArt.RoundRect(0.25f, boardY, Palette.Chocolate, 0.08f), new Vector2(0f, boardY * 0.5f), 6);
            PlaceholderArt.Part(go.transform, "board", PlaceholderArt.RoundRect(width, boardH, new Color(1f, 0.93f, 0.75f), 0.2f, 4f), new Vector2(0f, boardY), 7);
            var t = new GameObject("text"); t.transform.SetParent(go.transform, false); t.transform.localPosition = new Vector3(0f, boardY, 0f);
            var tm = t.AddComponent<TextMesh>();
            tm.text = text; tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.fontSize = 64; tm.characterSize = 0.06f; tm.color = new Color(0.25f, 0.12f, 0.1f);
            tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var mr = t.GetComponent<MeshRenderer>(); mr.sharedMaterial = tm.font.material; mr.sortingOrder = 8;
            return go;
        }
    }

    /// <summary>A shootable gumdrop target in the tutorial. Two hits pop it and charge the Super meter.</summary>
    public sealed class TargetDummy : MonoBehaviour, IShootable
    {
        public SuperMeter Meter; public int Health = 2; public float Reward = 20f;
        float m_Wobble;
        public static TargetDummy Create(Transform parent, Vector2 pos, SuperMeter meter)
        {
            var go = new GameObject("TargetDummy"); go.transform.SetParent(parent, false); go.transform.position = pos;
            var d = go.AddComponent<TargetDummy>(); d.Meter = meter;
            var col = go.AddComponent<CircleCollider2D>(); col.isTrigger = true; col.radius = 0.6f;
            var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic;
            PlaceholderArt.Part(go.transform, "body", PlaceholderArt.Circle(1.2f, new Color(0.55f, 0.85f, 0.5f)), Vector2.zero, 10);
            PlaceholderArt.Part(go.transform, "shine", PlaceholderArt.Circle(0.3f, new Color(1f, 1f, 1f, 0.8f), 0f), new Vector2(-0.25f, 0.25f), 11);
            PlaceholderArt.Part(go.transform, "eyeL", PlaceholderArt.Circle(0.2f, Color.white, 2f), new Vector2(-0.18f, 0.05f), 11);
            PlaceholderArt.Part(go.transform, "eyeR", PlaceholderArt.Circle(0.2f, Color.white, 2f), new Vector2(0.18f, 0.05f), 11);
            PlaceholderArt.Part(go.transform, "pupL", PlaceholderArt.Circle(0.08f, PlaceholderArt.Ink, 0f), new Vector2(-0.14f, 0.05f), 12);
            PlaceholderArt.Part(go.transform, "pupR", PlaceholderArt.Circle(0.08f, PlaceholderArt.Ink, 0f), new Vector2(0.22f, 0.05f), 12);
            return d;
        }
        void Update() { m_Wobble = Mathf.MoveTowards(m_Wobble, 0f, 4f * Time.deltaTime); transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 30f) * 12f * m_Wobble); }
        public bool OnStarHit(int damage, Vector2 point)
        {
            Health -= damage; m_Wobble = 1f; AudioHooks.Play(Cue.BossHit, 0.5f);
            if (Health <= 0)
            {
                AudioHooks.Play(Cue.TargetPop); Fx.Burst(transform.position, new Color(0.55f, 0.85f, 0.5f), 10, 5f, 0.25f, 0.6f);
                if (Meter) Meter.Add(Reward);
                Destroy(gameObject);
            }
            return true;
        }
    }

    /// <summary>A star pickup: charges the Super meter.</summary>
    public sealed class StarPickup : MonoBehaviour
    {
        public float Reward = 20f; float m_Base;
        public static StarPickup Create(Transform parent, Vector2 pos)
        {
            var go = new GameObject("StarPickup"); go.transform.SetParent(parent, false); go.transform.position = pos;
            var p = go.AddComponent<StarPickup>(); p.m_Base = pos.y;
            var col = go.AddComponent<CircleCollider2D>(); col.isTrigger = true; col.radius = 0.5f;
            var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic;
            PlaceholderArt.Part(go.transform, "star", PlaceholderArt.Star(0.9f, new Color(1f, 0.9f, 0.3f)), Vector2.zero, 10);
            PlaceholderArt.Part(go.transform, "eyeL", PlaceholderArt.Circle(0.1f, PlaceholderArt.Ink, 0f), new Vector2(-0.1f, 0.02f), 11);
            PlaceholderArt.Part(go.transform, "eyeR", PlaceholderArt.Circle(0.1f, PlaceholderArt.Ink, 0f), new Vector2(0.1f, 0.02f), 11);
            return p;
        }
        void Update() { transform.position = new Vector3(transform.position.x, m_Base + Mathf.Sin(Time.time * 3f + transform.position.x) * 0.12f, 0f); transform.localScale = Vector3.one * (1f + 0.05f * Mathf.Sin(Time.time * 6f)); }
        void OnTriggerEnter2D(Collider2D other)
        {
            var m = other.GetComponentInParent<SuperMeter>();
            if (!m) return;
            m.Add(Reward); AudioHooks.Play(Cue.Pickup); Fx.Burst(transform.position, Palette.Gold, 8, 4f, 0.2f, 0.4f); Destroy(gameObject);
        }
    }

    /// <summary>Marks the checkpoint right before the boss arena. Crossing it makes a retry start in front of the boss.</summary>
    public sealed class CheckpointFlag : MonoBehaviour
    {
        public static bool Reached;
        bool m_Raised; Transform m_Cloth;
        public static CheckpointFlag Create(Transform parent, float x)
        {
            var go = new GameObject("Checkpoint"); go.transform.SetParent(parent, false); go.transform.position = new Vector3(x, LevelLayout.FloorY, 0f);
            var f = go.AddComponent<CheckpointFlag>();
            PlaceholderArt.Part(go.transform, "pole", PlaceholderArt.RoundRect(0.16f, 3.2f, Color.white, 0.06f, 3f), new Vector2(0f, 1.6f), 6);
            f.m_Cloth = PlaceholderArt.Part(go.transform, "flag", PlaceholderArt.RoundRect(1.2f, 0.8f, new Color(0.6f, 0.6f, 0.65f), 0.1f), new Vector2(0.6f, 0.7f), 7).transform;
            var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = new Vector2(1.0f, 6f); col.offset = new Vector2(0f, 3f);
            var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic;
            return f;
        }
        void Update() { if (m_Raised) m_Cloth.localPosition = Vector3.Lerp(m_Cloth.localPosition, new Vector3(0.6f, 2.7f, 0f), 4f * Time.deltaTime); }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (m_Raised || !other.GetComponentInParent<PlayerHealth>()) return;
            m_Raised = true; Reached = true; Core.GameSession.StartAtCheckpoint = true;
            m_Cloth.GetComponent<SpriteRenderer>().sprite = PlaceholderArt.RoundRect(1.2f, 0.8f, Palette.Safe, 0.1f);
            AudioHooks.Play(Cue.Pickup);
        }
    }
}

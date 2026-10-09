using System;
using System.Collections.Generic;
using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Player
{
    /// <summary>One of Saiyan's stars. Flies straight, hits anything <see cref="IShootable"/>, vanishes on solid scenery or after its lifetime. Pooled and capped.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class StarShot : MonoBehaviour
    {
        /// <summary>Raised when a star hurt something: (damage dealt).</summary>
        public static event Action<int> Landed;

        public int Damage = 1; public bool Pierce; public float Speed = 18f, Life = 1.4f;
        Vector2 m_Dir; float m_Age; Pool<StarShot> m_Pool; readonly HashSet<IShootable> m_Done = new HashSet<IShootable>(); SpriteRenderer m_Sr; Rigidbody2D m_Rb;

        public static Pool<StarShot> CreatePool(Transform parent, int max)
            => new Pool<StarShot>(() =>
            {
                var go = new GameObject("StarShot"); go.transform.SetParent(parent, false);
                var s = go.AddComponent<StarShot>(); return s;
            }, max);

        void Awake()
        {
            m_Rb = GetComponent<Rigidbody2D>(); m_Rb.bodyType = RigidbodyType2D.Kinematic; m_Rb.gravityScale = 0f; m_Rb.useFullKinematicContacts = true;
            var c = GetComponent<CircleCollider2D>(); c.isTrigger = true; c.radius = 0.22f;
            m_Sr = gameObject.AddComponent<SpriteRenderer>(); m_Sr.sprite = PlaceholderArt.Star(0.55f, Palette.Gold); m_Sr.sortingOrder = 30;
        }

        public void Fire(Pool<StarShot> pool, Vector2 position, Vector2 direction, int damage, float scale = 1f, bool pierce = false, float speed = 18f, float life = 1.4f)
        {
            m_Pool = pool; transform.position = position; m_Dir = direction.normalized; Damage = damage; Pierce = pierce; Speed = speed; Life = life; m_Age = 0f; m_Done.Clear();
            transform.localScale = Vector3.one * scale;
            m_Rb.position = position;
        }

        void Update()
        {
            m_Age += Time.deltaTime;
            transform.Rotate(0, 0, -520f * Time.deltaTime);
            m_Rb.MovePosition(m_Rb.position + m_Dir * Speed * Time.deltaTime);
            if (m_Age >= Life) Despawn();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerHealth>() || other.GetComponent<Saiyan.Boss.ShotPassThrough>()) return;
            var target = other.GetComponentInParent<IShootable>();
            if (target != null)
            {
                if (m_Done.Contains(target)) return;
                m_Done.Add(target);
                if (target.OnStarHit(Damage, transform.position)) { Landed?.Invoke(Damage); if (!Pierce) Despawn(); }
                return;
            }
            if (!other.isTrigger) Despawn();                                // solid scenery
        }

        void Despawn() { if (m_Pool != null) m_Pool.Release(this); else gameObject.SetActive(false); }
    }
}

using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Player
{
    /// <summary>Fast repeatable star shots in the facing direction, plus the Super burst. Shot count is capped by the pool.</summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerShooter : MonoBehaviour
    {
        public float FireInterval = 0.11f, ShotSpeed = 18f, ShotLife = 1.2f; public int ShotDamage = 1, SuperDamage = 12, MaxShots = 24;
        public float SuperInvulnerability = 1.0f;
        public Vector2 MuzzleOffset = new Vector2(0.65f, 0.8f);
        public SuperMeter Meter; public PlayerHealth Health;

        public event System.Action Fired, SuperFired;
        public int ShotsFired { get; private set; }
        public int ActiveShots => m_Pool?.ActiveCount ?? 0;

        PlayerController m_Controller; Pool<StarShot> m_Pool; float m_Cooldown; Transform m_Root;

        void Awake() { m_Controller = GetComponent<PlayerController>(); }

        Pool<StarShot> EnsurePool()
        {
            if (m_Pool != null) return m_Pool;
            m_Root = new GameObject("Shots").transform;
            m_Pool = StarShot.CreatePool(m_Root, MaxShots + 4);
            return m_Pool;
        }

        void OnDestroy() { if (m_Root) Destroy(m_Root.gameObject); }

        void Update()
        {
            m_Cooldown -= Time.deltaTime;
            var i = m_Controller.LastIntent;
            if (m_Controller.InputLocked) return;
            if (i.superPressed && Meter && Meter.TryConsume()) { FireSuper(); return; }
            if (i.shootHeld && m_Cooldown <= 0f && !m_Controller.Dashing) Shoot();
        }

        void Shoot()
        {
            var s = EnsurePool().Get(); if (!s) return;
            m_Cooldown = FireInterval; ShotsFired++;
            var pos = (Vector2)transform.position + new Vector2(MuzzleOffset.x * m_Controller.Facing, MuzzleOffset.y);
            s.Fire(m_Pool, pos, new Vector2(m_Controller.Facing, 0f), ShotDamage, 1f, false, ShotSpeed, ShotLife);
            AudioHooks.Play(Cue.Shoot, 0.5f);
            Fired?.Invoke();
        }

        /// <summary>A giant piercing star (SuperDamage to the boss, once) with two helper stars. Saiyan is protected while it fires.</summary>
        public void FireSuper()
        {
            var pool = EnsurePool();
            var pos = (Vector2)transform.position + new Vector2(MuzzleOffset.x * m_Controller.Facing, MuzzleOffset.y);
            var big = pool.Get();
            if (big) big.Fire(pool, pos, new Vector2(m_Controller.Facing, 0f), SuperDamage, 3.2f, true, 15f, 1.8f);
            for (int k = -1; k <= 1; k += 2)
            {
                var h = pool.Get(); if (!h) continue;
                var d = Quaternion.Euler(0, 0, 14f * k) * new Vector2(m_Controller.Facing, 0f);
                h.Fire(pool, pos, d, 1, 1.4f, false, 16f, 1.4f);
            }
            if (Health) Health.GrantInvulnerability(SuperInvulnerability);
            AudioHooks.Play(Cue.Super);
            SuperFired?.Invoke();
        }
    }
}

using System;
using UnityEngine;

namespace Saiyan.Player
{
    /// <summary>All the numbers that define how Saiyan moves. Exposed in the inspector; defaults follow the brief's recommended tuning.</summary>
    [Serializable]
    public sealed class PlayerTuning
    {
        [Header("Run")] public float runSpeed = 6f; public float groundAccel = 70f, groundDecel = 80f, airAccel = 55f;
        [Header("Jump")] public float jumpVelocity = 12f; public float gravity = 28f, fallGravityMultiplier = 1.5f, jumpCutMultiplier = 0.45f, maxFallSpeed = 20f;
        public float coyoteTime = 0.1f, jumpBuffer = 0.12f;
        [Header("Dash")] public float dashSpeed = 14f; public float dashTime = 0.26f, dashCooldown = 0.55f;
        [Tooltip("The first part of a dash cannot be hurt (seconds). 0 = no invulnerability.")] public float dashInvulnerability = 0.12f;
        [Header("Hit")] public float knockbackSpeed = 7f, knockbackTime = 0.2f;
    }

    /// <summary>Side-on movement: run, variable-height jump with coyote time and jump buffer, and a ground/air dash. Driven by an <see cref="IIntentSource"/>.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        public PlayerTuning Tuning = new PlayerTuning();
        public IIntentSource Source;
        /// <summary>True during cutscenes, death and menus: ignores all input.</summary>
        public bool InputLocked;

        public bool Grounded { get; private set; }
        public int Facing { get; private set; } = 1;
        public bool Dashing => m_DashTimer > 0f;
        public bool DashInvulnerable => m_DashTimer > 0f && (Tuning.dashTime - m_DashTimer) < Tuning.dashInvulnerability;
        public float DashCooldownLeft => m_DashCooldown;
        public Vector2 Velocity => m_Rb.linearVelocity;
        public PlayerIntent LastIntent { get; private set; }

        public event Action Jumped, Landed, DashStarted;

        Rigidbody2D m_Rb; CapsuleCollider2D m_Col;
        readonly JumpTimers m_Jump = new JumpTimers();
        float m_DashTimer, m_DashCooldown, m_KnockTimer, m_JumpLockout; int m_DashDir; Vector2 m_KnockVel; bool m_JumpHeldPrev, m_Rising;
        static readonly Collider2D[] s_Hits = new Collider2D[8];
        ContactFilter2D m_Filter;

        void Awake()
        {
            m_Rb = GetComponent<Rigidbody2D>(); m_Col = GetComponent<CapsuleCollider2D>();
            m_Rb.freezeRotation = true; m_Rb.interpolation = RigidbodyInterpolation2D.Interpolate; m_Rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            m_Rb.gravityScale = Tuning.gravity / 9.81f;
            m_Col.sharedMaterial = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
            m_Filter = new ContactFilter2D { useTriggers = false }; m_Filter.NoFilter(); m_Filter.useTriggers = false;
        }

        void Update()
        {
            var i = Source != null ? Source.Read() : default;
            if (InputLocked) i = default;
            LastIntent = i;
            float dt = Time.deltaTime;
            bool wasGrounded = Grounded;
            m_JumpLockout = Mathf.Max(0f, m_JumpLockout - dt);
            Grounded = m_JumpLockout <= 0f && CheckGround();
            if (Grounded && !wasGrounded && m_Rb.linearVelocity.y <= 0.5f) { Landed?.Invoke(); m_Rising = false; }
            m_Jump.Coyote = Tuning.coyoteTime; m_Jump.Buffer = Tuning.jumpBuffer;
            m_Jump.Tick(dt, Grounded, i.jumpPressed);
            m_DashCooldown = Mathf.Max(0f, m_DashCooldown - dt);

            var v = m_Rb.linearVelocity;
            if (i.move != 0f) Facing = i.move > 0f ? 1 : -1;

            if (m_KnockTimer > 0f)                                     // knocked back: no steering for a moment
            {
                m_KnockTimer -= dt; v.x = m_KnockVel.x; m_Rb.linearVelocity = v; m_Rb.gravityScale = Tuning.gravity / 9.81f; return;
            }

            if (i.dashPressed && m_DashCooldown <= 0f && m_DashTimer <= 0f)
            {
                m_DashDir = i.move != 0f ? (i.move > 0f ? 1 : -1) : Facing; Facing = m_DashDir;
                m_DashTimer = Tuning.dashTime; m_DashCooldown = Tuning.dashCooldown; DashStarted?.Invoke();
            }
            if (m_DashTimer > 0f)
            {
                m_DashTimer -= dt;
                m_Rb.gravityScale = 0f;
                m_Rb.linearVelocity = new Vector2(m_DashDir * Tuning.dashSpeed, 0f);
                if (m_DashTimer <= 0f) { m_Rb.gravityScale = Tuning.gravity / 9.81f; m_Rb.linearVelocity = new Vector2(m_DashDir * Tuning.runSpeed, 0f); }
                m_JumpHeldPrev = i.jumpHeld; return;
            }

            // horizontal
            float target = i.move * Tuning.runSpeed;
            float rate = Mathf.Approximately(target, 0f) ? Tuning.groundDecel : (Grounded ? Tuning.groundAccel : Tuning.airAccel);
            if (!Grounded && Mathf.Approximately(target, 0f)) rate = Tuning.airAccel * 0.5f;
            v.x = Mathf.MoveTowards(v.x, target, rate * dt);

            // jump
            if (m_Jump.TryConsume())
            {
                v.y = Tuning.jumpVelocity; m_JumpLockout = 0.1f; Grounded = false; m_Rising = true; Jumped?.Invoke();
            }
            else if (m_Rising && !i.jumpHeld && m_JumpHeldPrev && v.y > 0f)
            {
                v.y *= Tuning.jumpCutMultiplier; m_Rising = false;       // let go early: shorter hop
            }
            m_JumpHeldPrev = i.jumpHeld;
            if (v.y <= 0f) m_Rising = false;

            m_Rb.gravityScale = (v.y < 0f ? Tuning.fallGravityMultiplier : 1f) * Tuning.gravity / 9.81f;
            if (v.y < -Tuning.maxFallSpeed) v.y = -Tuning.maxFallSpeed;
            m_Rb.linearVelocity = v;
        }

        bool CheckGround()
        {
            var b = m_Col.bounds;
            var centre = new Vector2(b.center.x, b.min.y - 0.04f);
            int n = Physics2D.OverlapBox(centre, new Vector2(b.size.x * 0.85f, 0.1f), 0f, m_Filter, s_Hits);
            for (int k = 0; k < n; k++) if (s_Hits[k] && s_Hits[k] != m_Col && !s_Hits[k].isTrigger) return true;
            return false;
        }

        public void Knockback(Vector2 fromPosition)
        {
            float dir = transform.position.x >= fromPosition.x ? 1f : -1f;
            m_KnockVel = new Vector2(dir * Tuning.knockbackSpeed, 0f); m_KnockTimer = Tuning.knockbackTime; m_DashTimer = 0f;
            var v = m_Rb.linearVelocity; v.y = Mathf.Max(v.y, 5f); m_Rb.linearVelocity = v; m_JumpLockout = 0.08f;
        }

        public void Stop() { m_Rb.linearVelocity = Vector2.zero; m_DashTimer = 0f; m_KnockTimer = 0f; }
        public void Teleport(Vector2 p) { m_Rb.position = p; transform.position = p; Stop(); m_Jump.Reset(); }
        public void SetFacing(int f) { Facing = f >= 0 ? 1 : -1; }
    }
}

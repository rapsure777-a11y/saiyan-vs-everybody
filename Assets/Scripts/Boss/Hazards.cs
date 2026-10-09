using System.Collections;
using Saiyan.Core;
using Saiyan.Player;
using UnityEngine;

namespace Saiyan.Boss
{
    /// <summary>A damaging area. Starts harmless until <see cref="Armed"/> is true; hurts the player while overlapping (the player's own invulnerability rules apply).</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Hazard : MonoBehaviour, IHazard
    {
        public int DamageAmount = 1; public bool Armed;
        public int Damage => DamageAmount;
        /// <summary>Real time since the hazard became armed.</summary>
        public float ArmedFor { get; private set; }
        static PlayerHealth s_Cache;
        Collider2D m_Col; Rigidbody2D m_Rb;

        protected virtual void Awake()
        {
            m_Rb = GetComponent<Rigidbody2D>(); m_Rb.bodyType = RigidbodyType2D.Kinematic; m_Rb.useFullKinematicContacts = true;
        }

        protected virtual void Update() { if (Armed) ArmedFor += Time.deltaTime; }

        void OnTriggerStay2D(Collider2D other) { TryHit(other); }
        void OnTriggerEnter2D(Collider2D other) { TryHit(other); }

        void TryHit(Collider2D other)
        {
            if (!Armed) return;
            var ph = other.GetComponentInParent<PlayerHealth>();
            if (!ph) return;
            ph.TakeHit(DamageAmount, transform.position);
        }

        public static BoxCollider2D AddBox(GameObject go, Vector2 size, Vector2 offset)
        { var c = go.AddComponent<BoxCollider2D>(); c.isTrigger = true; c.size = size; c.offset = offset; return c; }
    }

    /// <summary>Flashing warning shape on the floor (or anywhere). Pure visuals: it never hurts. Flashes yellow and red, faster as the attack nears.</summary>
    public sealed class TelegraphMarker : MonoBehaviour
    {
        SpriteRenderer m_Sr; float m_Age, m_Duration; Vector3 m_BaseScale;
        public static TelegraphMarker Create(Vector2 centre, Vector2 size, float duration, bool ellipse = false, int order = 12)
        {
            var go = new GameObject("Telegraph"); go.transform.position = centre;
            var m = go.AddComponent<TelegraphMarker>();
            m.m_Sr = go.AddComponent<SpriteRenderer>();
            m.m_Sr.sprite = ellipse ? PlaceholderArt.Ellipse(size.x, size.y, new Color(1f, 1f, 1f, 0.9f), 2f) : PlaceholderArt.RoundRect(size.x, size.y, new Color(1f, 1f, 1f, 0.9f), 0.15f, 2f);
            m.m_Sr.sortingOrder = order; m.m_Duration = duration; m.m_BaseScale = go.transform.localScale;
            return m;
        }

        void Update()
        {
            m_Age += Time.deltaTime;
            float t = Mathf.Clamp01(m_Age / Mathf.Max(0.01f, m_Duration));
            float rate = Mathf.Lerp(5f, 14f, t);                                   // blinks faster and faster
            bool bright = Mathf.Sin(m_Age * rate * Mathf.PI) > 0f;
            m_Sr.color = bright ? Palette.WarningBright : Palette.Warning;
            transform.localScale = m_BaseScale * (1f + 0.04f * Mathf.Sin(m_Age * 20f));
            if (m_Age >= m_Duration) Destroy(gameObject);
        }
        public void Kill() { Destroy(gameObject); }
    }

    /// <summary>A cupcake: flies a parabola to a marked floor point, harmful in flight, then bursts into a short blast.</summary>
    public sealed class ArcProjectile : Hazard
    {
        Vector2 m_From, m_To; float m_Flight, m_Apex, m_Age, m_BlastRadius, m_BlastSeconds; SpriteRenderer m_Sr; CircleCollider2D m_Circle; bool m_Landed; BossContext m_Ctx; Color m_Burst; bool m_LeavePuddle; BossTuning.FrostBlob m_Puddle;
        public System.Action<Vector2> Landed;

        public static Vector2 Position(Vector2 from, Vector2 to, float apexHeight, float t)
        {
            // a parabola through both ends that peaks apexHeight above the higher end
            float x = Mathf.Lerp(from.x, to.x, t);
            float baseY = Mathf.Lerp(from.y, to.y, t);
            return new Vector2(x, baseY + 4f * apexHeight * t * (1f - t));
        }

        public static ArcProjectile Launch(BossContext ctx, Vector2 from, Vector2 to, float flight, float apex, int damage, float blastRadius, float blastSeconds, Sprite sprite, Color burst)
        {
            if (!ctx.TrySpawnHazard()) return null;
            var go = new GameObject("ArcProjectile"); go.transform.position = from;
            var p = go.AddComponent<ArcProjectile>(); ctx.Register(p);
            p.m_Ctx = ctx; p.m_From = from; p.m_To = to; p.m_Flight = Mathf.Max(0.2f, flight); p.m_Apex = apex; p.DamageAmount = damage; p.m_BlastRadius = blastRadius; p.m_BlastSeconds = blastSeconds; p.m_Burst = burst;
            p.m_Circle = go.AddComponent<CircleCollider2D>(); p.m_Circle.isTrigger = true; p.m_Circle.radius = 0.32f;
            p.m_Sr = go.AddComponent<SpriteRenderer>(); p.m_Sr.sprite = sprite; p.m_Sr.sortingOrder = 22;
            p.Armed = true;
            return p;
        }

        public void LeavePuddle(BossTuning.FrostBlob cfg) { m_LeavePuddle = true; m_Puddle = cfg; }

        protected override void Update()
        {
            base.Update();
            if (m_Landed) return;
            m_Age += Time.deltaTime;
            float t = Mathf.Clamp01(m_Age / m_Flight);
            transform.position = Position(m_From, m_To, m_Apex, t);
            transform.Rotate(0, 0, 220f * Time.deltaTime);
            if (t >= 1f) Land();
        }

        void Land()
        {
            m_Landed = true;
            AudioHooks.Play(m_LeavePuddle ? Cue.Splat : Cue.Splat);
            Fx.Burst(m_To, m_Burst, 10, 5f, 0.26f, 0.6f);
            if (m_LeavePuddle) Puddle.Create(m_Ctx, m_To, m_Puddle);
            Landed?.Invoke(m_To);
            if (m_BlastSeconds > 0f && m_BlastRadius > 0f) { StartCoroutine(Blast()); }
            else Destroy(gameObject);
        }

        IEnumerator Blast()
        {
            m_Sr.enabled = false; m_Circle.radius = m_BlastRadius; transform.position = m_To;
            var ring = new GameObject("Blast"); ring.transform.position = m_To;
            var rs = ring.AddComponent<SpriteRenderer>(); rs.sprite = PlaceholderArt.Circle(m_BlastRadius * 2f, new Color(1f, 0.8f, 0.4f, 0.8f), 3f); rs.sortingOrder = 23;
            float t = 0f;
            while (t < m_BlastSeconds) { t += Time.deltaTime; if (rs) rs.color = new Color(1, 1, 1, 1f - t / m_BlastSeconds); yield return null; }
            Destroy(ring); Destroy(gameObject);
        }

        void OnDestroy() { m_Ctx?.Unregister(this); }
    }

    /// <summary>The low shockwave after a hand slam: travels along the floor, jump (or dash) to avoid it.</summary>
    public sealed class Shockwave : Hazard
    {
        float m_Dir, m_Speed, m_Life, m_Age, m_MinX, m_MaxX; BossContext m_Ctx;
        public static Shockwave Spawn(BossContext ctx, Vector2 from, float dir, BossTuning.HandSlam cfg)
        {
            if (!ctx.TrySpawnHazard()) return null;
            var go = new GameObject("Shockwave"); go.transform.position = from;
            var s = go.AddComponent<Shockwave>(); ctx.Register(s);
            s.m_Ctx = ctx; s.m_Dir = dir; s.m_Speed = cfg.waveSpeed; s.m_Life = cfg.waveLife; s.DamageAmount = cfg.damage; s.m_MinX = ctx.ArenaLeft - 1f; s.m_MaxX = ctx.WallX;
            Hazard.AddBox(go, new Vector2(cfg.waveWidth, cfg.waveHeight), new Vector2(0f, cfg.waveHeight * 0.5f));
            var sr = PlaceholderArt.Part(go.transform, "wave", PlaceholderArt.RoundRect(cfg.waveWidth, cfg.waveHeight, new Color(1f, 0.55f, 0.7f, 1f), 0.3f), new Vector2(0f, cfg.waveHeight * 0.5f), 20);
            PlaceholderArt.Part(go.transform, "waveCap", PlaceholderArt.Circle(cfg.waveHeight * 0.9f, new Color(1f, 0.95f, 0.8f, 1f)), new Vector2(dir * cfg.waveWidth * 0.2f, cfg.waveHeight * 0.55f), 21);
            s.Armed = true;
            return s;
        }
        protected override void Update()
        {
            base.Update();
            m_Age += Time.deltaTime;
            transform.position += new Vector3(m_Dir * m_Speed * Time.deltaTime, 0f, 0f);
            if (m_Age > m_Life || transform.position.x < m_MinX || transform.position.x > m_MaxX) Destroy(gameObject);
        }
        void OnDestroy() { m_Ctx?.Unregister(this); }
    }

    /// <summary>A sticky frosting puddle left by the Frost Blob: harmful to touch for a few seconds, fades out (and goes harmless while fading).</summary>
    public sealed class Puddle : Hazard
    {
        float m_Life, m_Age; SpriteRenderer m_Sr; BossContext m_Ctx;
        public static Puddle Create(BossContext ctx, Vector2 centre, BossTuning.FrostBlob cfg)
        {
            if (!ctx.TrySpawnHazard()) return null;
            var go = new GameObject("Puddle"); go.transform.position = centre;
            var p = go.AddComponent<Puddle>(); ctx.Register(p);
            p.m_Ctx = ctx; p.m_Life = cfg.puddleSeconds; p.DamageAmount = cfg.damage;
            Hazard.AddBox(go, new Vector2(cfg.puddleWidth * 0.9f, cfg.puddleHeight), new Vector2(0f, cfg.puddleHeight * 0.5f));
            p.m_Sr = PlaceholderArt.Part(go.transform, "puddle", PlaceholderArt.Ellipse(cfg.puddleWidth, cfg.puddleHeight * 1.5f, new Color(0.85f, 0.4f, 0.75f, 1f), 3f), new Vector2(0f, cfg.puddleHeight * 0.5f), 18);
            PlaceholderArt.Part(go.transform, "shine", PlaceholderArt.Ellipse(cfg.puddleWidth * 0.4f, cfg.puddleHeight * 0.5f, new Color(1f, 0.8f, 0.9f, 0.9f), 0f), new Vector2(-cfg.puddleWidth * 0.15f, cfg.puddleHeight * 0.7f), 19);
            p.Armed = true;
            return p;
        }
        protected override void Update()
        {
            base.Update();
            m_Age += Time.deltaTime;
            float left = m_Life - m_Age;
            if (left < 0.6f) { Armed = false; var c = m_Sr.color; c.a = Mathf.Clamp01(left / 0.6f); m_Sr.color = c; }      // fading: safe again
            if (left <= 0f) Destroy(gameObject);
        }
        void OnDestroy() { m_Ctx?.Unregister(this); }
    }

    /// <summary>The column the boss's hand comes down on. Telegraphed first (see HandSlam), armed only for the slam itself.</summary>
    public sealed class SlamColumn : Hazard
    {
        BossContext m_Ctx;
        public static SlamColumn Create(BossContext ctx, Vector2 floorCentre, float width, float height, int damage)
        {
            if (!ctx.TrySpawnHazard()) return null;
            var go = new GameObject("SlamColumn"); go.transform.position = floorCentre;
            var s = go.AddComponent<SlamColumn>(); ctx.Register(s); s.m_Ctx = ctx; s.DamageAmount = damage;
            Hazard.AddBox(go, new Vector2(width, height), new Vector2(0f, height * 0.5f));
            return s;
        }
        void OnDestroy() { m_Ctx?.Unregister(this); }
    }
}

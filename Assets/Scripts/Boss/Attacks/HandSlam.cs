using System.Collections;
using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Boss
{
    /// <summary>
    /// Cakezilla's glove hovers over a red floor column, shakes (warning), then slams it. The column hurts only during the slam; afterwards a low shockwave
    /// runs along the floor in each direction (jump or dash through it).
    /// </summary>
    public sealed class HandSlam : BossAttack
    {
        public override string Name => "Hand Slam";
        float m_X; TelegraphMarker m_Marker;
        public override float TelegraphSeconds(BossContext c) => c.Tuning.handSlam.telegraphSeconds;
        public override float Weight(BossContext c) => BossTuning.At(c.Tuning.handSlam.weight, c.Phase - 1);

        public override IEnumerator Telegraph(BossContext c)
        {
            var cfg = c.Tuning.handSlam;
            m_X = c.ClampToArena(c.PlayerX + Random.Range(-1.0f, 1.0f), 1.6f);
            m_Marker = TelegraphMarker.Create(new Vector2(m_X, c.FloorY + cfg.zoneHeight * 0.5f), new Vector2(cfg.zoneWidth, cfg.zoneHeight), cfg.telegraphSeconds, false, 11);
            AudioHooks.Play(Cue.Telegraph);
            c.Visual.Telegraph(true, 1);
            float t = 0f;
            var start = c.Visual.HandPosition(1);
            var above = new Vector2(m_X, c.FloorY + cfg.zoneHeight + 1.2f);
            while (t < cfg.telegraphSeconds)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / (cfg.telegraphSeconds * 0.5f)));
                c.Visual.MoveHand(1, Vector2.Lerp(start, above, k) + new Vector2(Mathf.Sin(t * 40f) * 0.06f * k, 0f));
                yield return null;
            }
        }

        public override IEnumerator Execute(BossContext c)
        {
            var cfg = c.Tuning.handSlam;
            c.Visual.Telegraph(false, 1);
            var col = SlamColumn.Create(c, new Vector2(m_X, c.FloorY), cfg.zoneWidth * 0.9f, cfg.zoneHeight, cfg.damage);
            var from = c.Visual.HandPosition(1); var to = new Vector2(m_X, c.FloorY + 1.1f);
            float t = 0f;
            while (t < cfg.slamSeconds) { t += Time.deltaTime; c.Visual.MoveHand(1, Vector2.Lerp(from, to, Mathf.Clamp01(t / cfg.slamSeconds))); yield return null; }
            if (col) col.Armed = true;
            AudioHooks.Play(Cue.Slam);
            CameraShake.Shake(0.35f, 0.3f);
            Fx.Burst(new Vector2(m_X, c.FloorY + 0.2f), new Color(1f, 0.85f, 0.9f), 12, 6f, 0.3f, 0.6f);
            Shockwave.Spawn(c, new Vector2(m_X - cfg.zoneWidth * 0.45f, c.FloorY), -1f, cfg);
            if (m_X + cfg.zoneWidth < c.WallX - 1f) Shockwave.Spawn(c, new Vector2(m_X + cfg.zoneWidth * 0.45f, c.FloorY), 1f, cfg);
            yield return new WaitForSeconds(0.2f);
            if (col) Object.Destroy(col.gameObject);                  // the slam itself is brief; the waves are the lingering part
            yield return new WaitForSeconds(cfg.holdSeconds);
            // glove goes home
            var home = c.Visual.HandHome(1); var cur = c.Visual.HandPosition(1); t = 0f;
            while (t < 0.5f) { t += Time.deltaTime; c.Visual.MoveHand(1, Vector2.Lerp(cur, home, Mathf.SmoothStep(0, 1, t / 0.5f))); yield return null; }
            yield return new WaitForSeconds(Mathf.Max(0f, cfg.waveLife * 0.25f));
        }
    }
}

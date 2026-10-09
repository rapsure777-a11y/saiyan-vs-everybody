using System.Collections;
using System.Collections.Generic;
using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Boss
{
    /// <summary>Cakezilla flicks 1 to 3 cupcakes in high arcs. Landing spots are marked on the floor for the whole wind-up and flight; each cupcake bursts into a small blast.</summary>
    public sealed class CupcakeToss : BossAttack
    {
        public override string Name => "Cupcake Toss";
        readonly List<Vector2> m_Targets = new List<Vector2>(); readonly List<TelegraphMarker> m_Markers = new List<TelegraphMarker>();
        public override float TelegraphSeconds(BossContext c) => c.Tuning.cupcakeToss.telegraphSeconds;
        public override float Weight(BossContext c) => BossTuning.At(c.Tuning.cupcakeToss.weight, c.Phase - 1);

        public override IEnumerator Telegraph(BossContext c)
        {
            var cfg = c.Tuning.cupcakeToss;
            int count = Random.Range(cfg.minCount, cfg.maxCount + 1);
            m_Targets.Clear(); m_Markers.Clear();
            float baseX = c.PlayerX;
            for (int i = 0; i < count; i++)
            {
                // the first lands near the player (clamped into the arena), the others fan out so there is always a gap
                float off = i == 0 ? Random.Range(-0.6f, 0.6f) : (i % 2 == 1 ? 1f : -1f) * cfg.spread * Mathf.Ceil(i / 2f + 0.5f) * 0.8f;
                float x = c.ClampToArena(baseX + off);
                m_Targets.Add(new Vector2(x, c.FloorY));
                float life = cfg.telegraphSeconds + i * cfg.stagger + cfg.flightSeconds;
                m_Markers.Add(TelegraphMarker.Create(new Vector2(x, c.FloorY + 0.05f), new Vector2(cfg.blastRadius * 2.2f, 0.45f), life, true));
            }
            AudioHooks.Play(Cue.Telegraph);
            c.Visual.Telegraph(true, 0);
            yield return new WaitForSeconds(cfg.telegraphSeconds);
        }

        public override IEnumerator Execute(BossContext c)
        {
            var cfg = c.Tuning.cupcakeToss;
            c.Visual.Telegraph(false, 0);
            var cupcake = PlaceholderArt.Circle(0.7f, Palette.Frosting);
            for (int i = 0; i < m_Targets.Count; i++)
            {
                var from = c.Visual.HandPosition(0) + new Vector2(0f, 0.6f);
                AudioHooks.Play(Cue.Toss);
                c.Visual.Throw(0);
                var p = ArcProjectile.Launch(c, from, m_Targets[i], cfg.flightSeconds, cfg.apexHeight, cfg.damage, cfg.blastRadius, cfg.blastSeconds, cupcake, Palette.Frosting);
                if (p) AddCherry(p);
                yield return new WaitForSeconds(cfg.stagger);
            }
            yield return new WaitForSeconds(cfg.flightSeconds);
        }

        static void AddCherry(ArcProjectile p)
        {
            PlaceholderArt.Part(p.transform, "cherry", PlaceholderArt.Circle(0.26f, new Color(0.95f, 0.1f, 0.15f)), new Vector2(0.05f, 0.3f), 23);
            PlaceholderArt.Part(p.transform, "wrapper", PlaceholderArt.RoundRect(0.55f, 0.22f, Palette.Sponge, 0.06f), new Vector2(0f, -0.28f), 22);
        }
    }
}

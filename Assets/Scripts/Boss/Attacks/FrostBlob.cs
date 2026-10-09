using System.Collections;
using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Boss
{
    /// <summary>Cakezilla spits a frosting blob at a marked floor spot; it leaves a sticky puddle that is harmful for a few seconds, then fades and turns safe.</summary>
    public sealed class FrostBlob : BossAttack
    {
        public override string Name => "Frost Blob";
        Vector2 m_Target; TelegraphMarker m_Marker;
        public override float TelegraphSeconds(BossContext c) => c.Tuning.frostBlob.telegraphSeconds;
        public override float Weight(BossContext c) => BossTuning.At(c.Tuning.frostBlob.weight, c.Phase - 1);

        public override IEnumerator Telegraph(BossContext c)
        {
            var cfg = c.Tuning.frostBlob;
            // aim a little ahead of where the player is running, so standing still is not the only way to get hit, but never on top of them without warning
            float x = c.ClampToArena(c.PlayerX + Random.Range(-2.5f, 2.5f), 1.8f);
            m_Target = new Vector2(x, c.FloorY);
            m_Marker = TelegraphMarker.Create(new Vector2(x, c.FloorY + 0.1f), new Vector2(cfg.puddleWidth, 0.7f), cfg.telegraphSeconds + cfg.flightSeconds, true);
            AudioHooks.Play(Cue.Telegraph);
            c.Visual.Telegraph(true, 2);
            yield return new WaitForSeconds(cfg.telegraphSeconds);
        }

        public override IEnumerator Execute(BossContext c)
        {
            var cfg = c.Tuning.frostBlob;
            c.Visual.Telegraph(false, 2);
            AudioHooks.Play(Cue.Toss);
            c.Visual.Spit();
            var sprite = PlaceholderArt.Circle(0.8f, new Color(0.9f, 0.45f, 0.78f));
            var p = ArcProjectile.Launch(c, c.Visual.MouthPosition(), m_Target, cfg.flightSeconds, cfg.apexHeight, cfg.damage, 0f, 0f, sprite, Palette.Frosting);
            if (p) p.LeavePuddle(cfg);
            yield return new WaitForSeconds(cfg.flightSeconds + 0.3f);
        }
    }
}

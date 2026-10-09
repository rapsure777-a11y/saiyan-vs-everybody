using System.Collections.Generic;
using NUnit.Framework;
using Saiyan.Boss;
using Saiyan.Core;
using Saiyan.Level;
using Saiyan.Player;
using UnityEngine;

namespace Saiyan.Tests
{
    public class JumpTimersTests
    {
        [Test]
        public void BufferedPressJumpsOnLanding()
        {
            var j = new JumpTimers { Coyote = 0.1f, Buffer = 0.12f };
            j.Tick(0.016f, false, true);                 // pressed in the air
            Assert.IsFalse(j.TryConsume());
            j.Tick(0.05f, false, false);                 // still falling, 50 ms later
            j.Tick(0.016f, true, false);                 // lands within the buffer window
            Assert.IsTrue(j.TryConsume());
        }

        [Test]
        public void PressTooEarlyIsForgotten()
        {
            var j = new JumpTimers();
            j.Tick(0.016f, false, true);
            for (int i = 0; i < 10; i++) j.Tick(0.016f, false, false);   // 160 ms in the air
            j.Tick(0.016f, true, false);
            Assert.IsFalse(j.TryConsume());
        }

        [Test]
        public void CoyoteAllowsJumpJustAfterLeavingLedge_ButNotLater()
        {
            var j = new JumpTimers { Coyote = 0.1f, Buffer = 0.12f };
            j.Tick(0.016f, true, false);                 // standing
            j.Tick(0.05f, false, false);                 // walked off 50 ms ago
            j.Tick(0.001f, false, true);
            Assert.IsTrue(j.TryConsume());
            var k = new JumpTimers { Coyote = 0.1f, Buffer = 0.12f };
            k.Tick(0.016f, true, false); k.Tick(0.15f, false, false); k.Tick(0.001f, false, true);
            Assert.IsFalse(k.TryConsume());              // 150 ms after the ledge: too late
        }

        [Test]
        public void OneJumpPerPress()
        {
            var j = new JumpTimers();
            j.Tick(0.016f, true, true);
            Assert.IsTrue(j.TryConsume());
            Assert.IsFalse(j.TryConsume());
            j.Tick(0.016f, true, false);
            Assert.IsFalse(j.TryConsume());
        }
    }

    public class AttackSelectorTests
    {
        [Test]
        public void NeverTheSameAttackThreeTimesInARow()
        {
            for (int seed = 1; seed <= 30; seed++)
            {
                var s = new AttackSelector(seed); var w = new List<float> { 1f, 1f, 1f };
                int a = -2, b = -2;
                for (int i = 0; i < 2000; i++)
                {
                    int c = s.Pick(w);
                    Assert.IsFalse(c == a && c == b, $"seed {seed} step {i}: attack {c} three times in a row");
                    a = b; b = c;
                }
            }
        }

        [Test]
        public void HeavilyWeightedAttackStillNeverTriples()
        {
            var s = new AttackSelector(5); var w = new List<float> { 100f, 1f, 1f };
            int a = -2, b = -2, zeros = 0;
            for (int i = 0; i < 5000; i++) { int c = s.Pick(w); if (c == 0) zeros++; Assert.IsFalse(c == a && c == b); a = b; b = c; }
            Assert.Greater(zeros, 2500);                 // the weight is still respected
        }

        [Test]
        public void ZeroWeightIsNeverPicked_AndNothingAvailableReturnsMinusOne()
        {
            var s = new AttackSelector(3);
            for (int i = 0; i < 500; i++) Assert.AreNotEqual(1, s.Pick(new List<float> { 1f, 0f, 1f }));
            Assert.AreEqual(-1, s.Pick(new List<float> { 0f, 0f }));
        }

        [Test]
        public void SingleOptionIsAllowedToRepeat()
        {
            var s = new AttackSelector(3);
            for (int i = 0; i < 10; i++) Assert.AreEqual(0, s.Pick(new List<float> { 1f }));
        }
    }

    public class BossHealthTests
    {
        static BossHealth Make(int max = 100)
        {
            var go = new GameObject("boss"); var h = go.AddComponent<BossHealth>(); h.Configure(max, 0.70f, 0.35f); return h;
        }

        [Test]
        public void ThresholdsFireOnceEach_InOrder()
        {
            var h = Make(); var seen = new List<int>(); h.ThresholdReached += p => seen.Add(p);
            for (int i = 0; i < 40; i++) h.TakeDamage(1);                       // 100 -> 60
            Assert.AreEqual(new List<int> { 2 }, seen);
            h.Invulnerable = false;
            for (int i = 0; i < 30; i++) h.TakeDamage(1);                       // 60 -> 30
            Assert.AreEqual(new List<int> { 2, 3 }, seen);
            Assert.AreEqual(3, h.Phase);
            Object.DestroyImmediate(h.gameObject);
        }

        [Test]
        public void BigHitStopsOnTheThreshold_NeverSkipsAPhase()
        {
            var h = Make(); var seen = new List<int>(); h.ThresholdReached += p => { seen.Add(p); h.Invulnerable = true; };
            h.TakeDamage(60);                                                   // would land at 40, past the 70 line
            Assert.AreEqual(70, h.Current);
            Assert.AreEqual(2, h.Phase);
            h.TakeDamage(12);                                                   // ignored: transition
            Assert.AreEqual(70, h.Current);
            h.Invulnerable = false; h.TakeDamage(50);                           // would land at 20, past 35
            Assert.AreEqual(35, h.Current); Assert.AreEqual(3, h.Phase);
            Object.DestroyImmediate(h.gameObject);
        }

        [Test]
        public void InvulnerableBossTakesNoDamage_AndDefeatFiresOnce()
        {
            var h = Make(10); int defeated = 0; h.DefeatedEvent += () => defeated++;
            h.Invulnerable = true; Assert.IsFalse(h.TakeDamage(5)); Assert.AreEqual(10, h.Current);
            h.Invulnerable = false;
            h.ThresholdReached += p => { };
            for (int i = 0; i < 30; i++) { h.Invulnerable = false; h.TakeDamage(1); }
            Assert.IsTrue(h.Defeated); Assert.AreEqual(0, h.Current); Assert.AreEqual(1, defeated);
            Object.DestroyImmediate(h.gameObject);
        }

        [Test]
        public void PhaseFor_MatchesThresholds()
        {
            Assert.AreEqual(1, BossHealth.PhaseFor(100, 100, 0.7f, 0.35f));
            Assert.AreEqual(1, BossHealth.PhaseFor(71, 100, 0.7f, 0.35f));
            Assert.AreEqual(2, BossHealth.PhaseFor(70, 100, 0.7f, 0.35f));
            Assert.AreEqual(2, BossHealth.PhaseFor(36, 100, 0.7f, 0.35f));
            Assert.AreEqual(3, BossHealth.PhaseFor(35, 100, 0.7f, 0.35f));
        }
    }

    public class PlayerHealthTests
    {
        static PlayerHealth Make(int hearts = 3) { var go = new GameObject("p"); var h = go.AddComponent<PlayerHealth>(); h.ResetHearts(hearts); return h; }

        [Test]
        public void HitCostsOneHeart_ThenInvulnerableUntilItWearsOff()
        {
            var h = Make(); h.PostHitInvulnerability = 1f;
            Assert.IsTrue(h.TakeHit(1, Vector2.zero)); Assert.AreEqual(2, h.Current);
            Assert.IsFalse(h.TakeHit(1, Vector2.zero)); Assert.AreEqual(2, h.Current);          // protected
            Assert.IsTrue(h.Blinking);
            Object.DestroyImmediate(h.gameObject);
        }

        [Test]
        public void DiesAtZeroAndRaisesDiedOnce()
        {
            var h = Make(1); int died = 0; h.Died += () => died++;
            h.TakeHit(1, Vector2.zero); h.TakeHit(1, Vector2.zero);
            Assert.IsTrue(h.Dead); Assert.AreEqual(0, h.Current); Assert.AreEqual(1, died);
            Object.DestroyImmediate(h.gameObject);
        }

        [Test]
        public void GrantedInvulnerabilityBlocksHits()
        {
            var h = Make(); h.GrantInvulnerability(1f);
            Assert.IsFalse(h.TakeHit(1, Vector2.zero)); Assert.AreEqual(3, h.Current);
            Object.DestroyImmediate(h.gameObject);
        }
    }

    public class SuperMeterTests
    {
        [Test]
        public void FillsSpendsAllAndOnlyWhenFull()
        {
            var go = new GameObject("m"); var m = go.AddComponent<SuperMeter>();
            m.Add(60f); Assert.IsFalse(m.Full); Assert.IsFalse(m.TryConsume()); Assert.AreEqual(60f, m.Value, 0.01f);
            m.Add(80f); Assert.IsTrue(m.Full); Assert.AreEqual(100f, m.Value, 0.01f);
            Assert.IsTrue(m.TryConsume()); Assert.AreEqual(0f, m.Value, 0.01f); Assert.IsFalse(m.TryConsume());
            Object.DestroyImmediate(go);
        }
    }

    public class BossDesignTests
    {
        [Test]
        public void EveryAttackTelegraphsAtLeastTheMinimum()
        {
            var t = BossTuning.CreateDefault();
            Assert.GreaterOrEqual(t.cupcakeToss.telegraphSeconds, BossTuning.MinTelegraphSeconds);
            Assert.GreaterOrEqual(t.handSlam.telegraphSeconds, BossTuning.MinTelegraphSeconds);
            Assert.GreaterOrEqual(t.frostBlob.telegraphSeconds, BossTuning.MinTelegraphSeconds);
        }

        [Test]
        public void PhaseLinesAreOrderedAndAttackWeightsExist()
        {
            var t = BossTuning.CreateDefault();
            Assert.Greater(t.phase2At, t.phase3At);
            Assert.AreEqual(0.70f, t.phase2At, 1e-4f); Assert.AreEqual(0.35f, t.phase3At, 1e-4f);
            Assert.AreEqual(3, t.cupcakeToss.weight.Length); Assert.AreEqual(3, t.handSlam.weight.Length); Assert.AreEqual(3, t.frostBlob.weight.Length);
            Assert.Greater(t.maxLiveHazards, 0);
        }

        [Test]
        public void ArcProjectile_StartsAndEndsWhereTold_AndPeaksInTheMiddle()
        {
            var a = new Vector2(10f, 5f); var b = new Vector2(4f, 0f);
            Assert.AreEqual(0f, Vector2.Distance(a, ArcProjectile.Position(a, b, 4f, 0f)), 1e-4f);
            Assert.AreEqual(0f, Vector2.Distance(b, ArcProjectile.Position(a, b, 4f, 1f)), 1e-4f);
            var mid = ArcProjectile.Position(a, b, 4f, 0.5f);
            Assert.AreEqual(2.5f + 4f, mid.y, 1e-4f);
        }

        [Test]
        public void ArenaGeometry_BossWallIsInsideTheScreen_AndPlatformsAreReachable()
        {
            Assert.Less(LevelLayout.BossWallX, LevelLayout.ArenaLeft + LevelLayout.CameraWidth);
            Assert.Greater(LevelLayout.BossWallX - LevelLayout.ArenaLeft, 10f);                  // enough room to dodge
            var tune = new PlayerTuning();
            float jumpHeight = tune.jumpVelocity * tune.jumpVelocity / (2f * tune.gravity);
            Assert.Greater(jumpHeight, 2.0f + 0.2f);                                              // platform top is 2.0 high
        }
    }

    public class AudioHooksTests
    {
        [Test]
        public void PlayingACueLogsIt_EvenWithoutAudioDevice()
        {
            AudioHooks.Log.Clear();
            AudioHooks.Play(Cue.Telegraph);
            Assert.AreEqual(Cue.Telegraph, AudioHooks.Log[AudioHooks.Log.Count - 1]);
        }
    }
}

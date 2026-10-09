using System;
using UnityEngine;

namespace Saiyan.Boss
{
    /// <summary>
    /// Every King Cakezilla number in one asset (Assets/Settings/KingCakezillaTuning.asset). Times are seconds, distances are world units.
    /// Fairness rule: every attack shows a warning (colored marker and a sound) for at least <see cref="MinTelegraphSeconds"/> before anything can hurt.
    /// </summary>
    [CreateAssetMenu(menuName = "Saiyan/Boss Tuning", fileName = "KingCakezillaTuning")]
    public sealed class BossTuning : ScriptableObject
    {
        public const float MinTelegraphSeconds = 0.6f;

        [Header("Health and phases")]
        public int maxHealth = 120;
        [Range(0.5f, 0.95f)] public float phase2At = 0.70f;
        [Range(0.1f, 0.6f)] public float phase3At = 0.35f;
        public float transitionSeconds = 2.6f;

        [Header("Rhythm (index 0 = phase 1)")]
        public float[] idleSeconds = { 1.1f, 0.9f, 0.7f };
        public float[] recoverySeconds = { 0.9f, 0.75f, 0.6f };

        [Serializable] public class CupcakeToss
        {
            public float telegraphSeconds = 0.9f; public int minCount = 1, maxCount = 3; public float flightSeconds = 1.25f, stagger = 0.3f, spread = 2.6f, apexHeight = 4.2f, blastRadius = 0.95f, blastSeconds = 0.2f;
            public int damage = 1; public float[] weight = { 1f, 1f, 1f };
        }
        [Serializable] public class HandSlam
        {
            public float telegraphSeconds = 1.15f, zoneWidth = 2.6f, zoneHeight = 5f, slamSeconds = 0.18f, holdSeconds = 0.35f; public float waveSpeed = 7f, waveHeight = 0.8f, waveWidth = 0.9f, waveLife = 3.2f;
            public int damage = 1; public float[] weight = { 1f, 1f, 1f };
        }
        [Serializable] public class FrostBlob
        {
            public float telegraphSeconds = 1.0f, flightSeconds = 1.0f, apexHeight = 3.4f, puddleWidth = 2.8f, puddleHeight = 0.4f, puddleSeconds = 3.5f;
            public int damage = 1; public float[] weight = { 1f, 1f, 1f };
        }

        public CupcakeToss cupcakeToss = new CupcakeToss();
        public HandSlam handSlam = new HandSlam();
        public FrostBlob frostBlob = new FrostBlob();

        [Header("Limits")]
        [Tooltip("Never more than this many damaging objects alive at once.")] public int maxLiveHazards = 12;

        public static BossTuning CreateDefault() { var t = CreateInstance<BossTuning>(); t.name = "KingCakezillaTuning (default)"; return t; }

        public static T At<T>(T[] arr, int phaseIndex) where T : struct => arr == null || arr.Length == 0 ? default : arr[Mathf.Clamp(phaseIndex, 0, arr.Length - 1)];
    }
}

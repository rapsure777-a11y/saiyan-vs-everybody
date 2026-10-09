using System;
using UnityEngine;

namespace Saiyan.Player
{
    /// <summary>The Super meter: charged by star hits and star pickups, spent all at once. Pure state so it is easy to test.</summary>
    public sealed class SuperMeter : MonoBehaviour
    {
        public float Max = 100f, ChargePerHit = 1.5f;
        public float Value { get; private set; }
        public bool Full => Value >= Max - 0.001f;
        public float Fraction => Max <= 0f ? 0f : Value / Max;
        public event Action<float> Changed;

        /// <summary>A star landed a hit. Ordinary stars charge the meter; the Super itself (big damage) does not refill it.</summary>
        public void OnShotLanded(int damage) { if (damage <= 3) Add(ChargePerHit * damage); }

        public void Add(float amount) { Value = Mathf.Clamp(Value + amount, 0f, Max); Changed?.Invoke(Fraction); }
        public void Clear() { Value = 0f; Changed?.Invoke(0f); }
        /// <summary>Spends the whole meter if full.</summary>
        public bool TryConsume() { if (!Full) return false; Clear(); return true; }
    }
}

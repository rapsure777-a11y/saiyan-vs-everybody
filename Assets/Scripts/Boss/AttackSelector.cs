using System;
using System.Collections.Generic;

namespace Saiyan.Boss
{
    /// <summary>
    /// Weighted attack choice that never repeats the same attack three times in a row (when there is any alternative).
    /// Pure logic with an injectable random source so it is unit tested.
    /// </summary>
    public sealed class AttackSelector
    {
        readonly Random m_Rng; int m_Last = -1, m_Prev = -1;
        public AttackSelector(int seed = 0) { m_Rng = seed == 0 ? new Random() : new Random(seed); }

        public int Last => m_Last;

        /// <param name="weights">Weight per attack index; zero or negative = not available.</param>
        /// <returns>The chosen index, or -1 when nothing is available.</returns>
        public int Pick(IList<float> weights)
        {
            float total = 0f; int avail = 0;
            for (int i = 0; i < weights.Count; i++) if (weights[i] > 0f) { total += weights[i]; avail++; }
            if (avail == 0) return -1;
            bool banLast = avail > 1 && m_Last >= 0 && m_Last == m_Prev;
            if (banLast) { total -= weights[m_Last]; }
            double roll = m_Rng.NextDouble() * total; int chosen = -1;
            for (int i = 0; i < weights.Count; i++)
            {
                if (weights[i] <= 0f || (banLast && i == m_Last)) continue;
                chosen = i; roll -= weights[i];
                if (roll < 0) break;
            }
            m_Prev = m_Last; m_Last = chosen;
            return chosen;
        }

        public void Reset() { m_Last = -1; m_Prev = -1; }
    }
}

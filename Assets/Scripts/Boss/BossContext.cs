using System.Collections.Generic;
using Saiyan.Player;
using UnityEngine;

namespace Saiyan.Boss
{
    /// <summary>What an attack needs to know about the fight: the player, the arena, the tuning, and a cap on live hazards.</summary>
    public sealed class BossContext
    {
        public BossTuning Tuning;
        public Transform Player; public PlayerHealth PlayerHealth;
        public BossVisual Visual;
        public float ArenaLeft, FloorY, WallX;                 // WallX = the invisible wall in front of the boss (the player cannot go past it)
        public int Phase = 1;
        readonly List<Hazard> m_Live = new List<Hazard>();

        public int LiveHazards { get { m_Live.RemoveAll(h => !h); return m_Live.Count; } }
        public bool TrySpawnHazard() => LiveHazards < Tuning.maxLiveHazards;
        public void Register(Hazard h) { m_Live.Add(h); }
        public void Unregister(Hazard h) { m_Live.Remove(h); }

        /// <summary>Destroys every live hazard and warning marker (phase changes, defeat, retry).</summary>
        public void ClearHazards()
        {
            foreach (var h in m_Live.ToArray()) if (h) Object.Destroy(h.gameObject);
            m_Live.Clear();
            foreach (var m in Object.FindObjectsByType<TelegraphMarker>(FindObjectsSortMode.None)) if (m) Object.Destroy(m.gameObject);
        }

        public float PlayerX => Player ? Player.position.x : ArenaLeft + 4f;
        /// <summary>Clamps an X to where the player can actually stand.</summary>
        public float ClampToArena(float x, float margin = 1.2f) => Mathf.Clamp(x, ArenaLeft + margin, WallX - margin);
    }
}

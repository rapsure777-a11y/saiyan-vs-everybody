using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Player
{
    /// <summary>Builds the complete player object (physics, health, shooting, super, placeholder visuals). Keeps scene setup free of hand-wired references.</summary>
    public static class PlayerFactory
    {
        public sealed class Player
        {
            public GameObject Go; public PlayerController Controller; public PlayerHealth Health; public PlayerShooter Shooter; public SuperMeter Meter; public PlayerVisual Visual; public SaiyanSpriteVisual SpriteVisual;
            public Transform Transform => Go.transform;
        }

        public static Player Create(Vector2 feetPosition, IIntentSource source, int hearts)
        {
            var go = new GameObject("Saiyan"); go.transform.position = feetPosition;
            var rb = go.AddComponent<Rigidbody2D>();
            var col = go.AddComponent<CapsuleCollider2D>(); col.size = new Vector2(0.6f, 1.3f); col.offset = new Vector2(0f, 0.68f); col.direction = CapsuleDirection2D.Vertical;
            var p = new Player { Go = go, Controller = go.AddComponent<PlayerController>() };
            p.Controller.Source = source;
            p.Health = go.AddComponent<PlayerHealth>(); p.Health.ResetHearts(hearts);
            p.Meter = go.AddComponent<SuperMeter>();
            p.Shooter = go.AddComponent<PlayerShooter>(); p.Shooter.Meter = p.Meter; p.Shooter.Health = p.Health;
            p.Visual = go.AddComponent<PlayerVisual>(); p.Visual.Build();
            p.SpriteVisual = go.AddComponent<SaiyanSpriteVisual>(); p.SpriteVisual.Build(p.Visual);       // real sprite art when it exists (toggle with F9 in the level)
            rb.position = feetPosition;
            return p;
        }
    }
}

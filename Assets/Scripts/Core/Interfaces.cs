using UnityEngine;

namespace Saiyan.Core
{
    /// <summary>Anything the player's stars can hit (boss hurtbox, target dummies, later: cupcake swarm).</summary>
    public interface IShootable
    {
        /// <summary>Returns true if the shot was absorbed (the star is used up unless it pierces).</summary>
        bool OnStarHit(int damage, Vector2 point);
    }

    /// <summary>Anything that can hurt the player.</summary>
    public interface IHazard { int Damage { get; } }
}

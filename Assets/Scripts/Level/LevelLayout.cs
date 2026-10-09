using UnityEngine;

namespace Saiyan.Level
{
    /// <summary>World-space layout of Level01_FrostingFields (units). One screen = 11.2 high by 19.9 wide (orthographic size 5.6, 16:9).</summary>
    public static class LevelLayout
    {
        public const float FloorY = 0f;
        public const float CameraSize = 5.6f;                          // half height
        public const float CameraWidth = CameraSize * 2f * 16f / 9f;   // 19.91
        public const float FloorFromBottom = 1.2f;                     // how far the floor sits above the bottom of the screen
        public static float CameraY => FloorY + CameraSize - FloorFromBottom;

        public const float PlayerStartX = 0f;
        public const float CheckpointX = 66f;
        public const float ArenaLeft = 72f;                            // left edge of the boss screen
        public static float ArenaCentreX => ArenaLeft + CameraWidth * 0.5f;
        public const float BossRootX = ArenaLeft + 17.0f;              // centre of the cake
        public const float BossWallX = ArenaLeft + 14.0f;              // the player cannot go past this (front edge of the cake)
        public const float FightTriggerX = ArenaLeft + 2.5f;
        public const float RetrySpawnX = ArenaLeft + 1.8f;
        public const float LevelStartX = -10f;
        public const float LevelEndX = ArenaLeft + 22f;
    }
}

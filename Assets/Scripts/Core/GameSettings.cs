using UnityEngine;

namespace Saiyan.Core
{
    /// <summary>Player-facing options (accessibility), saved in PlayerPrefs.</summary>
    public static class GameSettings
    {
        const string AssistKey = "saiyan.assist", ShakeKey = "saiyan.reducedshake";

        /// <summary>Assist Mode: 5 hearts instead of 3 and the boss has about a third less health.</summary>
        public static bool AssistMode { get => PlayerPrefs.GetInt(AssistKey, 0) == 1; set { PlayerPrefs.SetInt(AssistKey, value ? 1 : 0); PlayerPrefs.Save(); } }
        /// <summary>Reduced screen shake: slams and hits move the camera far less.</summary>
        public static bool ReducedShake { get => PlayerPrefs.GetInt(ShakeKey, 0) == 1; set { PlayerPrefs.SetInt(ShakeKey, value ? 1 : 0); PlayerPrefs.Save(); } }

        public static int PlayerHearts => AssistMode ? 5 : 3;
        public static float BossHealthScale => AssistMode ? 0.66f : 1f;
    }

    /// <summary>Survives scene reloads: lets a retry start at the checkpoint in front of the boss arena.</summary>
    public static class GameSession
    {
        public static bool StartAtCheckpoint;
        public static int Attempts;
        public static void Reset() { StartAtCheckpoint = false; Attempts = 0; }
    }
}

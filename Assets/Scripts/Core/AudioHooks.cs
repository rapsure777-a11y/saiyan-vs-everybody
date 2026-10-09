using System.Collections.Generic;
using UnityEngine;

namespace Saiyan.Core
{
    public enum Cue { Jump, Land, Shoot, Dash, PlayerHit, Super, Pickup, UiClick, Telegraph, Slam, Splat, Toss, BossHit, PhaseChange, Victory, Defeat, TargetPop }

    /// <summary>
    /// Audio architecture: gameplay code only calls <c>AudioHooks.Play(Cue.X)</c>. Until real sound assets exist, each cue is a short generated placeholder tone
    /// (distinct pitch and shape, so telegraphs are audible). To use real audio, assign clips with <see cref="Register"/> or drop AudioClips in Resources/Audio named after the cue.
    /// NO finished music or voice exists yet.
    /// </summary>
    public static class AudioHooks
    {
        static readonly Dictionary<Cue, AudioClip> s_Clips = new Dictionary<Cue, AudioClip>();
        static AudioSource s_Source;
        public static float Volume = 0.8f;
        /// <summary>Every cue played this session (for tests and debugging).</summary>
        public static readonly List<Cue> Log = new List<Cue>();
        public static event System.Action<Cue> Played;

        public static void Register(Cue cue, AudioClip clip) { s_Clips[cue] = clip; }

        public static void Play(Cue cue, float volume = 1f)
        {
            Log.Add(cue); if (Log.Count > 512) Log.RemoveRange(0, 256);
            Played?.Invoke(cue);
            if (!Application.isPlaying) return;
            if (!s_Source)
            {
                var go = new GameObject("AudioHooks") { hideFlags = HideFlags.HideAndDontSave };
                Object.DontDestroyOnLoad(go);
                s_Source = go.AddComponent<AudioSource>(); s_Source.spatialBlend = 0f;
            }
            if (!s_Clips.TryGetValue(cue, out var clip) || !clip)
            {
                clip = Resources.Load<AudioClip>("Audio/" + cue) ?? Generate(cue);
                s_Clips[cue] = clip;
            }
            s_Source.PlayOneShot(clip, Volume * volume);
        }

        // ---- placeholder tones ----
        static AudioClip Generate(Cue cue)
        {
            float f0, f1, len; int wave; // wave: 0 sine, 1 square, 2 noise
            switch (cue)
            {
                case Cue.Jump: f0 = 380; f1 = 700; len = 0.12f; wave = 0; break;
                case Cue.Land: f0 = 160; f1 = 90; len = 0.08f; wave = 2; break;
                case Cue.Shoot: f0 = 900; f1 = 1300; len = 0.05f; wave = 1; break;
                case Cue.Dash: f0 = 600; f1 = 200; len = 0.14f; wave = 2; break;
                case Cue.PlayerHit: f0 = 300; f1 = 90; len = 0.3f; wave = 1; break;
                case Cue.Super: f0 = 300; f1 = 1500; len = 0.5f; wave = 0; break;
                case Cue.Pickup: f0 = 900; f1 = 1500; len = 0.12f; wave = 0; break;
                case Cue.UiClick: f0 = 700; f1 = 700; len = 0.05f; wave = 0; break;
                case Cue.Telegraph: f0 = 520; f1 = 520; len = 0.28f; wave = 1; break;      // a steady warning honk
                case Cue.Slam: f0 = 90; f1 = 40; len = 0.4f; wave = 2; break;
                case Cue.Splat: f0 = 240; f1 = 120; len = 0.2f; wave = 2; break;
                case Cue.Toss: f0 = 250; f1 = 450; len = 0.15f; wave = 0; break;
                case Cue.BossHit: f0 = 200; f1 = 160; len = 0.04f; wave = 2; break;
                case Cue.PhaseChange: f0 = 500; f1 = 120; len = 0.9f; wave = 1; break;
                case Cue.Victory: f0 = 520; f1 = 1040; len = 0.9f; wave = 0; break;
                case Cue.Defeat: f0 = 400; f1 = 100; len = 0.8f; wave = 1; break;
                default: f0 = 800; f1 = 500; len = 0.1f; wave = 0; break;
            }
            int rate = 22050, n = Mathf.Max(1, (int)(len * rate));
            var data = new float[n]; float phase = 0; var rng = new System.Random((int)cue * 7919 + 1);
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n, f = Mathf.Lerp(f0, f1, t);
                phase += f / rate; float s;
                switch (wave) { case 1: s = (phase % 1f) < 0.5f ? 0.35f : -0.35f; break; case 2: s = ((float)rng.NextDouble() * 2f - 1f) * 0.5f; break; default: s = Mathf.Sin(phase * Mathf.PI * 2f) * 0.6f; break; }
                float env = Mathf.Min(1f, i / (rate * 0.004f)) * (1f - t) * (1f - t * 0.3f);
                data[i] = s * env;
            }
            var clip = AudioClip.Create("placeholder_" + cue, n, 1, rate, false); clip.SetData(data, 0);
            return clip;
        }
    }
}

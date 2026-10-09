using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saiyan.Art
{
    [Serializable] public sealed class ClipEventMeta { public int frame; public string name; }

    /// <summary>The anim.json written by Tools/sprites/process_frames.py next to each processed animation.</summary>
    [Serializable]
    public sealed class AnimMeta
    {
        public string character, name; public float fps = 8f; public bool loop = true; public float ppu = 200f; public int frameSize, frames; public float[] pivot; public int heightPx;
        public ClipEventMeta[] events;
    }

    [Serializable] public sealed class LibraryMeta { public string character; public string[] animations; }

    /// <summary>One animation: ordered sprites, timing, and frame events (for example the frame where a shot leaves the hand).</summary>
    public sealed class SpriteClip
    {
        public string Name; public Sprite[] Frames; public float Fps = 8f; public bool Loop = true; public ClipEventMeta[] Events = new ClipEventMeta[0];
        public float Duration => Frames.Length / Mathf.Max(0.01f, Fps);
    }

    /// <summary>
    /// Frame timing, separated from Unity so it is unit tested: advances with delta time, loops or holds the last frame, and reports EVERY frame entered (so events are
    /// never skipped, even after a long frame).
    /// </summary>
    public sealed class ClipPlayer
    {
        public int Count, Frame; public float Fps = 8f, Speed = 1f; public bool Loop = true, Finished, Paused;
        public Action<int> FrameEntered;
        double m_Time; long m_LastAbs;

        public ClipPlayer(int count = 1, float fps = 8f, bool loop = true) { Count = Mathf.Max(1, count); Fps = fps; Loop = loop; Reset(); }

        public void Reset() { m_Time = 0; m_LastAbs = 0; Frame = 0; Finished = false; }

        public void SetFrame(int frame)               // for manual stepping in the preview
        {
            Frame = Mathf.Clamp(frame, 0, Count - 1); m_LastAbs = Frame; m_Time = Frame;
        }

        public void Tick(float dt)
        {
            if (Paused || Finished) return;
            m_Time += (double)dt * Speed * Fps;
            long abs = (long)Math.Floor(m_Time);
            for (long n = m_LastAbs + 1; n <= abs; n++)
            {
                if (!Loop && n >= Count) { Finished = true; Frame = Count - 1; m_LastAbs = Count - 1; return; }
                int idx = (int)(n % Count); Frame = idx; m_LastAbs = n;
                FrameEntered?.Invoke(idx);
            }
        }
    }

    /// <summary>Loads processed sprite animations from Resources/Art/&lt;Character&gt;/&lt;animation&gt; (frames + anim.json) and the per-character library.json.</summary>
    public static class SpriteLibrary
    {
        static readonly Dictionary<string, SpriteClip> s_Cache = new Dictionary<string, SpriteClip>();

        public static string[] Animations(string character)
        {
            var t = Resources.Load<TextAsset>($"Art/{character}/library");
            if (!t) return new string[0];
            var m = JsonUtility.FromJson<LibraryMeta>(t.text);
            return m?.animations ?? new string[0];
        }

        public static bool HasCharacter(string character) => Animations(character).Length > 0;

        public static bool TryGet(string character, string anim, out SpriteClip clip)
        {
            string key = character + "/" + anim;
            if (s_Cache.TryGetValue(key, out clip)) return clip != null;
            clip = null;
            var sprites = Resources.LoadAll<Sprite>($"Art/{character}/{anim}");
            var meta = Resources.Load<TextAsset>($"Art/{character}/{anim}/anim");
            if (sprites == null || sprites.Length == 0) { s_Cache[key] = null; return false; }
            Array.Sort(sprites, (a, b) => string.CompareOrdinal(a.name, b.name));
            var m = meta ? JsonUtility.FromJson<AnimMeta>(meta.text) : new AnimMeta();
            clip = new SpriteClip { Name = anim, Frames = sprites, Fps = m.fps > 0 ? m.fps : 8f, Loop = m.loop, Events = m.events ?? new ClipEventMeta[0] };
            s_Cache[key] = clip; return true;
        }

        public static void ClearCache() { s_Cache.Clear(); }
    }

    /// <summary>Plays <see cref="SpriteClip"/>s on a SpriteRenderer (runtime sprite swapping), with frame events and a finished callback.</summary>
    public sealed class SpriteFlipbook : MonoBehaviour
    {
        public SpriteRenderer Target; public string Character;
        public SpriteClip Current { get; private set; }
        public string CurrentName => Current?.Name;
        public ClipPlayer Player { get; private set; }
        public float Speed { get => Player?.Speed ?? 1f; set { if (Player != null) Player.Speed = value; } }
        public bool Paused { get => Player != null && Player.Paused; set { if (Player != null) Player.Paused = value; } }
        public int Frame => Player?.Frame ?? 0;
        public event Action<string> Event;               // animation events by name
        public event Action<string> Finished;            // a non-looping clip ended

        public bool Has(string anim) => SpriteLibrary.TryGet(Character, anim, out _);

        /// <summary>Starts a clip; does nothing if it is already playing unless <paramref name="restart"/>. Returns false if the clip does not exist.</summary>
        public bool Play(string anim, bool restart = false)
        {
            if (!restart && Current != null && Current.Name == anim) return true;
            if (!SpriteLibrary.TryGet(Character, anim, out var clip)) return false;
            float speed = Player?.Speed ?? 1f; bool paused = Player?.Paused ?? false;
            Current = clip;
            Player = new ClipPlayer(clip.Frames.Length, clip.Fps, clip.Loop) { Speed = speed, Paused = paused };
            Player.FrameEntered = OnFrame;
            if (Target) Target.sprite = clip.Frames[0];
            return true;
        }

        void OnFrame(int idx)
        {
            if (Target && Current != null) Target.sprite = Current.Frames[idx];
            if (Current?.Events != null) foreach (var e in Current.Events) if (e.frame == idx && !string.IsNullOrEmpty(e.name)) Event?.Invoke(e.name);
        }

        public void StepFrame(int delta)
        {
            if (Player == null) return;
            int n = (Player.Frame + delta + Player.Count) % Player.Count; Player.SetFrame(n);
            if (Target) Target.sprite = Current.Frames[n];
        }

        void Update()
        {
            if (Player == null) return;
            bool wasFinished = Player.Finished;
            Player.Tick(Time.deltaTime);
            if (!wasFinished && Player.Finished) Finished?.Invoke(Current.Name);
        }
    }
}

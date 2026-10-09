using System.Collections.Generic;
using UnityEngine;

namespace Saiyan.Core
{
    /// <summary>Tiny component pool with a hard cap, so projectile counts stay bounded.</summary>
    public sealed class Pool<T> where T : Component
    {
        readonly System.Func<T> m_Make; readonly Stack<T> m_Free = new Stack<T>(); readonly List<T> m_All = new List<T>(); readonly int m_Max;
        public Pool(System.Func<T> make, int max) { m_Make = make; m_Max = max; }
        public int ActiveCount { get { int n = 0; foreach (var t in m_All) if (t && t.gameObject.activeSelf) n++; return n; } }
        /// <summary>Null when the cap is reached (the caller simply skips the spawn).</summary>
        public T Get()
        {
            while (m_Free.Count > 0) { var t = m_Free.Pop(); if (t) { t.gameObject.SetActive(true); return t; } }
            if (m_All.Count >= m_Max) return null;
            var n = m_Make(); m_All.Add(n); n.gameObject.SetActive(true); return n;
        }
        public void Release(T t) { if (!t) return; t.gameObject.SetActive(false); m_Free.Push(t); }
        public void ReleaseAll() { foreach (var t in m_All) if (t && t.gameObject.activeSelf) Release(t); }
    }
}

using Saiyan.Core;
using UnityEngine;

namespace Saiyan.Level
{
    /// <summary>
    /// Side-scrolling camera for the tutorial, then locked on the boss screen. Orthographic, 16:9 framing, floor sits just above the bottom of the screen.
    /// Also animates the sky and the parallax layers and applies <see cref="CameraShake"/>.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        public Transform Target; public float MinX, MaxX; public bool Locked; public float LockX;
        Camera m_Cam; CameraShake m_Shake;
        readonly System.Collections.Generic.List<(Transform t, float factor, float period, float y)> m_Layers = new System.Collections.Generic.List<(Transform, float, float, float)>();
        Transform m_Sky;

        void Awake()
        {
            m_Cam = GetComponent<Camera>(); m_Cam.orthographic = true; m_Cam.orthographicSize = LevelLayout.CameraSize; m_Cam.clearFlags = CameraClearFlags.SolidColor; m_Cam.backgroundColor = Palette.SkyBottom;
            m_Shake = gameObject.GetComponent<CameraShake>(); if (!m_Shake) m_Shake = gameObject.AddComponent<CameraShake>();
        }

        public void SetSky(Transform sky) { m_Sky = sky; }
        /// <summary>A background strip tiled every <paramref name="period"/> units; <paramref name="factor"/> 1 = glued to the camera (far), 0 = moves with the world (near).</summary>
        public void AddLayer(Transform t, float factor, float period) { m_Layers.Add((t, factor, period, t.position.y)); }

        void LateUpdate()
        {
            float x = Locked ? LockX : (Target ? Mathf.Clamp(Target.position.x + 1.5f, MinX, MaxX) : transform.position.x);
            var cur = transform.position;
            float nx = Mathf.Lerp(cur.x, x, 1f - Mathf.Exp(-(Locked ? 4f : 9f) * Time.unscaledDeltaTime));
            if (Mathf.Abs(nx - x) < 0.002f) nx = x;
            var shake = m_Shake ? m_Shake.Offset() : Vector3.zero;
            transform.position = new Vector3(nx, LevelLayout.CameraY, -10f) + shake;
            foreach (var l in m_Layers)
                if (l.t) l.t.position = new Vector3(nx - Mathf.Repeat(nx * (1f - l.factor), l.period) - l.period, l.y, 0f);
            if (m_Sky) m_Sky.position = new Vector3(nx, LevelLayout.CameraY, 5f);
        }

        public void SnapTo(float x) { transform.position = new Vector3(x, LevelLayout.CameraY, -10f); }
    }
}

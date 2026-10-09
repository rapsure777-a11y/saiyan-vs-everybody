using System.Collections.Generic;
using UnityEngine;

namespace Saiyan.Core
{
    /// <summary>
    /// Procedural placeholder sprites (clean cartoon shapes with a dark outline) used until real production art exists. All sizes are in world units at 64 pixels per unit.
    /// This is NOT the final art style target; it only needs to be readable and keep the palette of the concept art.
    /// </summary>
    public static class PlaceholderArt
    {
        public const float Ppu = 64f;
        public static readonly Color Ink = new Color(0.16f, 0.09f, 0.12f, 1f);
        static readonly Dictionary<string, Sprite> s_Cache = new Dictionary<string, Sprite>();

        static Sprite Make(string key, int w, int h, System.Func<float, float, float> sdf, Color fill, Color outline, float outlinePx)
        {
            if (s_Cache.TryGetValue(key, out var cached) && cached) return cached;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float d = sdf(x + 0.5f, y + 0.5f);                        // negative inside, in pixels
                    float a = Mathf.Clamp01(0.5f - d);                         // anti-aliased edge
                    float inner = Mathf.Clamp01(0.5f + (-d - outlinePx));      // 1 deep inside (fill), 0 in the outline band
                    var c = Color.Lerp(outline, fill, inner); c.a *= a * fill.a;
                    px[y * w + x] = c;
                }
            tex.SetPixels32(px); tex.Apply(false, true);
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), Ppu);
            s.hideFlags = HideFlags.HideAndDontSave;
            s_Cache[key] = s; return s;
        }

        static int Px(float units) => Mathf.Clamp(Mathf.RoundToInt(units * Ppu), 4, 640);
        static string K(string kind, float a, float b, Color c, float r) => $"{kind}|{a:0.00}|{b:0.00}|{ColorUtility.ToHtmlStringRGBA(c)}|{r:0.0}";

        public static Sprite Circle(float diameter, Color fill, float outlinePx = 3f) => Ellipse(diameter, diameter, fill, outlinePx);

        public static Sprite Ellipse(float w, float h, Color fill, float outlinePx = 3f)
        {
            int pw = Px(w), ph = Px(h);
            return Make(K("ell", w, h, fill, outlinePx), pw, ph, (x, y) =>
            {
                float rx = pw * 0.5f, ry = ph * 0.5f, nx = (x - rx) / rx, ny = (y - ry) / ry;
                return (Mathf.Sqrt(nx * nx + ny * ny) - 1f) * Mathf.Min(rx, ry);
            }, fill, Ink, outlinePx);
        }

        public static Sprite RoundRect(float w, float h, Color fill, float radius = 0.2f, float outlinePx = 3f)
        {
            int pw = Px(w), ph = Px(h);
            float rr = Mathf.Min(radius * Ppu, Mathf.Min(pw, ph) * 0.5f);
            return Make(K("rr", w, h, fill, radius * 100f + outlinePx * 1000f), pw, ph, (x, y) =>
            {
                float qx = Mathf.Abs(x - pw * 0.5f) - (pw * 0.5f - rr), qy = Mathf.Abs(y - ph * 0.5f) - (ph * 0.5f - rr);
                return Mathf.Sqrt(Mathf.Max(qx, 0) * Mathf.Max(qx, 0) + Mathf.Max(qy, 0) * Mathf.Max(qy, 0)) + Mathf.Min(Mathf.Max(qx, qy), 0) - rr;
            }, fill, Ink, outlinePx);
        }

        public static Sprite Star(float size, Color fill, int points = 5, float inner = 0.5f, float outlinePx = 3f)
        {
            int p = Px(size);
            return Make(K("star", size, points + inner, fill, outlinePx), p, p, (x, y) =>
            {
                float cx = p * 0.5f, dx = x - cx, dy = y - cx, r = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dx, dy); if (ang < 0) ang += Mathf.PI * 2f;                    // 0 = straight up
                float seg = Mathf.PI * 2f / points, t = (ang % seg) / seg;
                float f = t < 0.5f ? t * 2f : (1f - t) * 2f;                                            // 0 at a tip .. 1 at a valley
                float outer = cx * 0.96f, inn = outer * inner;
                float boundary = Mathf.Lerp(outer, inn, f);
                return (r - boundary) * 0.8f;
            }, fill, Ink, outlinePx);
        }

        public static Sprite Heart(float size, Color fill)
        {
            int p = Px(size);
            return Make(K("heart", size, 0, fill, 3f), p, p, (x, y) =>
            {
                float nx = (x / p - 0.5f) * 2.3f, ny = (y / p - 0.45f) * 2.3f;                           // classic implicit heart: (x^2+y^2-1)^3 - x^2 y^3
                float a = nx * nx + ny * ny - 1f; float v = a * a * a - nx * nx * ny * ny * ny;
                return Mathf.Clamp(v * 6f, -6f, 6f);
            }, fill, Ink, 2.5f);
        }

        /// <summary>A vertical gradient strip used for the sky.</summary>
        public static Sprite Gradient(Color top, Color bottom)
        {
            string key = "grad|" + ColorUtility.ToHtmlStringRGB(top) + ColorUtility.ToHtmlStringRGB(bottom);
            if (s_Cache.TryGetValue(key, out var c) && c) return c;
            var tex = new Texture2D(4, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
            for (int y = 0; y < 64; y++) for (int x = 0; x < 4; x++) tex.SetPixel(x, y, Color.Lerp(bottom, top, y / 63f));
            tex.Apply(false, true);
            var s = Sprite.Create(tex, new Rect(0, 0, 4, 64), new Vector2(0.5f, 0.5f), 4f); s.hideFlags = HideFlags.HideAndDontSave;
            s_Cache[key] = s; return s;
        }

        // ---- building blocks for composed placeholder characters and scenery ----

        public static SpriteRenderer Part(Transform parent, string name, Sprite sprite, Vector2 pos, int order, float rotation = 0f, Vector2? scale = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            go.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            go.transform.localScale = scale.HasValue ? new Vector3(scale.Value.x, scale.Value.y, 1f) : Vector3.one;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite; sr.sortingOrder = order;
            return sr;
        }

        public static SpriteRenderer Block(Transform parent, string name, Vector2 pos, Vector2 size, Color fill, int order, float radius = 0.12f)
            => Part(parent, name, RoundRect(size.x, size.y, fill, radius), pos, order);

        public static void ClearCache() { s_Cache.Clear(); }
    }

    /// <summary>The palette taken from the approved concept art.</summary>
    public static class Palette
    {
        public static readonly Color SkyTop = new Color(0.36f, 0.62f, 0.96f), SkyBottom = new Color(0.78f, 0.9f, 1f);
        public static readonly Color Frosting = new Color(1f, 0.52f, 0.7f), FrostingDark = new Color(0.9f, 0.34f, 0.58f), Cream = new Color(1f, 0.93f, 0.8f), Sponge = new Color(0.82f, 0.55f, 0.28f), Chocolate = new Color(0.4f, 0.22f, 0.14f);
        public static readonly Color Hoodie = new Color(0.12f, 0.33f, 0.82f), Skin = new Color(0.99f, 0.8f, 0.64f), Hair = new Color(0.32f, 0.18f, 0.1f), Glove = new Color(1f, 1f, 1f), Pants = new Color(0.13f, 0.13f, 0.16f), Gold = new Color(1f, 0.82f, 0.18f);
        public static readonly Color Warning = new Color(1f, 0.25f, 0.2f, 0.55f), WarningBright = new Color(1f, 0.9f, 0.2f, 0.7f), Safe = new Color(0.4f, 1f, 0.5f, 0.6f);
    }
}

using System.IO;
using Saiyan.Art;
using UnityEditor;
using UnityEngine;

namespace Saiyan.Editor
{
    /// <summary>
    /// Automatic import settings for processed sprite frames: every PNG under Assets/Resources/Art/&lt;Character&gt;/&lt;animation&gt;/ that has an anim.json beside it becomes a
    /// single sprite with the animation's pixels-per-unit and feet pivot, no mipmaps, no compression, bilinear filtering, alpha as transparency.
    /// </summary>
    public sealed class SpriteImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Resources/Art/") || !assetPath.EndsWith(".png")) return;
            string meta = Path.Combine(Path.GetDirectoryName(assetPath) ?? "", "anim.json");
            if (!File.Exists(meta)) return;
            var m = JsonUtility.FromJson<AnimMeta>(File.ReadAllText(meta));
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = m.ppu > 0 ? m.ppu : 200f;
            ti.mipmapEnabled = false; ti.filterMode = FilterMode.Bilinear; ti.alphaIsTransparency = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed; ti.maxTextureSize = 1024; ti.wrapMode = TextureWrapMode.Clamp;
            var s = new TextureImporterSettings(); ti.ReadTextureSettings(s);
            s.spriteAlignment = (int)SpriteAlignment.Custom;
            s.spritePivot = m.pivot != null && m.pivot.Length == 2 ? new Vector2(m.pivot[0], m.pivot[1]) : new Vector2(0.5f, 0f);
            s.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(s);
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace BazaarFlow.Visuals.EditorTools
{
    /// <summary>
    /// Import settings for everything under Assets/BazaarFlow/Art: crisp, uncompressed-quality sprites
    /// with mipmaps (art is drawn at 2-3x its on-screen size) and no power-of-two rescaling, so the generated art looks exactly as rendered.
    /// Store graphics (Art/Store) are imported the same way but are only used for the Play listing and icon.
    /// </summary>
    public class BazaarArtImporter : AssetPostprocessor
    {
        const string ArtRoot = "Assets/BazaarFlow/Art/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot)) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.wrapMode = TextureWrapMode.Clamp;
            // Sprites are drawn 2-3x smaller than their texture on most phones: mipmaps keep edges smooth.
            ti.mipmapEnabled = true;
            ti.filterMode = FilterMode.Trilinear;
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            bool big = assetPath.EndsWith("background.png");
            if (big) ti.mipmapEnabled = false; // shown at about 1:1
            ti.maxTextureSize = big ? 4096 : 2048;

            // Android: ASTC 4x4 keeps the outlines sharp; big background can use 6x6.
            var android = ti.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = big ? 4096 : 2048;
            android.format = big
                ? TextureImporterFormat.ASTC_6x6
                : TextureImporterFormat.ASTC_4x4;
            ti.SetPlatformTextureSettings(android);
        }
    }
}

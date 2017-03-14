using UnityEngine;
using UnityEditor;

internal sealed class CustomAssetImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        var importer = assetImporter as TextureImporter;

        importer.spritePixelsPerUnit = 16; // default 16 pixels per unit
        importer.filterMode = FilterMode.Point;
        TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
        settings.format = TextureImporterFormat.RGBA32;
        settings.maxTextureSize = 2048;
        importer.SetPlatformTextureSettings(settings);
    }
}

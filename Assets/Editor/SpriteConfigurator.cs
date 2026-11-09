using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteConfigurator : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (assetPath.StartsWith("Assets/Sprites/"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            if (fileName.StartsWith("panel_"))
            {
                importer.spriteBorder = new Vector4(24, 24, 24, 24);
            }
            else if (fileName.StartsWith("button_"))
            {
                importer.spriteBorder = new Vector4(16, 16, 16, 16);
            }
        }
    }

    [MenuItem("Tools/Reimport Sprites")]
    public static void ReimportAllSprites()
    {
        string[] files = Directory.GetFiles("Assets/Sprites", "*.png");
        foreach (string file in files)
        {
            AssetDatabase.ImportAsset(file.Replace("\\", "/"), ImportAssetOptions.ForceUpdate);
        }
        Debug.Log("Reimported all sprites as 2D/UI Sprites with borders!");
    }
}

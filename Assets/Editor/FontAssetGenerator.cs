using System.IO;
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace Minesweeper.Editor
{
    public static class FontAssetGenerator
    {
        [MenuItem("Tools/Generate Cyber Font Assets")]
        public static void GenerateFontAssets()
        {
            Debug.Log("Starting Font Asset generation for JetBrainsMono and SpaceGrotesk...");

            GenerateSDF("Assets/Fonts/JetBrainsMono-Bold.ttf", "Assets/Fonts/JetBrainsMono SDF.asset");
            GenerateSDF("Assets/Fonts/SpaceGrotesk-Bold.ttf", "Assets/Fonts/SpaceGrotesk SDF.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Finished generating font assets!");
        }

        private static void GenerateSDF(string ttfPath, string outPath)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null)
            {
                Debug.LogError($"Could not find font at {ttfPath}");
                return;
            }

            // Delete old asset if exists
            if (File.Exists(outPath))
            {
                AssetDatabase.DeleteAsset(outPath);
            }

            var fontAsset = TMP_FontAsset.CreateFontAsset(
                font,
                90,
                9,
                GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic,
                true
            );

            if (fontAsset != null)
            {
                AssetDatabase.CreateAsset(fontAsset, outPath);

                // Add atlas texture sub-assets
                if (fontAsset.atlasTextures != null)
                {
                    for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                    {
                        var tex = fontAsset.atlasTextures[i];
                        if (tex != null)
                        {
                            tex.name = $"{Path.GetFileNameWithoutExtension(outPath)} Atlas {i}";
                            AssetDatabase.AddObjectToAsset(tex, fontAsset);
                        }
                    }
                }

                // Add material sub-asset
                if (fontAsset.material != null)
                {
                    fontAsset.material.name = $"{Path.GetFileNameWithoutExtension(outPath)} Material";
                    AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                }

                EditorUtility.SetDirty(fontAsset);
                Debug.Log($"Successfully created dynamic TMP_FontAsset with sub-assets at {outPath}");
            }
        }
    }
}

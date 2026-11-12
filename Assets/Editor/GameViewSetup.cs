using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Minesweeper.Editor
{
    /// <summary>
    /// Helpers for inspecting the HUD at 1:1 pixel resolution inside the Unity Game view.
    ///
    /// The Game view normally renders the player at the size of the panel, so the canvas
    /// scale factor becomes fractional (e.g. 0.93) and the UI looks slightly soft.
    /// "Tools/Game View/1920x1080 (1:1)" switches the Game view to a fixed 1920x1080
    /// render target shown at 1:1, so every pixel of the interface can be checked exactly
    /// as it is drawn in the standalone build.
    /// </summary>
    public static class GameViewSetup
    {
        private const int TargetWidth = 1920;
        private const int TargetHeight = 1080;

        [MenuItem("Tools/Game View/1920x1080 (1:1)")]
        public static void SetFullResolution()
        {
            try
            {
                Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;

                Type gameViewType = editorAssembly.GetType("UnityEditor.GameView");
                Type gameViewSizesType = editorAssembly.GetType("UnityEditor.GameViewSizes");
                Type gameViewSizeType = editorAssembly.GetType("UnityEditor.GameViewSize");
                Type gameViewSizeTypeEnum = editorAssembly.GetType("UnityEditor.GameViewSizeType");
                Type gameViewSizeGroupType = editorAssembly.GetType("UnityEditor.GameViewSizeGroup");
                Type singletonType = editorAssembly.GetType("UnityEditor.ScriptableSingleton`1");

                var gameView = EditorWindow.GetWindow(gameViewType);

                var singleton = singletonType.MakeGenericType(gameViewSizesType)
                    .GetProperty("instance").GetValue(null, null);
                var currentGroupType = (int)gameViewSizesType.GetMethod("GetCurrentGroupType")
                    .Invoke(singleton, null);
                var group = gameViewSizesType.GetMethod("GetGroup")
                    .Invoke(singleton, new object[] { currentGroupType });

                object fixedSize = null;
                int index = 0;
                var all = (IEnumerable)gameViewSizeGroupType.GetMethod("GetGameViewSizes").Invoke(group, null);
                foreach (var item in all)
                {
                    var itemType = item.GetType();
                    if ((int)itemType.GetProperty("width").GetValue(item, null) == TargetWidth &&
                        (int)itemType.GetProperty("height").GetValue(item, null) == TargetHeight)
                    {
                        fixedSize = item;
                        break;
                    }
                    index++;
                }

                if (fixedSize == null)
                {
                    var ctor = gameViewSizeType.GetConstructor(new[] { gameViewSizeTypeEnum, typeof(int), typeof(int), typeof(string) });
                    fixedSize = ctor.Invoke(new object[]
                    {
                        Enum.ToObject(gameViewSizeTypeEnum, 1), TargetWidth, TargetHeight, "HUD 1920x1080"
                    });
                    gameViewSizeGroupType.GetMethod("AddCustomSize").Invoke(group, new[] { fixedSize });
                    index = (int)gameViewSizeGroupType.GetMethod("GetTotalCount").Invoke(group, null) - 1;
                }

                gameViewSizesType.GetMethod("SetSelectedSizeIndex")
                    .Invoke(group, new object[] { index });

                var scaleProperty = gameViewType.GetProperty("defaultScale",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (scaleProperty != null && scaleProperty.CanWrite)
                {
                    scaleProperty.SetValue(gameView, 1f, null);
                }

                gameView.Repaint();
                Debug.Log($"[GameViewSetup] Game view switched to {TargetWidth}x{TargetHeight} at 1:1 (index {index}).");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameViewSetup] Could not configure the Game view automatically: {e.Message}");
            }
        }
    }
}

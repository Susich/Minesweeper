using System.IO;
using UnityEngine;
using UnityEditor;

namespace Minesweeper.Editor
{
    public static class GameViewScreenshot
    {
        [MenuItem("Tools/Capture Game Screenshot %&s")] // Shortcut: Ctrl+Alt+S
        public static void CaptureScreenshot()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "screenshot.png");
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[Screenshot] Saved Game view screenshot to: {path}");
        }

        [MenuItem("Tools/Capture Scene & Game View")]
        public static void CaptureSceneView()
        {
            if (SceneView.lastActiveSceneView != null)
            {
                var camera = SceneView.lastActiveSceneView.camera;
                if (camera != null)
                {
                    int width = (int)SceneView.lastActiveSceneView.position.width;
                    int height = (int)SceneView.lastActiveSceneView.position.height;
                    var rt = new RenderTexture(width, height, 24);
                    camera.targetTexture = rt;
                    camera.Render();
                    RenderTexture.active = rt;

                    var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                    tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    tex.Apply();

                    camera.targetTexture = null;
                    RenderTexture.active = null;
                    Object.DestroyImmediate(rt);

                    byte[] bytes = tex.EncodeToPNG();
                    Object.DestroyImmediate(tex);

                    string path = Path.Combine(Directory.GetCurrentDirectory(), "scene_screenshot.png");
                    File.WriteAllBytes(path, bytes);
                    Debug.Log($"[Screenshot] Saved Scene view screenshot to: {path}");
                }
            }

            CaptureScreenshot();
        }
    }
}

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Minesweeper.Core;
using Minesweeper.Storage;
using Minesweeper.UI;

namespace Minesweeper.Editor
{
    /// <summary>
    /// Builds the Cyber HUD scene and renders a PNG preview of the given state so the
    /// layout can be compared against the Stitch mockups.
    /// </summary>
    public static class UIPreviewCapture
    {
        private const int PreviewWidth = 1600;
        private const int PreviewHeight = 900;

        /// <summary>Captures every HUD screen into the UI_Preview folder next to the project.</summary>
        [MenuItem("Tools/Capture UI Previews")]
        public static void CaptureAllPreviews()
        {
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "UI_Preview"));
            Directory.CreateDirectory(dir);

            var states = new[] { "game", "settings", "leaderboard", "victory", "defeat", "help" };
            CyberMinesweeperSceneBuilder.BuildScene();

            foreach (var s in states)
            {
                Capture(s, Path.Combine(dir, "preview_" + s + ".png"), buildScene: false);
            }
        }

        /// <summary>Command line entry point: -captureState game|settings|leaderboard|victory|defeat|all</summary>
        public static void CaptureFromCommandLine()
        {
            string state = "game";
            foreach (var arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("-captureState="))
                {
                    state = arg.Substring("-captureState=".Length);
                }
            }

            string outDir = ".";
            foreach (var arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("-captureOut="))
                {
                    outDir = arg.Substring("-captureOut=".Length);
                }
            }

            if (state == "all")
            {
                var states = new[] { "game", "settings", "leaderboard", "victory", "defeat", "help" };
                CyberMinesweeperSceneBuilder.BuildScene();
                foreach (var s in states)
                {
                    Capture(s, System.IO.Path.Combine(outDir, "preview_" + s + ".png"), buildScene: false);
                }
            }
            else
            {
                Capture(state, System.IO.Path.Combine(outDir, "preview_" + state + ".png"));
            }

            EditorApplication.Exit(0);
        }

        public static void Capture(string state, string outputPath, bool buildScene = true)
        {
            if (buildScene)
            {
                CyberMinesweeperSceneBuilder.BuildScene();
            }

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid()) return;

            // Populate a representative game state so the preview is not empty.
            PopulateState(state);

            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[UIPreviewCapture] Canvas not found.");
                return;
            }

            var camGo = new GameObject("PreviewCamera", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(11f / 255f, 14f / 255f, 23f / 255f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.transform.position = new Vector3(0, 0, -10);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            var prevRenderMode = canvas.renderMode;
            var prevCamera = canvas.worldCamera;
            var prevPlane = canvas.planeDistance;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 10f;

            var rt = new RenderTexture(PreviewWidth, PreviewHeight, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;

            var tex = new Texture2D(PreviewWidth, PreviewHeight, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, PreviewWidth, PreviewHeight), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            canvas.renderMode = prevRenderMode;
            canvas.worldCamera = prevCamera;
            canvas.planeDistance = prevPlane;
            Object.DestroyImmediate(camGo);

            var bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(outputPath, bytes);
            Debug.Log($"[UIPreviewCapture] Saved '{outputPath}' ({state})");
        }

        private static void PopulateState(string state)
        {
            // Always hide every modal first so only the requested one is visible.
            HideModal<SettingsModal>();
            HideModal<LeaderboardsModal>();
            HideModal<VictoryModal>();
            HideModal<DefeatModal>();

            switch (state)
            {
                case "settings":
                    ShowModal<SettingsModal>(m => m.Show());
                    break;
                case "leaderboard":
                    ShowModal<LeaderboardsModal>(m => m.Show(DifficultyLevel.Intermediate));
                    PopulateLeaderboardPreview();
                    break;
                case "victory":
                    ShowModal<VictoryModal>(m => m.Show());
                    break;
                case "defeat":
                    ShowModal<DefeatModal>(m => m.Show());
                    break;
                case "help":
                    ShowModal<HelpAboutModal>(m => m.Show(false));
                    break;
                default:
                    PopulateBoard();
                    break;
            }
        }

        private static void HideModal<T>() where T : MonoBehaviour
        {
            var modal = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (modal == null) return;
            // The script sits on the modal root; the visible backdrop is its child.
            var backdrop = modal.transform.Find("Backdrop");
            if (backdrop != null) backdrop.gameObject.SetActive(false);
        }

        private static void ShowModal<T>(System.Action<T> action) where T : MonoBehaviour
        {
            var modal = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (modal != null) action(modal);
        }

        /// <summary>
        /// The leaderboard needs a live GameController to read the saved scores, which does not
        /// exist in edit mode. The rows are built through the same private factory the runtime
        /// uses so the preview shows the real layout.
        /// </summary>
        private static void PopulateLeaderboardPreview()
        {
            var modal = Object.FindFirstObjectByType<LeaderboardsModal>(FindObjectsInactive.Include);
            if (modal == null) return;

            var type = typeof(LeaderboardsModal);
            var rowsField = type.GetField("_rowsContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var createRow = type.GetMethod("CreateFallbackRow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rowsField == null || createRow == null) return;

            var rows = rowsField.GetValue(modal) as Transform;
            if (rows == null) return;

            // Clear rows created by the modal itself so the preview is not doubled.
            foreach (Transform child in rows)
            {
                Object.DestroyImmediate(child.gameObject);
            }

            string date = System.DateTime.UtcNow.ToString("yyyy.MM.dd");
            var entries = new[]
            {
                new HighScoreEntry("CYBER_VIPER", 28.5f, date, 99.0f, 4.50f),
                new HighScoreEntry("NEXUS_9", 35.2f, date, 97.8f, 4.10f),
                new HighScoreEntry("GHOST_RUNNER", 42.6f, date, 96.5f, 3.80f),
                new HighScoreEntry("ZERO_COOL", 51.0f, date, 94.2f, 3.20f),
                new HighScoreEntry("VOID_WALKER", 63.4f, date, 92.1f, 2.90f),
            };

            for (int i = 0; i < entries.Length; i++)
            {
                var row = createRow.Invoke(modal, new object[] { i + 1, entries[i] }) as GameObject;
                if (row != null) row.SetActive(true);
            }
        }

        /// <summary>Fills the grid with a deterministic, mockup-like board of numbers and flags.</summary>
        private static void PopulateBoard()
        {
            var boardView = Object.FindFirstObjectByType<BoardGridView>(FindObjectsInactive.Include);
            if (boardView == null) return;

            const int width = 16;
            const int height = 16;
            boardView.BuildGrid(width, height);

            var cells = boardView.GetComponentsInChildren<CellView>(true);
            if (cells == null) return;

            // Deterministic pseudo-random pattern that resembles the mockup screenshot.
            foreach (var cell in cells)
            {
                if (cell == null) continue;
                int x = cell.X;
                int y = cell.Y;

                var data = new CellData(x, y);
                int seed = (x * 73856093) ^ (y * 19349663);
                seed = (seed & 0x7fffffff) % 100;

                if (seed < 38)
                {
                    data.State = CellState.Revealed;
                    data.AdjacentMines = (seed % 3) + 1;
                }
                else if (seed < 43)
                {
                    data.State = CellState.Flagged;
                }
                else
                {
                    data.State = CellState.Closed;
                }

                cell.UpdateVisuals(data);
            }
        }
    }
}

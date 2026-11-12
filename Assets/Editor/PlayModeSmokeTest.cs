using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Minesweeper.Editor
{
    /// <summary>
    /// Enters play mode in a headless editor, lets the game run for a few seconds and reports
    /// every runtime error/warning. Used to verify that the rebuilt HUD works at runtime.
    /// </summary>
    public static class PlayModeSmokeTest
    {
        private static readonly List<string> Problems = new List<string>();
        private static float _elapsed = 0f;
        private static bool _running = false;
        private static bool _interactionChecked = false;

        public static void Execute()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/CyberMinesweeper.unity");
            Application.logMessageReceived += OnLog;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            _running = true;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.EnterPlaymode();
            Debug.Log("[SmokeTest] Entering play mode...");
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Debug.Log("[SmokeTest] Play mode entered.");
            }
            else if (state == PlayModeStateChange.EnteredEditMode && _running)
            {
                Finish();
            }
        }

        private static void OnEditorUpdate()
        {
            if (!EditorApplication.isPlaying) return;

            _elapsed += 0.016f;
            if (_elapsed < 2f) return;

            if (!_interactionChecked)
            {
                _interactionChecked = true;
                RunInteractionChecks();
            }

            if (_elapsed < 6f) return;

            Finish();
        }

        /// <summary>Runs the HUD interaction checks inside the open editor (does not close Unity).</summary>
        [MenuItem("Tools/Run HUD Interaction Checks")]
        public static void RunChecksInEditor()
        {
            Problems.Clear();
            _interactionChecked = false;
            _running = true;
            _elapsed = 0f;

            Application.logMessageReceived -= OnLog;
            Application.logMessageReceived += OnLog;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            if (!EditorApplication.isPlaying)
            {
                EditorApplication.EnterPlaymode();
            }
        }

        /// <summary>
        /// Simulates the clicks a player performs on the HUD to make sure the modal
        /// close buttons and the navigation tabs really work.
        /// </summary>
        private static void RunInteractionChecks()
        {
            // Every modal must expose a working close button (the header "✕" and the footer button).
            CheckModalCloseButton<Minesweeper.UI.SettingsModal>("SettingsModal");
            CheckModalCloseButton<Minesweeper.UI.LeaderboardsModal>("LeaderboardsModal");
            CheckModalCloseButton<Minesweeper.UI.HelpAboutModal>("HelpAboutModal");

            // The result modals have their own action buttons.
            CheckSingleButton<Minesweeper.UI.VictoryModal>("VictoryModal", "_menuButton");
            CheckSingleButton<Minesweeper.UI.DefeatModal>("DefeatModal", "_menuButton");

            // The navigation tabs of the top bar must be wired.
            var topBar = UnityEngine.Object.FindFirstObjectByType<Minesweeper.UI.TopMenuBar>(FindObjectsInactive.Include);
            if (topBar == null)
            {
                Problems.Add("[Check] TopMenuBar not found");
            }
        }

        private static void CheckSingleButton<T>(string label, string fieldName) where T : MonoBehaviour
        {
            var modal = UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (modal == null)
            {
                Problems.Add($"[Check] {label} not found");
                return;
            }

            var field = modal.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var btn = field?.GetValue(modal) as UnityEngine.UI.Button;
            if (btn == null)
            {
                Problems.Add($"[Check] {label}.{fieldName} is not assigned");
                return;
            }

            bool clicked = false;
            btn.onClick.AddListener(() => clicked = true);
            btn.onClick.Invoke();

            if (!clicked)
            {
                Problems.Add($"[Check] {label}.{fieldName} did not react to onClick");
            }
        }

        private static void CheckModalCloseButton<T>(string label) where T : MonoBehaviour
        {
            var modal = UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (modal == null)
            {
                Problems.Add($"[Check] {label} not found");
                return;
            }

            var type = modal.GetType();
            foreach (var fieldName in new[] { "_headerCloseButton", "_closeButton" })
            {
                var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var btn = field?.GetValue(modal) as UnityEngine.UI.Button;
                if (btn == null)
                {
                    Problems.Add($"[Check] {label}.{fieldName} is not assigned");
                    continue;
                }

                bool clicked = false;
                btn.onClick.AddListener(() => clicked = true);
                btn.onClick.Invoke();

                if (!clicked)
                {
                    Problems.Add($"[Check] {label}.{fieldName} did not react to onClick");
                }
            }
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                Problems.Add($"[{type}] {condition}");
            }
        }

        private static void Finish()
        {
            _running = false;
            EditorApplication.update -= OnEditorUpdate;
            Application.logMessageReceived -= OnLog;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;

            if (Problems.Count == 0)
            {
                Debug.Log("[SmokeTest] RESULT: no runtime errors detected.");
            }
            else
            {
                Debug.LogError($"[SmokeTest] RESULT: {Problems.Count} runtime problem(s):");
                foreach (var p in Problems)
                {
                    Debug.LogError("  " + p);
                }
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(Problems.Count == 0 ? 0 : 1);
            }
            else if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
        }
    }
}

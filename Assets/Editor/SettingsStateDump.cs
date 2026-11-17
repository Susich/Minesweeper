using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Minesweeper.Core;
using Minesweeper.UI;

namespace Minesweeper.Editor
{
    /// <summary>Temporary play-mode dump of the settings modal state.</summary>
    public static class SettingsStateDump
    {
        private const string ActiveKey = "SettingsStateDump.Active";
        private const string PhaseKey = "SettingsStateDump.Phase";

        private static double _start;

        public static void Execute()
        {
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetInt(PhaseKey, 0);
            EditorSceneManager.OpenScene("Assets/Scenes/CyberMinesweeper.unity");
            Debug.Log("[DUMP] entering play mode");
            EditorApplication.EnterPlaymode();
        }

        [InitializeOnLoadMethod]
        private static void OnDomainLoad()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            if (EditorApplication.isPlaying)
            {
                _start = EditorApplication.timeSinceStartup;
                SessionState.SetInt(PhaseKey, 0);
            }
        }

        private static void OnUpdate()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            if (!EditorApplication.isPlaying) return;

            if (_start <= 0)
            {
                _start = EditorApplication.timeSinceStartup;
                return;
            }

            double t = EditorApplication.timeSinceStartup - _start;
            int phase = SessionState.GetInt(PhaseKey, 0);

            if (phase == 0 && t > 1.2)
            {
                SessionState.SetInt(PhaseKey, 1);
                try { DumpBeforeShow(); }
                catch (Exception e) { Debug.LogError("[DUMP] before show: " + e); }
            }
            else if (phase == 1 && t > 1.4)
            {
                SessionState.SetInt(PhaseKey, 2);
                try { OpenSettings(); }
                catch (Exception e) { Debug.LogError("[DUMP] open: " + e); }
            }
            else if (phase == 2 && t > 2.0)
            {
                SessionState.SetInt(PhaseKey, 3);
                try { DumpAfterShow(); }
                catch (Exception e) { Debug.LogError("[DUMP] after show: " + e); }
                try { Capture("settings_dump.png"); }
                catch (Exception e) { Debug.LogWarning("[DUMP] capture: " + e); }
                Finish();
            }
        }

        private static void DumpBeforeShow()
        {
            var gc = GameController.Instance;
            if (gc == null) { Debug.LogError("[DUMP] no GameController"); return; }
            var d = gc.CurrentDifficulty;
            Debug.Log($"[DUMP] current: level={d.Level} {d.Width}x{d.Height} mines={d.Mines}");
            Debug.Log($"[DUMP] settings: level={gc.Settings.Difficulty} custom={gc.Settings.CustomWidth}x{gc.Settings.CustomHeight}/{gc.Settings.CustomMines} muted={gc.Settings.IsMuted}");
        }

        private static void OpenSettings()
        {
            var modal = UnityEngine.Object.FindFirstObjectByType<SettingsModal>(FindObjectsInactive.Include);
            Debug.Log($"[DUMP] SettingsModal found={modal != null} active={modal != null && modal.IsOpen}");
            if (modal != null) modal.Show();
        }

        private static void DumpAfterShow()
        {
            var modal = UnityEngine.Object.FindFirstObjectByType<SettingsModal>(FindObjectsInactive.Include);
            if (modal == null) { Debug.LogError("[DUMP] no modal"); return; }

            var t = typeof(SettingsModal);
            object Field(string name) => t.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)?.GetValue(modal);

            Debug.Log($"[DUMP] selectedLevel={Field("_selectedLevel")} custom={Field("_customW")}x{Field("_customH")}/{Field("_customM")}");

            var mines = Field("_minesInput") as TMP_InputField;
            Debug.Log($"[DUMP] input mines='{(mines != null ? mines.text : "null")}' focused={(mines != null && mines.isFocused)} textComp='{(mines != null && mines.textComponent != null ? mines.textComponent.text : "null")}'");
            var wIn = Field("_widthInput") as TMP_InputField;
            var hIn = Field("_heightInput") as TMP_InputField;
            Debug.Log($"[DUMP] input w='{(wIn != null ? wIn.text : "null")}' h='{(hIn != null ? hIn.text : "null")}'");
            Debug.Log($"[DUMP] input wComp='{(wIn != null && wIn.textComponent != null ? wIn.textComponent.text : "null")}' hComp='{(hIn != null && hIn.textComponent != null ? hIn.textComponent.text : "null")}'");

            var mS = Field("_minesSlider") as Slider;
            var wS = Field("_widthSlider") as Slider;
            var hS = Field("_heightSlider") as Slider;
            Debug.Log($"[DUMP] sliders w={SliderInfo(wS)} h={SliderInfo(hS)} m={SliderInfo(mS)}");

            var density = Field("_densityText") as TextMeshProUGUI;
            var total = Field("_totalCellsText") as TextMeshProUGUI;
            Debug.Log($"[DUMP] density='{(density != null ? density.text : "null")}' total='{(total != null ? total.text : "null")}'");

            DumpCard("beginner", Field("_btnBeginner") as Button, Field("_indicatorBeginner") as Image);
            DumpCard("intermediate", Field("_btnIntermediate") as Button, Field("_indicatorIntermediate") as Image);
            DumpCard("expert", Field("_btnExpert") as Button, Field("_indicatorExpert") as Image);
            DumpCard("custom", Field("_btnCustom") as Button, Field("_indicatorCustom") as Image);

            var es = EventSystem.current;
            Debug.Log($"[DUMP] event selected={(es != null && es.currentSelectedGameObject != null ? es.currentSelectedGameObject.name : "none")}");
            Debug.Log($"[DUMP] muteToggleRemoved={(Field("_muteToggle") == null)}");
        }

        private static string SliderInfo(Slider s)
        {
            if (s == null) return "null";
            return $"{s.value:F1} of {s.minValue:F0}-{s.maxValue:F0}";
        }

        private static void DumpCard(string name, Button btn, Image indicator)
        {
            var img = btn != null ? btn.GetComponent<Image>() : null;
            string sprite = img != null && img.sprite != null ? img.sprite.name : "null";
            string color = indicator != null ? indicator.color.ToString() : "null";
            Debug.Log($"[DUMP] card {name}: btn={(btn != null)} sprite={sprite} imgColor={(img != null ? img.color.ToString() : "null")} indicator={color}");
        }

        private static void Capture(string fileName)
        {
            var canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var camGo = new GameObject("DumpCaptureCamera", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(11f / 255f, 14f / 255f, 23f / 255f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            var prevMode = canvas.renderMode;
            var prevCamera = canvas.worldCamera;
            var prevPlane = canvas.planeDistance;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 10f;

            var rt = new RenderTexture(1920, 1080, 24);
            cam.targetTexture = rt;
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(camGo);
            canvas.renderMode = prevMode;
            canvas.worldCamera = prevCamera;
            canvas.planeDistance = prevPlane;

            string dir = @"C:\Users\sus\AppData\Local\Temp\opencode";
            File.WriteAllBytes(Path.Combine(dir, fileName), tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            Debug.Log("[DUMP] saved " + fileName);
        }

        private static void Finish()
        {
            SessionState.SetBool(ActiveKey, false);
            EditorApplication.update -= OnUpdate;
            EditorApplication.isPlaying = false;
            Debug.Log("[DUMP] done");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
    }
}

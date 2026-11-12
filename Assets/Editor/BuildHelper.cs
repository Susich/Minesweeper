using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Minesweeper.Editor
{
    public static class BuildHelper
    {
        [MenuItem("Tools/Build Windows Standalone")]
        public static void BuildStandalonePlayer()
        {
            // The player is named after the product so the release archive and the
            // window title always match (Player Settings -> Product Name).
            string exeName = string.IsNullOrEmpty(PlayerSettings.productName)
                ? "Minesweeper"
                : PlayerSettings.productName;
            string buildPath = "Builds/" + exeName + ".exe";
            string[] scenes = new string[] { "Assets/Scenes/CyberMinesweeper.unity" };

            // HI-002: the product must render through the DirectX 11 graphics API.
            ConfigureDirectX11();

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"Starting standalone build to {buildPath}...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize / 1024 / 1024} MB, time: {summary.totalTime.TotalSeconds:F1}s");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            else
            {
                Debug.LogError($"Build failed with result: {summary.result}, errors: {summary.totalErrors}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        /// <summary>HI-002: для рендеринга используется графический API DirectX 11.</summary>
        private static void ConfigureDirectX11()
        {
            var target = BuildTarget.StandaloneWindows64;
            PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);
            PlayerSettings.SetGraphicsAPIs(target, new[] { GraphicsDeviceType.Direct3D11 });
            Debug.Log("[BuildHelper] Graphics API for Windows player set to Direct3D11.");
        }
    }
}

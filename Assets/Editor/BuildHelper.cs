using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Minesweeper.Editor
{
    public static class BuildHelper
    {
        [MenuItem("Tools/Build Windows Standalone")]
        public static void BuildStandalonePlayer()
        {
            string buildPath = "Builds/CyberMinesweeper.exe";
            string[] scenes = new string[] { "Assets/Scenes/CyberMinesweeper.unity" };

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
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"Build failed with result: {summary.result}, errors: {summary.totalErrors}");
                EditorApplication.Exit(1);
            }
        }
    }
}

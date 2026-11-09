using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace Minesweeper.Tests
{
    public static class RunTestsInBatch
    {
        [MenuItem("Tools/Run Unit Tests")]
        public static void RunFromMenu()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            Debug.Log("=== STARTING MINESWEEPER CORE UNIT TESTS ===");
            var testClass = new MinesweeperCoreTests();
            var methods = typeof(MinesweeperCoreTests).GetMethods(BindingFlags.Public | BindingFlags.Instance);

            int passed = 0;
            int failed = 0;

            foreach (var m in methods)
            {
                if (m.GetCustomAttribute<TestAttribute>() != null)
                {
                    try
                    {
                        m.Invoke(testClass, null);
                        Debug.Log($"[PASS] {m.Name}");
                        passed++;
                    }
                    catch (Exception ex)
                    {
                        var inner = ex.InnerException ?? ex;
                        Debug.LogError($"[FAIL] {m.Name}: {inner.Message}\n{inner.StackTrace}");
                        failed++;
                    }
                }
            }

            Debug.Log($"=== TEST SUMMARY: {passed} PASSED, {failed} FAILED ===");
        }

        public static void Execute()
        {
            Debug.Log("=== STARTING MINESWEEPER CORE UNIT TESTS ===");
            var testClass = new MinesweeperCoreTests();
            var methods = typeof(MinesweeperCoreTests).GetMethods(BindingFlags.Public | BindingFlags.Instance);

            int passed = 0;
            int failed = 0;

            foreach (var m in methods)
            {
                if (m.GetCustomAttribute<TestAttribute>() != null)
                {
                    try
                    {
                        m.Invoke(testClass, null);
                        Debug.Log($"[PASS] {m.Name}");
                        passed++;
                    }
                    catch (Exception ex)
                    {
                        var inner = ex.InnerException ?? ex;
                        Debug.LogError($"[FAIL] {m.Name}: {inner.Message}\n{inner.StackTrace}");
                        failed++;
                    }
                }
            }

            Debug.Log($"=== TEST SUMMARY: {passed} PASSED, {failed} FAILED ===");
            if (failed > 0)
            {
                EditorApplication.Exit(1);
            }
            else
            {
                EditorApplication.Exit(0);
            }
        }
    }
}

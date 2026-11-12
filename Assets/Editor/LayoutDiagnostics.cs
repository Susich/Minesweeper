using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Minesweeper.Editor
{
    /// <summary>Logs the computed RectTransforms of the tactical deck so layout issues can be diagnosed.</summary>
    public static class LayoutDiagnostics
    {
        private static string GetWorldCorners2D(this RectTransform rt)
        {
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            return $"({corners[0].x:F0},{corners[0].y:F0})-({corners[2].x:F0},{corners[2].y:F0})";
        }

        [MenuItem("Tools/Diagnose Deck Layout")]
        public static void DumpDeck()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Deck layout ===");

            var deck = GameObject.Find("TacticalDeck");
            if (deck == null) { Debug.Log(sb.ToString()); return; }

            var deckRt = deck.GetComponent<RectTransform>();
            sb.AppendLine($"Deck rect: {deckRt.rect.size} anchoredPos {deckRt.anchoredPosition}");

            var vg = deck.GetComponent<VerticalLayoutGroup>();
            if (vg != null)
            {
                sb.AppendLine($"VLG padding {vg.padding} spacing {vg.spacing} " +
                              $"controlW {vg.childControlWidth} controlH {vg.childControlHeight} " +
                              $"forceW {vg.childForceExpandWidth} forceH {vg.childForceExpandHeight}");
            }

            for (int i = 0; i < deck.transform.childCount; i++)
            {
                var child = deck.transform.GetChild(i) as RectTransform;
                if (child == null) continue;
                var le = child.GetComponent<LayoutElement>();
                sb.AppendLine($"  [{i}] {child.name,-22} rect {child.rect.size} " +
                              $"anchoredPos {child.anchoredPosition} " +
                              $"prefH {(le != null ? le.preferredHeight.ToString() : "-")} " +
                              $"minH {(le != null ? le.minHeight.ToString() : "-")}");
            }

            var header = deck.transform.Find("HeaderStatusDeck");
            if (header != null)
            {
                var hr = header as RectTransform;
                sb.AppendLine($"  HeaderStatusDeck rect {hr.rect.size} anchoredPos {hr.anchoredPosition} " +
                              $"worldCorners {hr.GetWorldCorners2D()}");
                for (int i = 0; i < header.childCount; i++)
                {
                    var c = header.GetChild(i) as RectTransform;
                    if (c == null) continue;
                    sb.AppendLine($"     - {c.name,-22} rect {c.rect.size} anchoredPos {c.anchoredPosition} " +
                                  $"world {c.GetWorldCorners2D()}");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}

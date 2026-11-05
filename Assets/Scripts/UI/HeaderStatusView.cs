using UnityEngine;
using TMPro;
using Minesweeper.Core;

namespace Minesweeper.UI
{
    public class HeaderStatusView : MonoBehaviour
    {
        [Header("LED Counters")]
        [SerializeField] private TextMeshProUGUI _mineCounterText;
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Telemetry Labels")]
        [SerializeField] private TextMeshProUGUI _matrixTitleText;
        [SerializeField] private TextMeshProUGUI _gridDensityText;
        [SerializeField] private TextMeshProUGUI _chanceText;

        private void Start()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnMineCounterChanged += UpdateMineDisplay;
                GameController.Instance.OnTimerChanged += UpdateTimerDisplay;
                GameController.Instance.OnGameInitialized += UpdateTelemetry;

                UpdateMineDisplay(GameController.Instance.RemainingMines);
                UpdateTimerDisplay(GameController.Instance.ElapsedSeconds);
                if (GameController.Instance.CurrentDifficulty != null)
                {
                    UpdateTelemetry(GameController.Instance.CurrentDifficulty);
                }
            }
        }

        private void OnDestroy()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnMineCounterChanged -= UpdateMineDisplay;
                GameController.Instance.OnTimerChanged -= UpdateTimerDisplay;
                GameController.Instance.OnGameInitialized -= UpdateTelemetry;
            }
        }

        public void UpdateMineDisplay(int remainingMines)
        {
            if (_mineCounterText == null) return;
            if (remainingMines < 0)
            {
                // Format negative counter, e.g. -01, -12
                _mineCounterText.text = $"-{Mathf.Abs(remainingMines):D2}";
            }
            else
            {
                _mineCounterText.text = $"{Mathf.Clamp(remainingMines, 0, 999):D3}";
            }
        }

        public void UpdateTimerDisplay(int seconds)
        {
            if (_timerText == null) return;
            _timerText.text = $"{Mathf.Clamp(seconds, 0, 999):D3}";
        }

        public void UpdateTelemetry(GameDifficulty diff)
        {
            if (diff == null) return;

            if (_gridDensityText != null)
            {
                string densityLabel = diff.Level == DifficultyLevel.Beginner ? "LIGHT" :
                                      diff.Level == DifficultyLevel.Intermediate ? "MEDIUM" : "DENSE";
                _gridDensityText.text = $"GRID: {diff.Width}x{diff.Height} [{densityLabel}]";
            }

            if (_chanceText != null)
            {
                float clearPercent = (1f - diff.MineDensity) * 100f;
                _chanceText.text = $"CHANCE: {clearPercent:F1}% CLEAR";
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minesweeper.Core;

namespace Minesweeper.UI
{
    public class HeaderStatusView : MonoBehaviour
    {
        [Header("LED Top Counters")]
        [SerializeField] private TextMeshProUGUI _mineCounterText;
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Secondary Action Buttons")]
        [SerializeField] private Button _btnRestart;
        [SerializeField] private Button _btnPause;
        [SerializeField] private Button _btnHint;
        [SerializeField] private TextMeshProUGUI _pauseText;
        [SerializeField] private Image _pauseIcon;

        [Header("Bottom Deck Status Info")]
        [SerializeField] private TextMeshProUGUI _flagsCountText;
        [SerializeField] private TextMeshProUGUI _bottomDifficultyText;
        [SerializeField] private Image _statusDot;
        [SerializeField] private TextMeshProUGUI _statusLabel;

        [Header("Sprites for Toggles")]
        public Sprite PauseSprite;
        public Sprite PlaySprite;

        private static readonly Color ColorInProgress = new Color(0f, 242f / 255f, 254f / 255f);     // #00F2FE Cyan
        private static readonly Color ColorDefeat = new Color(244f / 255f, 63f / 255f, 94f / 255f);     // #F43F5E Rose
        private static readonly Color ColorVictory = new Color(52f / 255f, 211f / 255f, 153f / 255f);   // #34D399 Emerald
        private static readonly Color ColorPaused = new Color(251f / 255f, 191f / 255f, 36f / 255f);    // #FBBF24 Amber

        private void Awake()
        {
            if (_btnRestart != null) _btnRestart.onClick.AddListener(OnRestartClicked);
            if (_btnPause != null) _btnPause.onClick.AddListener(OnPauseClicked);
            if (_btnHint != null) _btnHint.onClick.AddListener(OnHintClicked);
        }

        private void Start()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnMineCounterChanged += UpdateMineDisplay;
                GameController.Instance.OnTimerChanged += UpdateTimerDisplay;
                GameController.Instance.OnGameInitialized += UpdateDifficultyDisplay;
                GameController.Instance.OnGameStatusChanged += UpdateGameStatus;

                UpdateMineDisplay(GameController.Instance.RemainingMines);
                UpdateTimerDisplay(GameController.Instance.ElapsedSeconds);
                if (GameController.Instance.CurrentDifficulty != null)
                {
                    UpdateDifficultyDisplay(GameController.Instance.CurrentDifficulty);
                }
                UpdateGameStatus(GameController.Instance.Status);
            }
        }

        private void OnDestroy()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnMineCounterChanged -= UpdateMineDisplay;
                GameController.Instance.OnTimerChanged -= UpdateTimerDisplay;
                GameController.Instance.OnGameInitialized -= UpdateDifficultyDisplay;
                GameController.Instance.OnGameStatusChanged -= UpdateGameStatus;
            }
        }

        public void UpdateMineDisplay(int remainingMines)
        {
            if (_mineCounterText != null)
            {
                if (remainingMines < 0)
                {
                    _mineCounterText.text = $"-{Mathf.Abs(remainingMines):D2}";
                }
                else
                {
                    _mineCounterText.text = $"{Mathf.Clamp(remainingMines, 0, 999):D3}";
                }
            }

            if (_flagsCountText != null && GameController.Instance != null && GameController.Instance.Board != null)
            {
                int flags = GameController.Instance.Board.FlagsCount;
                int total = GameController.Instance.Board.TotalMines;
                _flagsCountText.text = $"Флаги: <color=white><b>{flags} / {total}</b></color>";
            }
        }

        public void UpdateTimerDisplay(int seconds)
        {
            if (_timerText != null)
            {
                _timerText.text = $"{Mathf.Clamp(seconds, 0, 999):D3}";
            }
        }

        public void UpdateDifficultyDisplay(GameDifficulty diff)
        {
            if (diff == null) return;

            if (_bottomDifficultyText != null)
            {
                _bottomDifficultyText.text = $"Сложность: {diff.Width}×{diff.Height} • {diff.Mines} мин";
            }

            if (_flagsCountText != null && GameController.Instance != null && GameController.Instance.Board != null)
            {
                int flags = GameController.Instance.Board.FlagsCount;
                _flagsCountText.text = $"Флаги: <color=white><b>{flags} / {diff.Mines}</b></color>";
            }
        }

        public void UpdateGameStatus(GameStatus status)
        {
            bool isPaused = status == GameStatus.Paused;
            if (_pauseText != null)
            {
                _pauseText.text = isPaused ? "Продолжить" : "Пауза";
            }
            if (_pauseIcon != null)
            {
                _pauseIcon.sprite = isPaused ? PlaySprite : PauseSprite;
            }

            if (_statusLabel != null && _statusDot != null)
            {
                switch (status)
                {
                    case GameStatus.WaitingForStart:
                    case GameStatus.InProgress:
                        _statusDot.color = ColorInProgress;
                        _statusLabel.text = "В процессе";
                        _statusLabel.color = Color.white;
                        break;
                    case GameStatus.Paused:
                        _statusDot.color = ColorPaused;
                        _statusLabel.text = "Пауза";
                        _statusLabel.color = ColorPaused;
                        break;
                    case GameStatus.Victory:
                        _statusDot.color = ColorVictory;
                        _statusLabel.text = "Партия завершена";
                        _statusLabel.color = Color.white;
                        break;
                    case GameStatus.Defeat:
                        _statusDot.color = ColorDefeat;
                        _statusLabel.text = "Детонация";
                        _statusLabel.color = ColorDefeat;
                        break;
                }
            }
        }

        private void OnRestartClicked()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.RestartGame();
            }
        }

        private void OnPauseClicked()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.TogglePause();
            }
        }

        private void OnHintClicked()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.GiveHint();
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minesweeper.Core;

namespace Minesweeper.UI
{
    public class DefeatModal : MonoBehaviour
    {
        [Header("Modal Root")]
        [SerializeField] private GameObject _panel;

        [Header("Telemetry Texts")]
        [SerializeField] private TextMeshProUGUI _sectorSubtitleText;
        [SerializeField] private TextMeshProUGUI _timeStatText;
        [SerializeField] private TextMeshProUGUI _openedPercentText;
        [SerializeField] private TextMeshProUGUI _remainingMinesText;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _inspectButton;
        [SerializeField] private Button _menuButton;

        private void Awake()
        {
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetryClicked);
            if (_inspectButton != null) _inspectButton.onClick.AddListener(OnInspectClicked);
            if (_menuButton != null) _menuButton.onClick.AddListener(OnMenuClicked);
        }

        private void Start()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnGameStatusChanged += OnGameStatusChanged;
                GameController.Instance.OnBoardReset += Hide;
            }
            Hide();
        }

        private void OnDestroy()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnGameStatusChanged -= OnGameStatusChanged;
                GameController.Instance.OnBoardReset -= Hide;
            }
        }

        private void OnGameStatusChanged(GameStatus status)
        {
            if (status == GameStatus.Defeat)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void Show()
        {
            if (_panel == null) _panel = gameObject;
            _panel.SetActive(true);

            var gc = GameController.Instance;
            if (gc != null)
            {
                int row = gc.LastDetonatedCell != null ? (gc.LastDetonatedCell.Y + 1) : 1;
                int col = gc.LastDetonatedCell != null ? (gc.LastDetonatedCell.X + 1) : 1;

                if (_sectorSubtitleText != null)
                {
                    _sectorSubtitleText.text = $"Сектор подорван на мине <color=#f87171><b>(ряд {row}, колонка {col})</b></color>";
                }

                if (_timeStatText != null)
                {
                    int totalSec = gc.ElapsedSeconds;
                    int minutes = totalSec / 60;
                    int seconds = totalSec % 60;
                    _timeStatText.text = $"{minutes:D2}:{seconds:D2}";
                }

                if (_openedPercentText != null && gc.Board != null)
                {
                    int totalSafe = (gc.Board.Width * gc.Board.Height) - gc.Board.TotalMines;
                    float percent = totalSafe > 0 ? ((float)gc.Board.RevealedSafeCount / totalSafe) * 100f : 0f;
                    _openedPercentText.text = $"{Mathf.Clamp(Mathf.RoundToInt(percent), 0, 100)}%";
                }

                if (_remainingMinesText != null && gc.Board != null)
                {
                    int rem = Mathf.Max(0, gc.Board.TotalMines - gc.Board.FlagsCount);
                    _remainingMinesText.text = rem.ToString();
                }
            }
        }

        public void Hide()
        {
            if (_panel == null) _panel = gameObject;
            _panel.SetActive(false);
        }

        private void OnRetryClicked()
        {
            Hide();
            if (GameController.Instance != null)
            {
                GameController.Instance.RestartGame();
            }
        }

        private void OnInspectClicked()
        {
            Hide();
            // Field remains in Defeat state with all mines visible for inspection
        }

        private void OnMenuClicked()
        {
            Hide();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minesweeper.Core;

namespace Minesweeper.UI
{
    public class VictoryModal : MonoBehaviour
    {
        [Header("Modal Root")]
        [SerializeField] private GameObject _panel;

        [Header("Telemetry Texts")]
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _difficultyText;
        [SerializeField] private TextMeshProUGUI _recordStatusText;

        [Header("Callsign Input")]
        [SerializeField] private TMP_InputField _callsignInput;

        private bool _sanitizing = false;

        [Header("Buttons")]
        [SerializeField] private Button _saveRecordButton;
        [SerializeField] private TextMeshProUGUI _saveButtonText;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _menuButton;

        private void Awake()
        {
            if (_saveRecordButton != null) _saveRecordButton.onClick.AddListener(OnSaveRecordClicked);
            if (_newGameButton != null) _newGameButton.onClick.AddListener(OnNewGameClicked);
            if (_menuButton != null) _menuButton.onClick.AddListener(OnMenuClicked);

            if (_callsignInput != null)
            {
                _callsignInput.characterLimit = 16;
                _callsignInput.onValueChanged.AddListener(OnInputChanged);
                _callsignInput.onEndEdit.AddListener(OnInputCommitted);
            }
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
            if (status == GameStatus.Victory)
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
                if (_timeText != null)
                {
                    _timeText.text = $"{gc.ElapsedSeconds} сек";
                }

                if (_difficultyText != null && gc.CurrentDifficulty != null)
                {
                    _difficultyText.text = $"{gc.CurrentDifficulty.Width}×{gc.CurrentDifficulty.Height} • {gc.CurrentDifficulty.Mines}м";
                }

                bool isRecord = gc.CheckHighScoreEligibility();
                if (_recordStatusText != null)
                {
                    _recordStatusText.text = isRecord ? "Новый!" : "—";
                    _recordStatusText.color = isRecord ? new Color(52f / 255f, 211f / 255f, 153f / 255f) : new Color(148f / 255f, 163f / 255f, 184f / 255f);
                }

                if (_callsignInput != null)
                {
                    string last = gc.Settings.LastPlayerName;
                    _callsignInput.text = string.IsNullOrWhiteSpace(last) ? "Игрок 1" : last;
                }

                if (_saveRecordButton != null)
                {
                    _saveRecordButton.interactable = true;
                }
                if (_saveButtonText != null)
                {
                    _saveButtonText.text = "Сохранить результат";
                }
            }
        }

        public void Hide()
        {
            if (_panel == null) _panel = gameObject;
            _panel.SetActive(false);
        }

        private void OnInputChanged(string text)
        {
            // FR-023: допустимы только буквы, цифры, дефис, подчёркивание и пробелы.
            if (_callsignInput != null)
            {
                string clean = SanitizeName(text, trimEnd: false);
                if (clean != text)
                {
                    _sanitizing = true;
                    _callsignInput.text = clean;
                    _sanitizing = false;
                }
                text = clean;
            }

            if (_saveRecordButton != null)
            {
                // Блокируем подтверждение при пустом поле или строке из одних пробелов (FR-023).
                _saveRecordButton.interactable = !string.IsNullOrWhiteSpace(text);
            }
        }

        private void OnInputCommitted(string text)
        {
            if (_callsignInput == null || _sanitizing) return;

            string clean = SanitizeName(text, trimEnd: true);
            if (clean != text)
            {
                _sanitizing = true;
                _callsignInput.text = clean;
                _sanitizing = false;
            }
        }

        /// <summary>Фильтрует имя оператора по правилам FR-023.</summary>
        private static string SanitizeName(string raw, bool trimEnd)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            var sb = new System.Text.StringBuilder(16);
            foreach (char c in raw)
            {
                if (sb.Length >= 16) break;
                if (char.IsLetter(c) || char.IsDigit(c) || c == '-' || c == '_' || c == ' ')
                {
                    sb.Append(c);
                }
            }

            string result = sb.ToString();
            result = result.TrimStart();                 // пробелы в начале запрещены
            if (trimEnd) result = result.TrimEnd();       // пробелы в конце запрещены
            return result;
        }

        private void OnSaveRecordClicked()
        {
            var gc = GameController.Instance;
            if (gc != null)
            {
                string name = _callsignInput != null ? _callsignInput.text.Trim() : "Player";
                if (string.IsNullOrWhiteSpace(name)) name = "Player";

                gc.RegisterHighScore(name);

                if (_saveRecordButton != null)
                {
                    _saveRecordButton.interactable = false;
                }
                if (_saveButtonText != null)
                {
                    _saveButtonText.text = "✓ Сохранено!";
                }

                // Open Leaderboards modal
                var lb = FindFirstObjectByType<LeaderboardsModal>(FindObjectsInactive.Include);
                if (lb != null)
                {
                    lb.Show(gc.CurrentDifficulty.Level);
                }
            }
        }

        private void OnNewGameClicked()
        {
            Hide();
            if (GameController.Instance != null)
            {
                GameController.Instance.RestartGame();
            }
        }

        private void OnMenuClicked()
        {
            Hide();
        }
    }
}

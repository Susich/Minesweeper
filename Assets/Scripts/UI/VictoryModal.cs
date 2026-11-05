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
        [SerializeField] private TextMeshProUGUI _accuracyText;
        [SerializeField] private TextMeshProUGUI _efficiencyText;
        [SerializeField] private GameObject _recordBanner;

        [Header("Callsign Input")]
        [SerializeField] private TMP_InputField _callsignInput;

        [Header("Buttons")]
        [SerializeField] private Button _saveRecordButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _menuButton;

        private void Awake()
        {
            if (_saveRecordButton != null) _saveRecordButton.onClick.AddListener(OnSaveRecordClicked);
            if (_newGameButton != null) _newGameButton.onClick.AddListener(OnNewGameClicked);
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
                    _timeText.text = $"{gc.ElapsedTime:F2} c";

                if (_difficultyText != null && gc.CurrentDifficulty != null)
                    _difficultyText.text = $"{gc.CurrentDifficulty.Width}x{gc.CurrentDifficulty.Height} [{gc.CurrentDifficulty.Mines} МИН]";

                if (_accuracyText != null)
                    _accuracyText.text = $"{gc.CalculateAccuracy():F1}%";

                if (_efficiencyText != null)
                    _efficiencyText.text = $"{gc.CalculateEfficiency():F2} ЗВУ/с";

                bool isEligible = gc.CheckHighScoreEligibility();
                if (_recordBanner != null)
                {
                    _recordBanner.SetActive(isEligible);
                }

                if (_callsignInput != null)
                {
                    _callsignInput.text = gc.Settings.LastPlayerName ?? "CYBER_OPERATOR";
                    _saveRecordButton.interactable = isEligible;
                }
            }
        }

        public void Hide()
        {
            if (_panel == null) _panel = gameObject;
            _panel.SetActive(false);
        }

        private void OnSaveRecordClicked()
        {
            var gc = GameController.Instance;
            if (gc != null)
            {
                string name = _callsignInput != null ? _callsignInput.text : "CYBER_OPERATOR";
                gc.RegisterHighScore(name);
                if (_saveRecordButton != null) _saveRecordButton.interactable = false;

                // Open Leaderboards modal
                var lb = FindObjectOfType<LeaderboardsModal>(true);
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

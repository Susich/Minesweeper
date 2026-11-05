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
        [SerializeField] private TextMeshProUGUI _pointOfFailureText;
        [SerializeField] private TextMeshProUGUI _accuracyText;
        [SerializeField] private TextMeshProUGUI _falseFlagsText;
        [SerializeField] private TextMeshProUGUI _statusText;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _inspectButton;

        private void Awake()
        {
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetryClicked);
            if (_inspectButton != null) _inspectButton.onClick.AddListener(OnInspectClicked);
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
                int detonatedX = gc.LastDetonatedCell != null ? gc.LastDetonatedCell.X : 0;
                int detonatedY = gc.LastDetonatedCell != null ? gc.LastDetonatedCell.Y : 0;

                if (_pointOfFailureText != null)
                    _pointOfFailureText.text = $"ТОЧКА РАЗРЫВА: GRID_X: {detonatedX} // Y: {detonatedY}";

                if (_accuracyText != null)
                    _accuracyText.text = $"ТОЧНОСТЬ РАЗВЕДКИ: {gc.CalculateAccuracy():F1}% (НЕУДАЧА)";

                // Count false flags
                int falseFlags = 0;
                if (gc.Board != null)
                {
                    for (int x = 0; x < gc.Board.Width; x++)
                    {
                        for (int y = 0; y < gc.Board.Height; y++)
                        {
                            if (gc.Board.GetCell(x, y).State == CellState.FalseFlag)
                                falseFlags++;
                        }
                    }
                }

                if (_falseFlagsText != null)
                    _falseFlagsText.text = $"ЛОЖНЫЕ МАРКИРОВКИ: {falseFlags} КРИТ. ОШИБОК";

                if (_statusText != null)
                    _statusText.text = "СТАТУС ИГРОКА: ПОРАЖЕНИЕ";
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
            // Field remains in Defeat state with all mines revealed for analysis
        }
    }
}

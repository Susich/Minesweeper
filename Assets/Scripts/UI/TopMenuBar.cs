using UnityEngine;
using UnityEngine.UI;
using Minesweeper.Core;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Minesweeper.UI
{
    public class TopMenuBar : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button _btnNewGame;
        [SerializeField] private Button _btnRecords;
        [SerializeField] private Button _btnSettings;
        [SerializeField] private Button _btnHelpRules;
        [SerializeField] private Button _btnHelpAbout;
        [SerializeField] private Button _btnExit;

        [Header("Window Controls")]
        [SerializeField] private Button _btnMinimize;
        [SerializeField] private Button _btnMaximize;
        [SerializeField] private Button _btnClose;

        [Header("Modals References")]
        [SerializeField] private SettingsModal _settingsModal;
        [SerializeField] private LeaderboardsModal _leaderboardsModal;
        [SerializeField] private HelpAboutModal _helpAboutModal;

        [Header("Dropdown Submenus (Optional)")]
        [SerializeField] private GameObject _gameMenuDropdown;
        [SerializeField] private GameObject _helpMenuDropdown;

        private void Awake()
        {
            if (_btnNewGame != null) _btnNewGame.onClick.AddListener(OnNewGameClicked);
            if (_btnRecords != null) _btnRecords.onClick.AddListener(OnRecordsClicked);
            if (_btnSettings != null) _btnSettings.onClick.AddListener(OnSettingsClicked);
            if (_btnHelpRules != null) _btnHelpRules.onClick.AddListener(OnHelpRulesClicked);
            if (_btnHelpAbout != null) _btnHelpAbout.onClick.AddListener(OnHelpAboutClicked);
            if (_btnExit != null) _btnExit.onClick.AddListener(OnExitClicked);

            if (_btnMinimize != null) _btnMinimize.onClick.AddListener(OnMinimizeClicked);
            if (_btnMaximize != null) _btnMaximize.onClick.AddListener(OnMaximizeClicked);
            if (_btnClose != null) _btnClose.onClick.AddListener(OnExitClicked);
        }

        private void Update()
        {
            // Escape key toggles settings/pause or closes open modals (HI-001)
            if (CheckEscapePressed())
            {
                OnEscapePressed();
            }
        }

        private bool CheckEscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;
#endif
            try
            {
                return Input.GetKeyDown(KeyCode.Escape);
            }
            catch
            {
                return false;
            }
        }

        private void OnEscapePressed()
        {
            if (_settingsModal != null && _settingsModal.gameObject.activeSelf)
            {
                _settingsModal.Hide();
                return;
            }
            if (_leaderboardsModal != null && _leaderboardsModal.gameObject.activeSelf)
            {
                _leaderboardsModal.Hide();
                return;
            }
            if (_helpAboutModal != null && _helpAboutModal.gameObject.activeSelf)
            {
                _helpAboutModal.Hide();
                return;
            }

            // If no modal open, toggle settings
            if (_settingsModal != null)
            {
                _settingsModal.Show();
            }
        }

        public void OnNewGameClicked()
        {
            CloseDropdowns();
            if (GameController.Instance != null)
            {
                GameController.Instance.RestartGame();
            }
        }

        public void OnRecordsClicked()
        {
            CloseDropdowns();
            if (_leaderboardsModal != null)
            {
                var diff = GameController.Instance != null ? GameController.Instance.CurrentDifficulty.Level : DifficultyLevel.Beginner;
                _leaderboardsModal.Show(diff);
            }
        }

        public void OnSettingsClicked()
        {
            CloseDropdowns();
            if (_settingsModal != null)
            {
                _settingsModal.Show();
            }
        }

        public void OnHelpRulesClicked()
        {
            CloseDropdowns();
            if (_helpAboutModal != null)
            {
                _helpAboutModal.Show(false);
            }
        }

        public void OnHelpAboutClicked()
        {
            CloseDropdowns();
            if (_helpAboutModal != null)
            {
                _helpAboutModal.Show(true);
            }
        }

        public void OnExitClicked()
        {
            CloseDropdowns();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void OnMinimizeClicked()
        {
            CloseDropdowns();
            if (GameController.Instance != null)
            {
                GameController.Instance.PauseGame();
            }
        }

        private void OnMaximizeClicked()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        private void CloseDropdowns()
        {
            if (_gameMenuDropdown != null) _gameMenuDropdown.SetActive(false);
            if (_helpMenuDropdown != null) _helpMenuDropdown.SetActive(false);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minesweeper.Core;
using Minesweeper.Audio;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Minesweeper.UI
{
    public class TopMenuBar : MonoBehaviour
    {
        [Header("Main Navigation Tabs")]
        [SerializeField] private Button _btnNewGame;
        [SerializeField] private Button _btnRecords;
        [SerializeField] private Button _btnSettings;
        [SerializeField] private Button _btnHelp;

        [Header("Tab Highlight Visuals")]
        [SerializeField] private Image _gameTabBg;
        [SerializeField] private Image _recordsTabBg;
        [SerializeField] private Image _settingsTabBg;
        [SerializeField] private Image _helpTabBg;

        [Header("Right Controls")]
        [SerializeField] private Button _btnDifficultyPill;
        [SerializeField] private TextMeshProUGUI _difficultyPillText;
        [SerializeField] private Button _btnSoundToggle;
        [SerializeField] private Image _soundIconImage;
        [SerializeField] private Button _btnFullscreen;
        [SerializeField] private Button _btnSettingsGear;
        [SerializeField] private TextMeshProUGUI _fpsText;

        [Header("Sound Toggle Icons")]
        public Sprite SoundOnSprite;
        public Sprite SoundOffSprite;

        [Header("Modals References")]
        [SerializeField] private SettingsModal _settingsModal;
        [SerializeField] private LeaderboardsModal _leaderboardsModal;
        [SerializeField] private HelpAboutModal _helpAboutModal;

        private float _fpsTimer = 0f;
        private int _fpsFrameCount = 0;

        private void Awake()
        {
            if (_btnNewGame != null) _btnNewGame.onClick.AddListener(OnNewGameClicked);
            if (_btnRecords != null) _btnRecords.onClick.AddListener(OnRecordsClicked);
            if (_btnSettings != null) _btnSettings.onClick.AddListener(OnSettingsClicked);
            if (_btnHelp != null) _btnHelp.onClick.AddListener(OnHelpClicked);

            if (_btnDifficultyPill != null) _btnDifficultyPill.onClick.AddListener(OnDifficultyPillClicked);
            if (_btnSoundToggle != null) _btnSoundToggle.onClick.AddListener(OnSoundToggleClicked);
            if (_btnFullscreen != null) _btnFullscreen.onClick.AddListener(OnFullscreenClicked);
            if (_btnSettingsGear != null) _btnSettingsGear.onClick.AddListener(OnSettingsClicked);
        }

        private void Start()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnGameInitialized += UpdateDifficultyPill;
                if (GameController.Instance.CurrentDifficulty != null)
                {
                    UpdateDifficultyPill(GameController.Instance.CurrentDifficulty);
                }
            }
            UpdateSoundIcon();
            SetActiveTabHighlight(0);
        }

        private void OnDestroy()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnGameInitialized -= UpdateDifficultyPill;
            }
        }

        private void Update()
        {
            // Update FPS counter
            _fpsFrameCount++;
            _fpsTimer += Time.unscaledDeltaTime;
            if (_fpsTimer >= 0.5f)
            {
                int fps = Mathf.RoundToInt(_fpsFrameCount / _fpsTimer);
                if (_fpsText != null)
                {
                    _fpsText.text = $"FPS: {fps}";
                }
                _fpsFrameCount = 0;
                _fpsTimer = 0f;
            }

            // Escape key handling (HI-001)
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
            if (_settingsModal != null && _settingsModal.IsOpen)
            {
                _settingsModal.Hide();
                SetActiveTabHighlight(0);
                return;
            }
            if (_leaderboardsModal != null && _leaderboardsModal.IsOpen)
            {
                _leaderboardsModal.Hide();
                SetActiveTabHighlight(0);
                return;
            }
            if (_helpAboutModal != null && _helpAboutModal.IsOpen)
            {
                _helpAboutModal.Hide();
                SetActiveTabHighlight(0);
                return;
            }

            // Toggle pause or settings
            if (GameController.Instance != null)
            {
                GameController.Instance.TogglePause();
            }
        }

        public void UpdateDifficultyPill(GameDifficulty diff)
        {
            if (_difficultyPillText == null || diff == null) return;
            string levelName = diff.Level == DifficultyLevel.Beginner ? "Новичок (9×9)" :
                               diff.Level == DifficultyLevel.Intermediate ? "Стандартный (16×16)" :
                               diff.Level == DifficultyLevel.Expert ? "Профессионал (30×16)" :
                               $"Особый ({diff.Width}×{diff.Height})";
            _difficultyPillText.text = $"{levelName} <color=#929db2>• {diff.Mines} мин</color>";
        }

        public void OnNewGameClicked()
        {
            CloseAllModals();
            SetActiveTabHighlight(0);
            if (GameController.Instance != null)
            {
                GameController.Instance.RestartGame();
            }
        }

        public void OnRecordsClicked()
        {
            CloseAllModals();
            SetActiveTabHighlight(1);
            if (_leaderboardsModal != null)
            {
                var diff = GameController.Instance != null ? GameController.Instance.CurrentDifficulty.Level : DifficultyLevel.Beginner;
                _leaderboardsModal.Show(diff);
            }
        }

        public void OnSettingsClicked()
        {
            CloseAllModals();
            SetActiveTabHighlight(2);
            if (_settingsModal != null)
            {
                _settingsModal.Show();
            }
        }

        public void OnHelpClicked()
        {
            CloseAllModals();
            SetActiveTabHighlight(3);
            if (_helpAboutModal != null)
            {
                _helpAboutModal.Show(false);
            }
        }

        private void OnDifficultyPillClicked()
        {
            OnSettingsClicked();
        }

        private void OnSoundToggleClicked()
        {
            if (GameController.Instance != null)
            {
                bool newMute = !GameController.Instance.Settings.IsMuted;
                GameController.Instance.Settings.IsMuted = newMute;
                GameController.Instance.SaveMgr.SaveSettings(GameController.Instance.Settings);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SetMute(newMute);
                }
                UpdateSoundIcon();
            }
        }

        private void UpdateSoundIcon()
        {
            bool isMuted = GameController.Instance != null && GameController.Instance.Settings.IsMuted;
            if (_soundIconImage != null)
            {
                _soundIconImage.sprite = isMuted ? SoundOffSprite : SoundOnSprite;
                _soundIconImage.color = isMuted ? new Color(244f / 255f, 63f / 255f, 94f / 255f) : new Color(0f, 242f / 255f, 254f / 255f);
            }
        }

        private void OnFullscreenClicked()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        public void SetActiveTabHighlight(int tabIndex)
        {
            Color activeBg = new Color(0f, 242f / 255f, 254f / 255f, 0.15f);
            Color inactiveBg = new Color(0, 0, 0, 0);

            if (_gameTabBg != null) _gameTabBg.color = tabIndex == 0 ? activeBg : inactiveBg;
            if (_recordsTabBg != null) _recordsTabBg.color = tabIndex == 1 ? activeBg : inactiveBg;
            if (_settingsTabBg != null) _settingsTabBg.color = tabIndex == 2 ? activeBg : inactiveBg;
            if (_helpTabBg != null) _helpTabBg.color = tabIndex == 3 ? activeBg : inactiveBg;
        }

        private void CloseAllModals()
        {
            if (_settingsModal != null && _settingsModal.IsOpen) _settingsModal.Hide();
            if (_leaderboardsModal != null && _leaderboardsModal.IsOpen) _leaderboardsModal.Hide();
            if (_helpAboutModal != null && _helpAboutModal.IsOpen) _helpAboutModal.Hide();
        }
    }
}

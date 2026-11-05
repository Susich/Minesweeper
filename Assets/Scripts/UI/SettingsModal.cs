using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minesweeper.Core;
using Minesweeper.Audio;

namespace Minesweeper.UI
{
    public class SettingsModal : MonoBehaviour
    {
        [Header("Modal Root")]
        [SerializeField] private GameObject _panel;

        [Header("Preset Buttons")]
        [SerializeField] private Button _btnBeginner;
        [SerializeField] private Button _btnIntermediate;
        [SerializeField] private Button _btnExpert;
        [SerializeField] private Button _btnCustom;

        [Header("Custom Steppers")]
        [SerializeField] private GameObject _customContainer;
        [SerializeField] private TextMeshProUGUI _widthText;
        [SerializeField] private Button _widthMinus;
        [SerializeField] private Button _widthPlus;

        [SerializeField] private TextMeshProUGUI _heightText;
        [SerializeField] private Button _heightMinus;
        [SerializeField] private Button _heightPlus;

        [SerializeField] private TextMeshProUGUI _minesText;
        [SerializeField] private Button _minesMinus;
        [SerializeField] private Button _minesPlus;

        [SerializeField] private TextMeshProUGUI _densityText;

        [Header("Audio Settings")]
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Toggle _muteToggle;
        [SerializeField] private Toggle _timerTickToggle;

        [Header("Modal Actions")]
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _closeButton;

        private DifficultyLevel _selectedLevel = DifficultyLevel.Beginner;
        private int _customW = 16;
        private int _customH = 16;
        private int _customM = 40;

        private void Awake()
        {
            if (_btnBeginner != null) _btnBeginner.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Beginner));
            if (_btnIntermediate != null) _btnIntermediate.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Intermediate));
            if (_btnExpert != null) _btnExpert.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Expert));
            if (_btnCustom != null) _btnCustom.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Custom));

            if (_widthMinus != null) _widthMinus.onClick.AddListener(() => AdjustWidth(-1));
            if (_widthPlus != null) _widthPlus.onClick.AddListener(() => AdjustWidth(1));

            if (_heightMinus != null) _heightMinus.onClick.AddListener(() => AdjustHeight(-1));
            if (_heightPlus != null) _heightPlus.onClick.AddListener(() => AdjustHeight(1));

            if (_minesMinus != null) _minesMinus.onClick.AddListener(() => AdjustMines(-5));
            if (_minesPlus != null) _minesPlus.onClick.AddListener(() => AdjustMines(5));

            if (_volumeSlider != null) _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            if (_muteToggle != null) _muteToggle.onValueChanged.AddListener(OnMuteChanged);
            if (_timerTickToggle != null) _timerTickToggle.onValueChanged.AddListener(OnTimerTickChanged);

            if (_okButton != null) _okButton.onClick.AddListener(ApplyAndClose);
            if (_cancelButton != null) _cancelButton.onClick.AddListener(Hide);
            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
        }

        private void Start()
        {
            Hide();
        }

        public void Show()
        {
            if (_panel == null) _panel = gameObject;
            _panel.SetActive(true);

            if (GameController.Instance != null)
            {
                GameController.Instance.PauseGame();

                var diff = GameController.Instance.CurrentDifficulty;
                _selectedLevel = diff.Level;
                _customW = GameController.Instance.Settings.CustomWidth;
                _customH = GameController.Instance.Settings.CustomHeight;
                _customM = GameController.Instance.Settings.CustomMines;

                if (_volumeSlider != null) _volumeSlider.value = GameController.Instance.Settings.SoundVolume;
                if (_muteToggle != null) _muteToggle.isOn = GameController.Instance.Settings.IsMuted;
                if (_timerTickToggle != null) _timerTickToggle.isOn = GameController.Instance.Settings.TimerTickEnabled;
            }

            UpdateUI();
        }

        public void Hide()
        {
            if (_panel == null) _panel = gameObject;
            _panel.SetActive(false);

            if (GameController.Instance != null)
            {
                GameController.Instance.ResumeGame();
            }
        }

        private void SelectDifficulty(DifficultyLevel level)
        {
            _selectedLevel = level;
            UpdateUI();
        }

        private void AdjustWidth(int delta)
        {
            _customW += delta;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void AdjustHeight(int delta)
        {
            _customH += delta;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void AdjustMines(int delta)
        {
            _customM += delta;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void OnVolumeChanged(float val)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetVolume(val);
            }
            if (GameController.Instance != null)
            {
                GameController.Instance.Settings.SoundVolume = val;
                GameController.Instance.SaveMgr.SaveSettings(GameController.Instance.Settings);
            }
        }

        private void OnMuteChanged(bool muted)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMute(muted);
            }
            if (GameController.Instance != null)
            {
                GameController.Instance.Settings.IsMuted = muted;
                GameController.Instance.SaveMgr.SaveSettings(GameController.Instance.Settings);
            }
        }

        private void OnTimerTickChanged(bool enabled)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetTimerTickEnabled(enabled);
            }
            if (GameController.Instance != null)
            {
                GameController.Instance.Settings.TimerTickEnabled = enabled;
                GameController.Instance.SaveMgr.SaveSettings(GameController.Instance.Settings);
            }
        }

        private void UpdateUI()
        {
            if (_customContainer != null)
            {
                _customContainer.SetActive(_selectedLevel == DifficultyLevel.Custom);
            }

            if (_widthText != null) _widthText.text = _customW.ToString();
            if (_heightText != null) _heightText.text = _customH.ToString();
            if (_minesText != null) _minesText.text = _customM.ToString();

            int w = _selectedLevel == DifficultyLevel.Beginner ? 9 :
                    _selectedLevel == DifficultyLevel.Intermediate ? 16 :
                    _selectedLevel == DifficultyLevel.Expert ? 30 : _customW;
            int h = _selectedLevel == DifficultyLevel.Beginner ? 9 :
                    _selectedLevel == DifficultyLevel.Intermediate ? 16 :
                    _selectedLevel == DifficultyLevel.Expert ? 16 : _customH;
            int m = _selectedLevel == DifficultyLevel.Beginner ? 10 :
                    _selectedLevel == DifficultyLevel.Intermediate ? 40 :
                    _selectedLevel == DifficultyLevel.Expert ? 99 : _customM;

            if (_densityText != null && (w * h) > 0)
            {
                float density = ((float)m / (w * h)) * 100f;
                _densityText.text = $"Тактическая плотность поля: {density:F1}%";
            }
        }

        private void ApplyAndClose()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.StartNewGame(_selectedLevel, _customW, _customH, _customM);
            }
            Hide();
        }
    }
}

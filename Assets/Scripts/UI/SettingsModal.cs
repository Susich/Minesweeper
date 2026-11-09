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

        [Header("Preset Radio / Status Indicators")]
        [SerializeField] private Image _indicatorBeginner;
        [SerializeField] private Image _indicatorIntermediate;
        [SerializeField] private Image _indicatorExpert;
        [SerializeField] private Image _indicatorCustom;

        [Header("Custom Steppers")]
        [SerializeField] private GameObject _customContainer;
        [SerializeField] private TextMeshProUGUI _widthText;
        [SerializeField] private Button _widthMinus;
        [SerializeField] private Button _widthPlus;
        [SerializeField] private Slider _widthSlider;

        [SerializeField] private TextMeshProUGUI _heightText;
        [SerializeField] private Button _heightMinus;
        [SerializeField] private Button _heightPlus;
        [SerializeField] private Slider _heightSlider;

        [SerializeField] private TextMeshProUGUI _minesText;
        [SerializeField] private Button _minesMinus;
        [SerializeField] private Button _minesPlus;
        [SerializeField] private Slider _minesSlider;

        [Header("Tactical Density & Summary")]
        [SerializeField] private TextMeshProUGUI _densityText;
        [SerializeField] private TextMeshProUGUI _densityStatusBadge;
        [SerializeField] private TextMeshProUGUI _totalCellsText;

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

        private static readonly Color ColorActiveBorder = new Color(0f, 242f / 255f, 254f / 255f, 1f);
        private static readonly Color ColorInactiveBorder = new Color(38f / 255f, 49f / 255f, 68f / 255f, 1f);
        private static readonly Color ColorActiveBg = new Color(17f / 255f, 23f / 255f, 36f / 255f, 1f);
        private static readonly Color ColorInactiveBg = new Color(13f / 255f, 17f / 255f, 26f / 255f, 1f);

        private void Awake()
        {
            if (_btnBeginner != null) _btnBeginner.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Beginner));
            if (_btnIntermediate != null) _btnIntermediate.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Intermediate));
            if (_btnExpert != null) _btnExpert.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Expert));
            if (_btnCustom != null) _btnCustom.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Custom));

            if (_widthMinus != null) _widthMinus.onClick.AddListener(() => AdjustWidth(-1));
            if (_widthPlus != null) _widthPlus.onClick.AddListener(() => AdjustWidth(1));
            if (_widthSlider != null) _widthSlider.onValueChanged.AddListener(val => SetWidth((int)val));

            if (_heightMinus != null) _heightMinus.onClick.AddListener(() => AdjustHeight(-1));
            if (_heightPlus != null) _heightPlus.onClick.AddListener(() => AdjustHeight(1));
            if (_heightSlider != null) _heightSlider.onValueChanged.AddListener(val => SetHeight((int)val));

            if (_minesMinus != null) _minesMinus.onClick.AddListener(() => AdjustMines(-5));
            if (_minesPlus != null) _minesPlus.onClick.AddListener(() => AdjustMines(5));
            if (_minesSlider != null) _minesSlider.onValueChanged.AddListener(val => SetMines((int)val));

            if (_volumeSlider != null) _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            if (_muteToggle != null) _muteToggle.onValueChanged.AddListener(OnMuteChanged);
            if (_timerTickToggle != null) _timerTickToggle.onValueChanged.AddListener(OnTimerTickChanged);

            if (_okButton != null) _okButton.onClick.AddListener(ApplyAndClose);
            if (_cancelButton != null) _cancelButton.onClick.AddListener(Hide);
            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
        }

        public bool IsOpen => _panel != null ? _panel.activeSelf : gameObject.activeSelf;

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
                _selectedLevel = diff != null ? diff.Level : DifficultyLevel.Intermediate;
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
            _selectedLevel = DifficultyLevel.Custom;
            _customW += delta;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void SetWidth(int val)
        {
            _selectedLevel = DifficultyLevel.Custom;
            _customW = val;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void AdjustHeight(int delta)
        {
            _selectedLevel = DifficultyLevel.Custom;
            _customH += delta;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void SetHeight(int val)
        {
            _selectedLevel = DifficultyLevel.Custom;
            _customH = val;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void AdjustMines(int delta)
        {
            _selectedLevel = DifficultyLevel.Custom;
            _customM += delta;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void SetMines(int val)
        {
            _selectedLevel = DifficultyLevel.Custom;
            _customM = val;
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
            UpdatePresetButtonState(_btnBeginner, _indicatorBeginner, _selectedLevel == DifficultyLevel.Beginner);
            UpdatePresetButtonState(_btnIntermediate, _indicatorIntermediate, _selectedLevel == DifficultyLevel.Intermediate);
            UpdatePresetButtonState(_btnExpert, _indicatorExpert, _selectedLevel == DifficultyLevel.Expert);
            UpdatePresetButtonState(_btnCustom, _indicatorCustom, _selectedLevel == DifficultyLevel.Custom);

            if (_customContainer != null)
            {
                _customContainer.SetActive(true); // Keep visible or dim when not custom
            }

            if (_widthText != null) _widthText.text = _customW.ToString();
            if (_heightText != null) _heightText.text = _customH.ToString();
            if (_minesText != null) _minesText.text = _customM.ToString();

            if (_widthSlider != null && (int)_widthSlider.value != _customW) _widthSlider.SetValueWithoutNotify(_customW);
            if (_heightSlider != null && (int)_heightSlider.value != _customH) _heightSlider.SetValueWithoutNotify(_customH);
            if (_minesSlider != null && (int)_minesSlider.value != _customM) _minesSlider.SetValueWithoutNotify(_customM);

            int w = _selectedLevel == DifficultyLevel.Beginner ? 9 :
                    _selectedLevel == DifficultyLevel.Intermediate ? 16 :
                    _selectedLevel == DifficultyLevel.Expert ? 30 : _customW;
            int h = _selectedLevel == DifficultyLevel.Beginner ? 9 :
                    _selectedLevel == DifficultyLevel.Intermediate ? 16 :
                    _selectedLevel == DifficultyLevel.Expert ? 16 : _customH;
            int m = _selectedLevel == DifficultyLevel.Beginner ? 10 :
                    _selectedLevel == DifficultyLevel.Intermediate ? 40 :
                    _selectedLevel == DifficultyLevel.Expert ? 99 : _customM;

            int totalCells = w * h;
            if (_totalCellsText != null)
            {
                _totalCellsText.text = $"Всего ячеек: {totalCells} • 1-й клик: 100% безопасен";
            }

            if (totalCells > 0)
            {
                float density = ((float)m / totalCells) * 100f;
                if (_densityText != null)
                {
                    _densityText.text = $"Плотность мин: {density:F1}%";
                }

                if (_densityStatusBadge != null)
                {
                    if (density < 14f)
                    {
                        _densityStatusBadge.text = "[НИЗКАЯ]";
                        _densityStatusBadge.color = new Color(56f / 255f, 189f / 255f, 248f / 255f);
                    }
                    else if (density <= 23f)
                    {
                        _densityStatusBadge.text = "[ОПТИМАЛЬНО]";
                        _densityStatusBadge.color = new Color(0f, 242f / 255f, 254f / 255f);
                    }
                    else if (density <= 35f)
                    {
                        _densityStatusBadge.text = "[ВЫСОКАЯ]";
                        _densityStatusBadge.color = new Color(251f / 255f, 191f / 255f, 36f / 255f);
                    }
                    else
                    {
                        _densityStatusBadge.text = "[ЭКСТРЕМАЛЬНАЯ]";
                        _densityStatusBadge.color = new Color(248f / 255f, 113f / 255f, 113f / 255f);
                    }
                }
            }
        }

        private void UpdatePresetButtonState(Button btn, Image indicator, bool isSelected)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isSelected ? ColorActiveBg : ColorInactiveBg;
            }

            if (indicator != null)
            {
                indicator.color = isSelected ? ColorActiveBorder : new Color(1f, 1f, 1f, 0.2f);
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

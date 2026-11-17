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
        [SerializeField] private TMP_InputField _widthInput;
        [SerializeField] private Button _widthMinus;
        [SerializeField] private Button _widthPlus;
        [SerializeField] private Slider _widthSlider;

        [SerializeField] private TMP_InputField _heightInput;
        [SerializeField] private Button _heightMinus;
        [SerializeField] private Button _heightPlus;
        [SerializeField] private Slider _heightSlider;

        [SerializeField] private TMP_InputField _minesInput;
        [SerializeField] private Button _minesMinus;
        [SerializeField] private Button _minesPlus;
        [SerializeField] private Slider _minesSlider;

        [Header("Tactical Density & Summary")]
        [SerializeField] private TextMeshProUGUI _densityText;
        [SerializeField] private TextMeshProUGUI _densityStatusBadge;
        [SerializeField] private TextMeshProUGUI _totalCellsText;

        [Header("Audio Settings")]
        [SerializeField] private Slider _volumeSlider;
        // Legacy scene reference: mute is controlled by the speaker button in the top
        // bar, so the duplicate checkbox is removed from the settings modal at runtime.
        [SerializeField, HideInInspector] private Toggle _muteToggle;
        [SerializeField] private Toggle _timerTickToggle;

        [Header("Modal Actions")]
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _headerCloseButton;

        [Header("Preset Card Sprites")]
        public Sprite ActiveCardSprite;
        public Sprite NormalCardSprite;

        private DifficultyLevel _selectedLevel = DifficultyLevel.Beginner;
        private int _customW = 16;
        private int _customH = 16;
        private int _customM = 40;
        private bool _pausedByModal;

        private static readonly Color ColorActiveBorder = new Color(0f, 242f / 255f, 254f / 255f, 1f);
        private static readonly Color ColorInactiveBorder = new Color(38f / 255f, 49f / 255f, 68f / 255f, 1f);
        private static readonly Color ColorActiveBg = new Color(17f / 255f, 23f / 255f, 36f / 255f, 1f);
        private static readonly Color ColorInactiveBg = new Color(13f / 255f, 17f / 255f, 26f / 255f, 1f);

        private void Awake()
        {
            // The mute toggle duplicates the speaker button in the top bar; only the
            // timer-tick option stays in the audio block.
            if (_muteToggle != null)
            {
                _muteToggle.onValueChanged.RemoveAllListeners();
                _muteToggle.gameObject.SetActive(false);
                Destroy(_muteToggle.gameObject);
                _muteToggle = null;
            }

            if (_btnBeginner != null) _btnBeginner.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Beginner));
            if (_btnIntermediate != null) _btnIntermediate.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Intermediate));
            if (_btnExpert != null) _btnExpert.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Expert));
            if (_btnCustom != null) _btnCustom.onClick.AddListener(() => SelectDifficulty(DifficultyLevel.Custom));

            if (_widthMinus != null) _widthMinus.onClick.AddListener(() => AdjustWidth(-1));
            if (_widthPlus != null) _widthPlus.onClick.AddListener(() => AdjustWidth(1));
            if (_widthSlider != null) _widthSlider.onValueChanged.AddListener(val => SetWidth((int)val));
            if (_widthInput != null) _widthInput.onEndEdit.AddListener(OnWidthTyped);

            if (_heightMinus != null) _heightMinus.onClick.AddListener(() => AdjustHeight(-1));
            if (_heightPlus != null) _heightPlus.onClick.AddListener(() => AdjustHeight(1));
            if (_heightSlider != null) _heightSlider.onValueChanged.AddListener(val => SetHeight((int)val));
            if (_heightInput != null) _heightInput.onEndEdit.AddListener(OnHeightTyped);

            if (_minesMinus != null) _minesMinus.onClick.AddListener(() => AdjustMines(-5));
            if (_minesPlus != null) _minesPlus.onClick.AddListener(() => AdjustMines(5));
            if (_minesSlider != null) _minesSlider.onValueChanged.AddListener(val => SetMines((int)val));
            if (_minesInput != null) _minesInput.onEndEdit.AddListener(OnMinesTyped);

            if (_volumeSlider != null) _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            if (_timerTickToggle != null) _timerTickToggle.onValueChanged.AddListener(OnTimerTickChanged);

            if (_okButton != null) _okButton.onClick.AddListener(ApplyAndClose);
            if (_cancelButton != null) _cancelButton.onClick.AddListener(Hide);
            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
            if (_headerCloseButton != null && _headerCloseButton != _closeButton)
            {
                _headerCloseButton.onClick.AddListener(Hide);
            }
        }

        public bool IsOpen => _panel != null ? _panel.activeSelf : gameObject.activeSelf;

        private void Start()
        {
            Hide();
        }

        public void Show()
        {
            if (_panel == null) _panel = gameObject;
            bool wasOpen = _panel.activeSelf;
            _panel.SetActive(true);

            if (!wasOpen && GameController.Instance != null)
            {
                _pausedByModal = GameController.Instance.PauseForModal();

                var diff = GameController.Instance.CurrentDifficulty;
                _selectedLevel = diff != null ? diff.Level : DifficultyLevel.Intermediate;
                _customW = GameController.Instance.Settings.CustomWidth;
                _customH = GameController.Instance.Settings.CustomHeight;
                _customM = GameController.Instance.Settings.CustomMines;

                if (_volumeSlider != null) _volumeSlider.value = GameController.Instance.Settings.SoundVolume;
                if (_timerTickToggle != null) _timerTickToggle.isOn = GameController.Instance.Settings.TimerTickEnabled;
            }

            UpdateUI();
        }

        /// <summary>Opens the dialog with the «Особые параметры» preset selected.</summary>
        public void ShowCustom()
        {
            Show();
            SelectDifficulty(DifficultyLevel.Custom);
        }

        public void Hide()
        {
            if (_panel == null) _panel = gameObject;
            _panel.SetActive(false);

            if (_pausedByModal)
            {
                _pausedByModal = false;
                if (GameController.Instance != null)
                {
                    GameController.Instance.ResumeAfterModal(true);
                }
            }
        }

        private void SelectDifficulty(DifficultyLevel level)
        {
            _selectedLevel = level;
            UpdateUI();
        }

        /// <summary>
        /// Copies the values of the currently selected preset into the custom editor,
        /// so pressing +/− right after choosing a preset starts from what the player sees
        /// instead of jumping back to stale custom values (issue #5).
        /// </summary>
        private void BeginCustomAdjustment()
        {
            if (_selectedLevel == DifficultyLevel.Custom) return;

            GetDisplayedParameters(out _customW, out _customH, out _customM);
            _selectedLevel = DifficultyLevel.Custom;
        }

        private void AdjustWidth(int delta)
        {
            BeginCustomAdjustment();
            _customW += delta;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void SetWidth(int val)
        {
            BeginCustomAdjustment();
            _customW = val;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void AdjustHeight(int delta)
        {
            BeginCustomAdjustment();
            _customH += delta;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void SetHeight(int val)
        {
            BeginCustomAdjustment();
            _customH = val;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void AdjustMines(int delta)
        {
            BeginCustomAdjustment();
            _customM += delta;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        private void SetMines(int val)
        {
            BeginCustomAdjustment();
            _customM = val;
            GameDifficulty.ValidateCustom(ref _customW, ref _customH, ref _customM);
            UpdateUI();
        }

        // --- Typed values (SRS UI-012: поля ввода высоты, ширины и количества мин) ---
        private void OnWidthTyped(string value)
        {
            if (int.TryParse(value, out int parsed)) SetWidth(parsed);
            else UpdateUI();
        }

        private void OnHeightTyped(string value)
        {
            if (int.TryParse(value, out int parsed)) SetHeight(parsed);
            else UpdateUI();
        }

        private void OnMinesTyped(string value)
        {
            if (int.TryParse(value, out int parsed)) SetMines(parsed);
            else UpdateUI();
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

        private void GetDisplayedParameters(out int width, out int height, out int mines)
        {
            switch (_selectedLevel)
            {
                case DifficultyLevel.Beginner:
                    width = 9; height = 9; mines = 10;
                    break;
                case DifficultyLevel.Intermediate:
                    width = 16; height = 16; mines = 40;
                    break;
                case DifficultyLevel.Expert:
                    width = 30; height = 16; mines = 99;
                    break;
                default:
                    width = _customW; height = _customH; mines = _customM;
                    break;
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

            // The steppers always show the parameters that will actually be applied,
            // i.e. the values of the selected preset (issue #5).
            GetDisplayedParameters(out int w, out int h, out int m);

            if (_widthInput != null && !_widthInput.isFocused) _widthInput.SetTextWithoutNotify(w.ToString());
            if (_heightInput != null && !_heightInput.isFocused) _heightInput.SetTextWithoutNotify(h.ToString());
            if (_minesInput != null && !_minesInput.isFocused) _minesInput.SetTextWithoutNotify(m.ToString());

            if (_widthSlider != null && (int)_widthSlider.value != w) _widthSlider.SetValueWithoutNotify(w);
            if (_heightSlider != null && (int)_heightSlider.value != h) _heightSlider.SetValueWithoutNotify(h);

            if (_minesSlider != null)
            {
                // The mine cap follows the current grid, exactly like GameDifficulty.ValidateCustom.
                int maxMines = Mathf.Max(10, w * h - 9);
                if (!Mathf.Approximately(_minesSlider.maxValue, maxMines))
                {
                    _minesSlider.maxValue = maxMines;
                }
                if ((int)_minesSlider.value != m) _minesSlider.SetValueWithoutNotify(m);
            }

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
                if (isSelected && ActiveCardSprite != null)
                {
                    img.sprite = ActiveCardSprite;
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }
                else if (!isSelected && NormalCardSprite != null)
                {
                    img.sprite = NormalCardSprite;
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }
                else
                {
                    img.color = isSelected ? ColorActiveBg : ColorInactiveBg;
                }
            }

            if (indicator != null)
            {
                indicator.color = isSelected ? ColorActiveBorder : new Color(1f, 1f, 1f, 0.22f);
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

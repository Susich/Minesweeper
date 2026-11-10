using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minesweeper.Core;
using Minesweeper.Storage;

namespace Minesweeper.UI
{
    public class LeaderboardsModal : MonoBehaviour
    {
        [Header("Modal Root")]
        [SerializeField] private GameObject _panel;

        [Header("Tab Buttons")]
        [SerializeField] private Button _tabBeginner;
        [SerializeField] private Button _tabIntermediate;
        [SerializeField] private Button _tabExpert;
        [SerializeField] private Button _tabCustom;

        [Header("Tab Highlights")]
        [SerializeField] private Image _tabBeginnerBg;
        [SerializeField] private Image _tabIntermediateBg;
        [SerializeField] private Image _tabExpertBg;
        [SerializeField] private Image _tabCustomBg;

        [Header("Rows Container")]
        [SerializeField] private Transform _rowsContainer;
        [SerializeField] private GameObject _rowPrefab;
        [SerializeField] private GameObject _customNoticeObject;

        [Header("Best Result Banner")]
        [SerializeField] private TextMeshProUGUI _bestResultText;

        [Header("Actions")]
        [SerializeField] private Button _resetScoresButton;
        [SerializeField] private Button _closeButton;

        [Header("Font Reference")]
        [SerializeField] private TMP_FontAsset _fontAsset;

        private DifficultyLevel _activeTab = DifficultyLevel.Beginner;

        private static readonly Color ColorActiveTab = new Color(0f, 242f / 255f, 254f / 255f, 0.25f);
        private static readonly Color ColorInactiveTab = new Color(13f / 255f, 17f / 255f, 26f / 255f, 0.9f);

        private void Awake()
        {
            if (_tabBeginner != null) _tabBeginner.onClick.AddListener(() => SwitchTab(DifficultyLevel.Beginner));
            if (_tabIntermediate != null) _tabIntermediate.onClick.AddListener(() => SwitchTab(DifficultyLevel.Intermediate));
            if (_tabExpert != null) _tabExpert.onClick.AddListener(() => SwitchTab(DifficultyLevel.Expert));
            if (_tabCustom != null) _tabCustom.onClick.AddListener(() => SwitchTab(DifficultyLevel.Custom));

            if (_resetScoresButton != null) _resetScoresButton.onClick.AddListener(OnResetScoresClicked);
            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
        }

        public bool IsOpen => _panel != null ? _panel.activeSelf : gameObject.activeSelf;

        private void Start()
        {
            Hide();
        }

        public void Show(DifficultyLevel level = DifficultyLevel.Beginner)
        {
            if (_panel == null) _panel = gameObject;
            _panel.SetActive(true);

            if (GameController.Instance != null)
            {
                GameController.Instance.PauseGame();
            }

            SwitchTab(level);
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

        public void SetFont(TMP_FontAsset font)
        {
            _fontAsset = font;
        }

        private void SwitchTab(DifficultyLevel level)
        {
            _activeTab = level;

            UpdateTabVisual(_tabBeginnerBg, _activeTab == DifficultyLevel.Beginner);
            UpdateTabVisual(_tabIntermediateBg, _activeTab == DifficultyLevel.Intermediate);
            UpdateTabVisual(_tabExpertBg, _activeTab == DifficultyLevel.Expert);
            UpdateTabVisual(_tabCustomBg, _activeTab == DifficultyLevel.Custom);

            PopulateTable();
        }

        private void UpdateTabVisual(Image bg, bool isActive)
        {
            if (bg != null)
            {
                bg.color = isActive ? ColorActiveTab : ColorInactiveTab;
            }
        }

        public void PopulateTable()
        {
            if (GameController.Instance == null) return;
            var highScores = GameController.Instance.HighScores;

            if (_activeTab == DifficultyLevel.Custom)
            {
                if (_rowsContainer != null) _rowsContainer.gameObject.SetActive(false);
                if (_customNoticeObject != null) _customNoticeObject.SetActive(true);
                if (_bestResultText != null) _bestResultText.text = "ОСОБЫЙ РЕЖИМ НЕ УЧИТЫВАЕТСЯ В РЕЕСТРЕ РЕКОРДОВ (FR-026)";
                return;
            }

            if (_customNoticeObject != null) _customNoticeObject.SetActive(false);
            if (_rowsContainer != null) _rowsContainer.gameObject.SetActive(true);

            var list = highScores.GetListForDifficulty(_activeTab);

            // Clear previous rows
            if (_rowsContainer != null)
            {
                foreach (Transform child in _rowsContainer)
                {
                    Destroy(child.gameObject);
                }

                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var entry = list[i];
                        var rowObj = CreateFallbackRow(i + 1, entry);
                        rowObj.SetActive(true);
                    }
                }
            }

            if (_bestResultText != null)
            {
                if (list != null && list.Count > 0)
                {
                    _bestResultText.text = $"ВАШ ЛУЧШИЙ РЕЗУЛЬТАТ: {list[0].TimeSeconds:F1}s [РАНГ #1 — {list[0].PlayerName}]";
                }
                else
                {
                    _bestResultText.text = "РЕКОРДОВ ПОКА НЕТ // БУДЬТЕ ПЕРВЫМ";
                }
            }
        }

        private GameObject CreateFallbackRow(int rank, HighScoreEntry entry)
        {
            var rowObj = new GameObject($"Row_{rank}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            rowObj.transform.SetParent(_rowsContainer, false);
            var rt = rowObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 32);

            var le = rowObj.GetComponent<LayoutElement>();
            le.minHeight = 32;
            le.preferredHeight = 32;
            le.flexibleWidth = 1f;

            var img = rowObj.GetComponent<Image>();
            img.color = rank == 1 ? new Color(38f / 255f, 32f / 255f, 18f / 255f, 0.9f) :
                        rank == 2 ? new Color(16f / 255f, 32f / 255f, 38f / 255f, 0.9f) :
                        rank == 3 ? new Color(24f / 255f, 28f / 255f, 36f / 255f, 0.9f) :
                        new Color(15f / 255f, 20f / 255f, 28f / 255f, 0.75f);

            var hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 0, 0);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            // 1. Rank (#1, #2, ...)
            var rankColor = rank == 1 ? new Color(251f / 255f, 191f / 255f, 36f / 255f) :
                            rank == 2 ? new Color(0f, 242f / 255f, 254f / 255f) :
                            rank == 3 ? new Color(203f / 255f, 213f / 255f, 225f / 255f) :
                            new Color(148f / 255f, 163f / 255f, 184f / 255f);
            var rankTxt = CreateLabel(rowObj.transform, $"#{rank}", 50, rankColor);
            rankTxt.fontStyle = FontStyles.Bold;
            rankTxt.alignment = TextAlignmentOptions.MidlineLeft;

            // 2. Operator Callsign
            var nameTxt = CreateLabel(rowObj.transform, entry.PlayerName, 270, Color.white);
            nameTxt.fontStyle = FontStyles.Bold;
            nameTxt.alignment = TextAlignmentOptions.MidlineLeft;

            // 3. Time
            var timeColor = rank == 1 ? new Color(251f / 255f, 191f / 255f, 36f / 255f) :
                            new Color(0f, 242f / 255f, 254f / 255f);
            var timeTxt = CreateLabel(rowObj.transform, $"{entry.TimeSeconds:F1}s", 100, timeColor);
            timeTxt.fontStyle = FontStyles.Bold;
            timeTxt.alignment = TextAlignmentOptions.MidlineRight;

            // 4. Date
            var dateTxt = CreateLabel(rowObj.transform, entry.Date, 120, new Color(148f / 255f, 163f / 255f, 184f / 255f));
            dateTxt.alignment = TextAlignmentOptions.MidlineRight;

            return rowObj;
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string text, float width, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (_fontAsset != null) tmp.font = _fontAsset;
            tmp.text = text;
            tmp.fontSize = 13;
            tmp.color = color;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(width, 24);
            var le = go.GetComponent<LayoutElement>();
            le.minWidth = width;
            le.preferredWidth = width;
            return tmp;
        }

        private void OnResetScoresClicked()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.HighScores.ResetToDefaults();
                GameController.Instance.SaveMgr.SaveHighScores(GameController.Instance.HighScores);
                PopulateTable();
            }
        }
    }
}

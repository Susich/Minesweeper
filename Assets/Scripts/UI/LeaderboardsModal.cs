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
        /// <summary>Scales the table metrics onto the native 1920x1080 canvas.</summary>
        private const float UI_SCALE = 1.2f;

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
        [SerializeField] private TextMeshProUGUI _bestStatsText;

        [Header("Actions")]
        [SerializeField] private Button _resetScoresButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _headerCloseButton;

        [Header("Font Reference")]
        [SerializeField] private TMP_FontAsset _fontAsset;

        [Header("Row Layout")]
        [SerializeField] private float _rowHeight = 48f;
        [SerializeField] private Sprite _avatarSprite;
        [SerializeField] private Sprite _replaySprite;
        [SerializeField] private Sprite _playSprite;

        [Header("Tab Sprites")]
        public Sprite ActiveTabSprite;
        public Sprite InactiveTabSprite;

        private DifficultyLevel _activeTab = DifficultyLevel.Beginner;

        private static readonly Color ColorActiveTab = new Color(0f, 242f / 255f, 254f / 255f, 0.25f);
        private static readonly Color ColorInactiveTab = new Color(13f / 255f, 17f / 255f, 26f / 255f, 0.9f);
        private static readonly Color ColPrimary = new Color(224f / 255f, 253f / 255f, 1f);
        private static readonly Color ColPrimaryContainer = new Color(0f, 242f / 255f, 254f / 255f);
        private static readonly Color ColSecondary = new Color(56f / 255f, 189f / 255f, 248f / 255f);
        private static readonly Color ColSurface = new Color(24f / 255f, 30f / 255f, 43f / 255f);
        private static readonly Color ColOnSurfaceVariant = new Color(148f / 255f, 163f / 255f, 184f / 255f);

        private void Awake()
        {
            if (_tabBeginner != null) _tabBeginner.onClick.AddListener(() => SwitchTab(DifficultyLevel.Beginner));
            if (_tabIntermediate != null) _tabIntermediate.onClick.AddListener(() => SwitchTab(DifficultyLevel.Intermediate));
            if (_tabExpert != null) _tabExpert.onClick.AddListener(() => SwitchTab(DifficultyLevel.Expert));
            if (_tabCustom != null) _tabCustom.onClick.AddListener(() => SwitchTab(DifficultyLevel.Custom));

            if (_resetScoresButton != null) _resetScoresButton.onClick.AddListener(OnResetScoresClicked);
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
            if (bg == null) return;

            if (isActive && ActiveTabSprite != null)
            {
                bg.sprite = ActiveTabSprite;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }
            else if (!isActive && InactiveTabSprite != null)
            {
                bg.sprite = InactiveTabSprite;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(1f, 1f, 1f, 0f);
            }
            else
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
                    var best = list[0];
                    _bestResultText.text = $"ВАШ ЛУЧШИЙ РЕЗУЛЬТАТ: {best.TimeSeconds:000.0}s   [РАНГ #1 — {best.PlayerName}]";
                    if (_bestStatsText != null)
                    {
                        _bestStatsText.text = $"Точность: {best.Accuracy:F1}%    ЗВУК/С: {best.Efficiency:F2}    Операций: {GetOpenCellCount(_activeTab)}";
                    }
                }
                else
                {
                    _bestResultText.text = "РЕКОРДОВ ПОКА НЕТ // БУДЬТЕ ПЕРВЫМ";
                    if (_bestStatsText != null) _bestStatsText.text = string.Empty;
                }
            }
        }

        private static int GetOpenCellCount(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Beginner: return 9 * 9 - 10;
                case DifficultyLevel.Intermediate: return 16 * 16 - 40;
                case DifficultyLevel.Expert: return 30 * 16 - 99;
                default: return 0;
            }
        }

        private static uint HashName(string name)
        {
            unchecked
            {
                uint hash = 2166136261u;
                if (string.IsNullOrEmpty(name)) return hash;
                for (int i = 0; i < name.Length; i++)
                {
                    hash ^= name[i];
                    hash *= 16777619u;
                }
                return hash;
            }
        }

        private static string BuildOperatorId(string name)
        {
            uint hash = HashName(name);
            return hash.ToString("X6");
        }

        private static string BuildFactionTag(string name)
        {
            string[] factions = { "ORBIT_NET", "DEEP_VOID", "NEURAL_LINK", "DECK_4", "CURRENT_RIG" };
            uint hash = HashName(name);
            return factions[hash % (uint)factions.Length];
        }

        private GameObject CreateFallbackRow(int rank, HighScoreEntry entry)
        {
            var rowObj = new GameObject($"Row_{rank}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            rowObj.transform.SetParent(_rowsContainer, false);
            var rt = rowObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, _rowHeight);

            var le = rowObj.GetComponent<LayoutElement>();
            le.minHeight = _rowHeight;
            le.preferredHeight = _rowHeight;
            le.flexibleWidth = 1f;

            var img = rowObj.GetComponent<Image>();
            img.color = rank == 1 ? new Color(38f / 255f, 32f / 255f, 18f / 255f, 0.9f) :
                        rank == 2 ? new Color(16f / 255f, 32f / 255f, 38f / 255f, 0.9f) :
                        rank == 3 ? new Color(24f / 255f, 28f / 255f, 36f / 255f, 0.9f) :
                        new Color(15f / 255f, 20f / 255f, 28f / 255f, 0.75f);

            var hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(18, 18, 0, 0);
            hlg.spacing = 10 * UI_SCALE;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // 1. Rank chip ------------------------------------------------
            var rankColor = rank == 1 ? new Color(251f / 255f, 191f / 255f, 36f / 255f) :
                            rank == 2 ? new Color(0f, 242f / 255f, 254f / 255f) :
                            rank == 3 ? new Color(203f / 255f, 213f / 255f, 225f / 255f) :
                            new Color(148f / 255f, 163f / 255f, 184f / 255f);

            var rankChip = new GameObject("RankChip", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            rankChip.transform.SetParent(rowObj.transform, false);
            rankChip.GetComponent<RectTransform>().sizeDelta = new Vector2(34 * UI_SCALE, 24 * UI_SCALE);
            var chipLe = rankChip.GetComponent<LayoutElement>();
            chipLe.minWidth = 34 * UI_SCALE; chipLe.preferredWidth = 34 * UI_SCALE;
            chipLe.minHeight = 22 * UI_SCALE; chipLe.preferredHeight = 22 * UI_SCALE;
            var chipImg = rankChip.GetComponent<Image>();
            chipImg.color = new Color(rankColor.r, rankColor.g, rankColor.b, 0.16f);
            var rankTxt = CreateLabel(rankChip.transform, $"#{rank}", 34, rankColor);
            rankTxt.fontSize = 12 * UI_SCALE;
            rankTxt.fontStyle = FontStyles.Bold;
            rankTxt.alignment = TextAlignmentOptions.Center;
            var rankRt = rankTxt.GetComponent<RectTransform>();
            rankRt.anchorMin = Vector2.zero; rankRt.anchorMax = Vector2.one; rankRt.offsetMin = Vector2.zero; rankRt.offsetMax = Vector2.zero;

            // 2. Operator (avatar + callsign / id) -------------------------
            var operatorCol = new GameObject("Operator", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            operatorCol.transform.SetParent(rowObj.transform, false);
            operatorCol.GetComponent<RectTransform>().sizeDelta = new Vector2(250 * UI_SCALE, _rowHeight);
            var opLe = operatorCol.GetComponent<LayoutElement>();
            opLe.minWidth = 250 * UI_SCALE; opLe.preferredWidth = 250 * UI_SCALE;
            opLe.minHeight = _rowHeight; opLe.preferredHeight = _rowHeight;
            var opHlg = operatorCol.GetComponent<HorizontalLayoutGroup>();
            opHlg.spacing = 10 * UI_SCALE;
            opHlg.childAlignment = TextAnchor.MiddleLeft;
            opHlg.childControlWidth = false;
            opHlg.childControlHeight = false;
            opHlg.childForceExpandWidth = false;

            var avatar = new GameObject("Avatar", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            avatar.transform.SetParent(operatorCol.transform, false);
            avatar.GetComponent<RectTransform>().sizeDelta = new Vector2(26 * UI_SCALE, 26 * UI_SCALE);
            var avLe = avatar.GetComponent<LayoutElement>();
            avLe.minWidth = 26 * UI_SCALE; avLe.preferredWidth = 26 * UI_SCALE;
            avLe.minHeight = 26 * UI_SCALE; avLe.preferredHeight = 26 * UI_SCALE;
            var avImg = avatar.GetComponent<Image>();
            avImg.sprite = LoadAvatarSprite();
            avImg.color = new Color(rankColor.r, rankColor.g, rankColor.b, 0.9f);

            var namesCol = new GameObject("Names", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            namesCol.transform.SetParent(operatorCol.transform, false);
            namesCol.GetComponent<RectTransform>().sizeDelta = new Vector2(200 * UI_SCALE, _rowHeight);
            var nmLe = namesCol.GetComponent<LayoutElement>();
            nmLe.minWidth = 200 * UI_SCALE; nmLe.preferredWidth = 200 * UI_SCALE;
            nmLe.minHeight = _rowHeight; nmLe.preferredHeight = _rowHeight;
            var nmHlg = namesCol.GetComponent<VerticalLayoutGroup>();
            nmHlg.spacing = 0 * UI_SCALE;
            nmHlg.childAlignment = TextAnchor.MiddleLeft;
            nmHlg.childControlWidth = true;
            nmHlg.childControlHeight = false;
            nmHlg.childForceExpandHeight = false;

            var nameTxt = CreateLabel(namesCol.transform, entry.PlayerName, 200, Color.white);
            nameTxt.fontSize = 13 * UI_SCALE;
            nameTxt.fontStyle = FontStyles.Bold;
            nameTxt.alignment = TextAlignmentOptions.MidlineLeft;
            nameTxt.rectTransform.sizeDelta = new Vector2(200 * UI_SCALE, 19 * UI_SCALE);

            var idTxt = CreateLabel(namesCol.transform, $"ID: {BuildOperatorId(entry.PlayerName)} · {BuildFactionTag(entry.PlayerName)}", 200, new Color(148f / 255f, 163f / 255f, 184f / 255f));
            idTxt.fontSize = 9 * UI_SCALE;
            idTxt.fontStyle = FontStyles.Normal;
            idTxt.alignment = TextAlignmentOptions.MidlineLeft;
            idTxt.rectTransform.sizeDelta = new Vector2(200 * UI_SCALE, 14 * UI_SCALE);

            // 3. Time -----------------------------------------------------
            var timeColor = rank == 1 ? new Color(251f / 255f, 191f / 255f, 36f / 255f) :
                            new Color(0f, 242f / 255f, 254f / 255f);
            var timeTxt = CreateLabel(rowObj.transform, $"{entry.TimeSeconds:000.0}s", 100, timeColor);
            timeTxt.fontSize = 14 * UI_SCALE;
            timeTxt.fontStyle = FontStyles.Bold;
            timeTxt.alignment = TextAlignmentOptions.MidlineRight;

            // 4. Efficiency (ops / sec) -----------------------------------
            var effCol = new GameObject("Efficiency", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            effCol.transform.SetParent(rowObj.transform, false);
            effCol.GetComponent<RectTransform>().sizeDelta = new Vector2(86 * UI_SCALE, _rowHeight);
            var effLe = effCol.GetComponent<LayoutElement>();
            effLe.minWidth = 86 * UI_SCALE; effLe.preferredWidth = 86 * UI_SCALE;
            effLe.minHeight = _rowHeight; effLe.preferredHeight = _rowHeight;
            var effHlg = effCol.GetComponent<VerticalLayoutGroup>();
            effHlg.spacing = 0 * UI_SCALE;
            effHlg.childAlignment = TextAnchor.MiddleLeft;
            effHlg.childControlWidth = true;
            effHlg.childControlHeight = false;
            effHlg.childForceExpandHeight = false;

            var effTxt = CreateLabel(effCol.transform, $"{entry.Efficiency:F2}", 86, ColPrimary);
            effTxt.fontSize = 13 * UI_SCALE;
            effTxt.fontStyle = FontStyles.Bold;
            effTxt.alignment = TextAlignmentOptions.MidlineLeft;
            effTxt.rectTransform.sizeDelta = new Vector2(86 * UI_SCALE, 19 * UI_SCALE);

            var opsTxt = CreateLabel(effCol.transform, $"{GetOpenCellCount(_activeTab)} ops", 86, new Color(148f / 255f, 163f / 255f, 184f / 255f));
            opsTxt.fontSize = 9 * UI_SCALE;
            opsTxt.fontStyle = FontStyles.Normal;
            opsTxt.alignment = TextAlignmentOptions.MidlineLeft;
            opsTxt.rectTransform.sizeDelta = new Vector2(86 * UI_SCALE, 14 * UI_SCALE);

            // 5. Accuracy -------------------------------------------------
            var accCol = new GameObject("Accuracy", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            accCol.transform.SetParent(rowObj.transform, false);
            accCol.GetComponent<RectTransform>().sizeDelta = new Vector2(104 * UI_SCALE, _rowHeight);
            var accLe = accCol.GetComponent<LayoutElement>();
            accLe.minWidth = 104 * UI_SCALE; accLe.preferredWidth = 104 * UI_SCALE;
            accLe.minHeight = _rowHeight; accLe.preferredHeight = _rowHeight;
            var accHlg = accCol.GetComponent<HorizontalLayoutGroup>();
            accHlg.spacing = 6 * UI_SCALE;
            accHlg.childAlignment = TextAnchor.MiddleLeft;
            accHlg.childControlWidth = false;
            accHlg.childControlHeight = false;
            accHlg.childForceExpandWidth = false;

            var accDot = new GameObject("Dot", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            accDot.transform.SetParent(accCol.transform, false);
            accDot.GetComponent<RectTransform>().sizeDelta = new Vector2(8 * UI_SCALE, 8 * UI_SCALE);
            var dotLe = accDot.GetComponent<LayoutElement>();
            dotLe.minWidth = 8 * UI_SCALE; dotLe.preferredWidth = 8 * UI_SCALE;
            dotLe.minHeight = 8 * UI_SCALE; dotLe.preferredHeight = 8 * UI_SCALE;
            var dotImg = accDot.GetComponent<Image>();
            dotImg.sprite = LoadAvatarSprite();
            dotImg.color = entry.Accuracy >= 97f ? ColPrimaryContainer :
                          entry.Accuracy >= 93f ? ColSecondary :
                          new Color(148f / 255f, 163f / 255f, 184f / 255f);

            var accTxt = CreateLabel(accCol.transform, $"{entry.Accuracy:F1}%", 88, ColPrimary);
            accTxt.fontSize = 13 * UI_SCALE;
            accTxt.fontStyle = FontStyles.Bold;
            accTxt.alignment = TextAlignmentOptions.MidlineLeft;

            // 6. Date -----------------------------------------------------
            var dateCol = new GameObject("Date", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            dateCol.transform.SetParent(rowObj.transform, false);
            dateCol.GetComponent<RectTransform>().sizeDelta = new Vector2(130 * UI_SCALE, _rowHeight);
            var dateLe = dateCol.GetComponent<LayoutElement>();
            dateLe.minWidth = 130 * UI_SCALE; dateLe.preferredWidth = 130 * UI_SCALE;
            dateLe.minHeight = _rowHeight; dateLe.preferredHeight = _rowHeight;
            var dateHlg = dateCol.GetComponent<VerticalLayoutGroup>();
            dateHlg.spacing = 0 * UI_SCALE;
            dateHlg.childAlignment = TextAnchor.MiddleLeft;
            dateHlg.childControlWidth = true;
            dateHlg.childControlHeight = false;
            dateHlg.childForceExpandHeight = false;

            var dateTxt = CreateLabel(dateCol.transform, entry.Date, 130, new Color(203f / 255f, 213f / 255f, 225f / 255f));
            dateTxt.fontSize = 12 * UI_SCALE;
            dateTxt.fontStyle = FontStyles.Normal;
            dateTxt.alignment = TextAlignmentOptions.MidlineLeft;
            dateTxt.rectTransform.sizeDelta = new Vector2(130 * UI_SCALE, 19 * UI_SCALE);

            var timeOfDay = CreateLabel(dateCol.transform, $"{HashName(entry.PlayerName) % 24:00}:{HashName(entry.PlayerName + "t") % 60:00} UTC", 130, new Color(148f / 255f, 163f / 255f, 184f / 255f));
            timeOfDay.fontSize = 9 * UI_SCALE;
            timeOfDay.fontStyle = FontStyles.Normal;
            timeOfDay.alignment = TextAlignmentOptions.MidlineLeft;
            timeOfDay.rectTransform.sizeDelta = new Vector2(130 * UI_SCALE, 14 * UI_SCALE);

            // 7. Replay ----------------------------------------------------
            var replayGo = new GameObject("ReplayButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            replayGo.transform.SetParent(rowObj.transform, false);
            replayGo.GetComponent<RectTransform>().sizeDelta = new Vector2(30 * UI_SCALE, 26 * UI_SCALE);
            var repLe = replayGo.GetComponent<LayoutElement>();
            repLe.minWidth = 30 * UI_SCALE; repLe.preferredWidth = 30 * UI_SCALE;
            repLe.minHeight = 26 * UI_SCALE; repLe.preferredHeight = 26 * UI_SCALE;
            var repImg = replayGo.GetComponent<Image>();
            repImg.color = rank == 1 ? new Color(0f, 242f / 255f, 254f / 255f, 0.16f) : ColSurface;
            replayGo.AddComponent<Button>();

            var repIconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            repIconGo.transform.SetParent(replayGo.transform, false);
            var repIconRt = repIconGo.GetComponent<RectTransform>();
            repIconRt.anchorMin = new Vector2(0.5f, 0.5f);
            repIconRt.anchorMax = new Vector2(0.5f, 0.5f);
            repIconRt.sizeDelta = new Vector2(13 * UI_SCALE, 13 * UI_SCALE);
            repIconRt.anchoredPosition = Vector2.zero;
            var repIconImg = repIconGo.GetComponent<Image>();
            repIconImg.sprite = LoadReplaySprite();
            repIconImg.color = rank == 1 ? ColPrimaryContainer : ColOnSurfaceVariant;
            repIconImg.raycastTarget = false;

            return rowObj;
        }

        private Sprite LoadAvatarSprite()
        {
            return _avatarSprite;
        }

        private Sprite LoadReplaySprite()
        {
            return _playSprite != null ? _playSprite : _replaySprite;
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string text, float width, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (_fontAsset != null) tmp.font = _fontAsset;
            tmp.text = text;
            tmp.fontSize = 13 * UI_SCALE;
            tmp.color = color;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(width * UI_SCALE, 24 * UI_SCALE);
            var le = go.GetComponent<LayoutElement>();
            le.minWidth = width * UI_SCALE;
            le.preferredWidth = width * UI_SCALE;
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

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

        [Header("Rows Container")]
        [SerializeField] private Transform _rowsContainer;
        [SerializeField] private GameObject _rowPrefab;
        [SerializeField] private GameObject _customNoticeObject;

        [Header("Best Result Banner")]
        [SerializeField] private TextMeshProUGUI _bestResultText;

        [Header("Actions")]
        [SerializeField] private Button _resetScoresButton;
        [SerializeField] private Button _closeButton;

        private DifficultyLevel _activeTab = DifficultyLevel.Beginner;

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

        private void SwitchTab(DifficultyLevel level)
        {
            _activeTab = level;
            PopulateTable();
        }

        public void PopulateTable()
        {
            if (GameController.Instance == null) return;
            var highScores = GameController.Instance.HighScores;

            if (_activeTab == DifficultyLevel.Custom)
            {
                if (_rowsContainer != null) _rowsContainer.gameObject.SetActive(false);
                if (_customNoticeObject != null) _customNoticeObject.SetActive(true);
                if (_bestResultText != null) _bestResultText.text = "ОСОБЫЙ РЕЖИМ НЕ УЧИТЫВАЕТСЯ В РЕЕСТРЕ РЕКОРДОВ";
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
                        var rowObj = _rowPrefab != null ? Instantiate(_rowPrefab, _rowsContainer) : CreateFallbackRow(i + 1, entry);
                        rowObj.SetActive(true);

                        var texts = rowObj.GetComponentsInChildren<TextMeshProUGUI>();
                        if (texts.Length >= 4)
                        {
                            texts[0].text = $"0{i + 1}";
                            texts[1].text = entry.PlayerName;
                            texts[2].text = $"{entry.TimeSeconds:F1}s";
                            texts[3].text = entry.Date;
                        }
                    }
                }
            }

            if (_bestResultText != null)
            {
                if (list != null && list.Count > 0)
                {
                    _bestResultText.text = $"ВАШ ЛУЧШИЙ РЕЗУЛЬТАТ: {list[0].TimeSeconds:F1}s [РАНГ #1 - {list[0].PlayerName}]";
                }
                else
                {
                    _bestResultText.text = "РЕКОРДОВ ПОКА НЕТ";
                }
            }
        }

        private GameObject CreateFallbackRow(int rank, HighScoreEntry entry)
        {
            var rowObj = new GameObject($"Row_{rank}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            rowObj.transform.SetParent(_rowsContainer, false);
            var hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;

            CreateLabel(rowObj.transform, $"0{rank}", 40, Color.cyan);
            CreateLabel(rowObj.transform, entry.PlayerName, 160, Color.white);
            CreateLabel(rowObj.transform, $"{entry.TimeSeconds:F1}s", 80, Color.cyan);
            CreateLabel(rowObj.transform, entry.Date, 100, Color.gray);
            return rowObj;
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string text, float width, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 14;
            tmp.color = color;
            var le = go.AddComponent<LayoutElement>();
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

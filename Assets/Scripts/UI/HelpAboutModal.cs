using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minesweeper.Core;

namespace Minesweeper.UI
{
    public class HelpAboutModal : MonoBehaviour
    {
        [Header("Modal Root")]
        [SerializeField] private GameObject _panel;

        [Header("Content Containers")]
        [SerializeField] private GameObject _rulesContainer;
        [SerializeField] private GameObject _aboutContainer;

        [Header("Header Title")]
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Buttons")]
        [SerializeField] private Button _tabRulesButton;
        [SerializeField] private Button _tabAboutButton;
        [SerializeField] private Image _tabRulesBg;
        [SerializeField] private Image _tabAboutBg;
        [SerializeField] private Button _closeButton;

        private static readonly Color ColorActiveTab = new Color(0f, 242f / 255f, 254f / 255f, 0.25f);
        private static readonly Color ColorInactiveTab = new Color(13f / 255f, 17f / 255f, 26f / 255f, 0.9f);

        private void Awake()
        {
            if (_tabRulesButton != null) _tabRulesButton.onClick.AddListener(ShowRules);
            if (_tabAboutButton != null) _tabAboutButton.onClick.AddListener(ShowAbout);
            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
        }

        public bool IsOpen => _panel != null ? _panel.activeSelf : gameObject.activeSelf;

        private void Start()
        {
            Hide();
        }

        public void Show(bool showAbout = false)
        {
            if (_panel == null) _panel = gameObject;
            _panel.SetActive(true);

            if (GameController.Instance != null)
            {
                GameController.Instance.PauseGame();
            }

            if (showAbout) ShowAbout();
            else ShowRules();
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

        public void ShowRules()
        {
            if (_titleText != null) _titleText.text = "ПРАВИЛА ИГРЫ // СПРАВОЧНЫЙ МОДУЛЬ";
            if (_rulesContainer != null) _rulesContainer.SetActive(true);
            if (_aboutContainer != null) _aboutContainer.SetActive(false);

            if (_tabRulesBg != null) _tabRulesBg.color = ColorActiveTab;
            if (_tabAboutBg != null) _tabAboutBg.color = ColorInactiveTab;
        }

        public void ShowAbout()
        {
            if (_titleText != null) _titleText.text = "О ПРОГРАММЕ // ТАКТИЧЕСКИЙ САПЁР HUD";
            if (_rulesContainer != null) _rulesContainer.SetActive(false);
            if (_aboutContainer != null) _aboutContainer.SetActive(true);

            if (_tabRulesBg != null) _tabRulesBg.color = ColorInactiveTab;
            if (_tabAboutBg != null) _tabAboutBg.color = ColorActiveTab;
        }
    }
}

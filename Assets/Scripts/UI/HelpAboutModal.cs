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
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            if (_tabRulesButton != null) _tabRulesButton.onClick.AddListener(ShowRules);
            if (_tabAboutButton != null) _tabAboutButton.onClick.AddListener(ShowAbout);
            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
        }

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
        }

        public void ShowAbout()
        {
            if (_titleText != null) _titleText.text = "О ПРОГРАММЕ // ТАКТИЧЕСКИЙ САПЁР HUD";
            if (_rulesContainer != null) _rulesContainer.SetActive(false);
            if (_aboutContainer != null) _aboutContainer.SetActive(true);
        }
    }
}

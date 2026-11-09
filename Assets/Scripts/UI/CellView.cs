using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Minesweeper.Core;

namespace Minesweeper.UI
{
    public class CellView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image _bgImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _numberText;

        [Header("Cell State Sprites")]
        public Sprite ClosedSprite;
        public Sprite HoverSprite;
        public Sprite PressedSprite;
        public Sprite RevealedSprite;
        public Sprite ExplodedSprite;

        [Header("Icon Sprites")]
        public Sprite FlagSprite;
        public Sprite MineSprite;
        public Sprite FalseFlagSprite;

        public int X { get; private set; }
        public int Y { get; private set; }

        private bool _isHovered = false;
        private bool _isPressed = false;
        private CellData _currentData;
        private Coroutine _punchCoroutine;

        // Canonical colors from GDD & Cyberpunk HUD Design
        private static readonly Color Color1 = new Color(0f / 255f, 242f / 255f, 254f / 255f);     // #00F2FE Cyan
        private static readonly Color Color2 = new Color(0f / 255f, 255f / 255f, 102f / 255f);     // #00FF66 Green
        private static readonly Color Color3 = new Color(255f / 255f, 0f / 255f, 85f / 255f);      // #FF0055 Crimson
        private static readonly Color Color4 = new Color(99f / 255f, 102f / 255f, 241f / 255f);    // #6366F1 Indigo
        private static readonly Color Color5 = new Color(245f / 255f, 158f / 255f, 11f / 255f);    // #F59E0B Amber
        private static readonly Color Color6 = new Color(20f / 255f, 184f / 255f, 166f / 255f);    // #14B8A6 Teal
        private static readonly Color Color7 = new Color(168f / 255f, 85f / 255f, 247f / 255f);    // #A855F7 Violet
        private static readonly Color Color8 = new Color(100f / 255f, 116f / 255f, 139f / 255f);   // #64748B Slate

        public void Configure(Image bg, Image icon, TextMeshProUGUI num, Sprite closed, Sprite hover, Sprite pressed, Sprite revealed, Sprite exploded, Sprite flag, Sprite mine, Sprite falseFlag)
        {
            _bgImage = bg;
            _iconImage = icon;
            _numberText = num;
            ClosedSprite = closed;
            HoverSprite = hover;
            PressedSprite = pressed;
            RevealedSprite = revealed;
            ExplodedSprite = exploded;
            FlagSprite = flag;
            MineSprite = mine;
            FalseFlagSprite = falseFlag;
        }

        public void Initialize(int x, int y, float cellSize = 40f)
        {
            X = x;
            Y = y;
            _isHovered = false;
            _isPressed = false;
            transform.localScale = Vector3.one;

            if (_numberText != null)
            {
                _numberText.fontSize = Mathf.RoundToInt(cellSize * 0.58f);
            }
            if (_iconImage != null)
            {
                _iconImage.rectTransform.sizeDelta = new Vector2(cellSize * 0.7f, cellSize * 0.7f);
            }

            UpdateVisuals(new CellData(x, y));
        }

        public void UpdateVisuals(CellData data)
        {
            CellState oldState = _currentData != null ? _currentData.State : CellState.Closed;
            _currentData = data;
            if (_bgImage == null) return;

            // Reset overlays
            _bgImage.color = Color.white;
            if (_iconImage != null) _iconImage.gameObject.SetActive(false);
            if (_numberText != null) _numberText.text = "";

            switch (data.State)
            {
                case CellState.Closed:
                    _bgImage.sprite = _isPressed ? PressedSprite : (_isHovered ? HoverSprite : ClosedSprite);
                    break;

                case CellState.Flagged:
                    _bgImage.sprite = ClosedSprite;
                    if (_iconImage != null)
                    {
                        _iconImage.sprite = FlagSprite;
                        _iconImage.gameObject.SetActive(true);
                    }
                    if (oldState != CellState.Flagged)
                    {
                        TriggerPunch(1.22f, 1.0f, 0.12f);
                    }
                    break;

                case CellState.Revealed:
                    _bgImage.sprite = RevealedSprite;
                    if (data.IsMine)
                    {
                        if (_iconImage != null)
                        {
                            _iconImage.sprite = MineSprite;
                            _iconImage.gameObject.SetActive(true);
                        }
                    }
                    else if (data.AdjacentMines > 0)
                    {
                        if (_numberText != null)
                        {
                            _numberText.text = data.AdjacentMines.ToString();
                            _numberText.color = GetNumberColor(data.AdjacentMines);
                        }
                    }

                    if (oldState == CellState.Closed)
                    {
                        TriggerPunch(0.82f, 1.0f, 0.14f);
                    }
                    break;

                case CellState.Exploded:
                    _bgImage.sprite = ExplodedSprite != null ? ExplodedSprite : RevealedSprite;
                    if (_iconImage != null)
                    {
                        _iconImage.sprite = MineSprite;
                        _iconImage.gameObject.SetActive(true);
                    }
                    TriggerPunch(1.35f, 1.0f, 0.22f);
                    break;

                case CellState.FalseFlag:
                    _bgImage.sprite = RevealedSprite;
                    if (_iconImage != null)
                    {
                        _iconImage.sprite = FalseFlagSprite;
                        _iconImage.gameObject.SetActive(true);
                    }
                    break;
            }
        }

        private Color GetNumberColor(int number)
        {
            switch (number)
            {
                case 1: return Color1;
                case 2: return Color2;
                case 3: return Color3;
                case 4: return Color4;
                case 5: return Color5;
                case 6: return Color6;
                case 7: return Color7;
                case 8: return Color8;
                default: return Color.white;
            }
        }

        private bool _isLeftDown = false;
        private bool _isRightDown = false;

        public void HighlightHint()
        {
            if (_bgImage != null)
            {
                _bgImage.sprite = HoverSprite != null ? HoverSprite : ClosedSprite;
                _bgImage.color = new Color(251f / 255f, 191f / 255f, 36f / 255f); // Amber glow highlight
            }
            TriggerPunch(1.35f, 1.0f, 0.45f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _isLeftDown = true;
                _isPressed = true;
                if (_currentData != null && _currentData.State == CellState.Closed)
                {
                    _bgImage.sprite = PressedSprite;
                    if (GameController.Instance != null)
                    {
                        GameController.Instance.SetMouseDownOnBoard(true);
                    }
                }
                else if (_isRightDown && _currentData != null && _currentData.State == CellState.Revealed)
                {
                    // Simultaneous LMB + RMB chord (FR-010, GDD 2.1.2)
                    GameController.Instance?.OnCellChordClick(X, Y);
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                _isRightDown = true;
                if (_isLeftDown && _currentData != null && _currentData.State == CellState.Revealed)
                {
                    // Simultaneous LMB + RMB chord (FR-010, GDD 2.1.2)
                    GameController.Instance?.OnCellChordClick(X, Y);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _isLeftDown = false;
                _isPressed = false;
                if (GameController.Instance != null)
                {
                    GameController.Instance.SetMouseDownOnBoard(false);
                }
                if (_currentData != null && _currentData.State == CellState.Closed)
                {
                    _bgImage.sprite = _isHovered ? HoverSprite : ClosedSprite;
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                _isRightDown = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (GameController.Instance == null) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (eventData.clickCount >= 2 && _currentData != null && _currentData.State == CellState.Revealed)
                {
                    // Chording on double left click (FR-010)
                    GameController.Instance.OnCellChordClick(X, Y);
                }
                else
                {
                    GameController.Instance.OnCellLeftClick(X, Y);
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                GameController.Instance.OnCellRightClick(X, Y);
            }
            else if (eventData.button == PointerEventData.InputButton.Middle)
            {
                // Chording on middle mouse button (FR-010)
                GameController.Instance.OnCellChordClick(X, Y);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            if (_currentData != null && _currentData.State == CellState.Closed && !_isPressed)
            {
                _bgImage.sprite = HoverSprite;
                TriggerPunch(1.0f, 1.05f, 0.08f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            if (_currentData != null && _currentData.State == CellState.Closed && !_isPressed)
            {
                _bgImage.sprite = ClosedSprite;
                TriggerPunch(transform.localScale.x, 1.0f, 0.08f);
            }
        }

        private void TriggerPunch(float fromScale, float toScale, float duration)
        {
            if (!gameObject.activeInHierarchy) return;
            if (_punchCoroutine != null) StopCoroutine(_punchCoroutine);
            _punchCoroutine = StartCoroutine(PunchRoutine(fromScale, toScale, duration));
        }

        private IEnumerator PunchRoutine(float fromScale, float toScale, float duration)
        {
            float elapsed = 0f;
            transform.localScale = Vector3.one * fromScale;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float s = Mathf.Lerp(fromScale, toScale, Mathf.Sin(t * Mathf.PI * 0.5f));
                transform.localScale = Vector3.one * s;
                yield return null;
            }
            transform.localScale = Vector3.one * toScale;
            _punchCoroutine = null;
        }
    }
}

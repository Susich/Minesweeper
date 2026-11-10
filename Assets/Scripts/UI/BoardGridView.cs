using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Minesweeper.Core;

namespace Minesweeper.UI
{
    public class BoardGridView : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private RectTransform _gridContainer;
        [SerializeField] private GridLayoutGroup _gridLayout;
        [SerializeField] private RectTransform _consoleFrame;

        [Header("Cell Prefab")]
        [SerializeField] private GameObject _cellPrefab;

        [Header("Cell Visual Resources")]
        public Sprite ClosedSprite;
        public Sprite HoverSprite;
        public Sprite PressedSprite;
        public Sprite RevealedSprite;
        public Sprite ExplodedSprite;
        public Sprite FlagSprite;
        public Sprite MineSprite;
        public Sprite FalseFlagSprite;
        public TMPro.TMP_FontAsset FontAsset;

        private CellView[,] _cellViews;
        private int _currentWidth = 0;
        private int _currentHeight = 0;

        private void Start()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnGameInitialized += OnGameInitialized;
                GameController.Instance.OnCellUpdated += OnCellUpdated;
                GameController.Instance.OnMultipleCellsUpdated += OnMultipleCellsUpdated;
                GameController.Instance.OnDetonated += OnDetonated;
                GameController.Instance.OnHintGiven += OnHintGiven;

                if (GameController.Instance.CurrentDifficulty != null)
                {
                    OnGameInitialized(GameController.Instance.CurrentDifficulty);
                }
            }
        }

        private void OnDestroy()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.OnGameInitialized -= OnGameInitialized;
                GameController.Instance.OnCellUpdated -= OnCellUpdated;
                GameController.Instance.OnMultipleCellsUpdated -= OnMultipleCellsUpdated;
                GameController.Instance.OnDetonated -= OnDetonated;
                GameController.Instance.OnHintGiven -= OnHintGiven;
            }
        }

        private void OnHintGiven(CellData cellData)
        {
            if (cellData != null && cellData.X >= 0 && cellData.X < _currentWidth && cellData.Y >= 0 && cellData.Y < _currentHeight)
            {
                _cellViews[cellData.X, cellData.Y]?.HighlightHint();
            }
        }

        public void ConfigureResources(Sprite closed, Sprite hover, Sprite pressed, Sprite revealed, Sprite exploded, Sprite flag, Sprite mine, Sprite falseFlag, TMPro.TMP_FontAsset font)
        {
            ClosedSprite = closed;
            HoverSprite = hover;
            PressedSprite = pressed;
            RevealedSprite = revealed;
            ExplodedSprite = exploded;
            FlagSprite = flag;
            MineSprite = mine;
            FalseFlagSprite = falseFlag;
            FontAsset = font;
        }

        public void SetCellPrefab(GameObject prefab)
        {
            _cellPrefab = prefab;
        }

        public void SetConsoleFrame(RectTransform frame)
        {
            _consoleFrame = frame;
        }

        private void OnGameInitialized(GameDifficulty difficulty)
        {
            BuildGrid(difficulty.Width, difficulty.Height);
        }

        public void BuildGrid(int width, int height)
        {
            if (_gridContainer == null) _gridContainer = GetComponent<RectTransform>();

            if (_cellPrefab == null)
            {
                _cellPrefab = CreateCellTemplate();
            }

            // Clean up existing cells
            if (_cellViews != null)
            {
                for (int x = 0; x < _currentWidth; x++)
                {
                    for (int y = 0; y < _currentHeight; y++)
                    {
                        if (_cellViews[x, y] != null)
                        {
                            Destroy(_cellViews[x, y].gameObject);
                        }
                    }
                }
            }

            _currentWidth = width;
            _currentHeight = height;
            _cellViews = new CellView[width, height];

            float cellSize = AdjustGridAndFrame(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cellObj = Instantiate(_cellPrefab, _gridContainer);
                    cellObj.name = $"Cell_{x}_{y}";
                    cellObj.SetActive(true);

                    var cellView = cellObj.GetComponent<CellView>();
                    cellView.Initialize(x, y, cellSize);
                    _cellViews[x, y] = cellView;
                }
            }
        }

        private float AdjustGridAndFrame(int width, int height)
        {
            float spacing = 3f;

            // Available area inside GridWrapper (deck is 570px tall, grid area is ~430px)
            float availW = 900f;
            float availH = 430f;

            float maxCellW = (availW - (spacing * (width - 1))) / width;
            float maxCellH = (availH - (spacing * (height - 1))) / height;

            // Clamped: between 22 and 44 px with integer pixel snap for crisp rendering
            float cellSize = Mathf.Floor(Mathf.Clamp(Mathf.Min(maxCellW, maxCellH), 22f, 44f));

            if (_gridLayout != null)
            {
                _gridLayout.cellSize = new Vector2(cellSize, cellSize);
                _gridLayout.spacing = new Vector2(spacing, spacing);
                _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                _gridLayout.constraintCount = width;
            }

            float totalGridW = width * cellSize + (width - 1) * spacing;
            float totalGridH = height * cellSize + (height - 1) * spacing;

            if (_gridContainer != null)
            {
                _gridContainer.sizeDelta = new Vector2(totalGridW, totalGridH);
                _gridContainer.anchoredPosition = Vector2.zero;
            }

            // Keep deck width responsive (between 820px and 1000px) and fixed height 570px (never overlaps top bar)
            if (_consoleFrame != null)
            {
                float deckW = Mathf.Clamp(totalGridW + 64f, 820f, 1000f);
                _consoleFrame.sizeDelta = new Vector2(deckW, 570f);
            }

            return cellSize;
        }

        private void OnCellUpdated(CellData cellData)
        {
            if (cellData != null && cellData.X >= 0 && cellData.X < _currentWidth && cellData.Y >= 0 && cellData.Y < _currentHeight)
            {
                _cellViews[cellData.X, cellData.Y]?.UpdateVisuals(cellData);
            }
        }

        private void OnMultipleCellsUpdated(List<CellData> cells)
        {
            if (cells == null) return;
            for (int i = 0; i < cells.Count; i++)
            {
                OnCellUpdated(cells[i]);
            }
        }

        private void OnDetonated(CellData hitCell)
        {
            if (hitCell != null)
            {
                OnCellUpdated(hitCell);
            }
        }

        private GameObject CreateCellTemplate()
        {
            var go = new GameObject("CellTemplate", typeof(RectTransform), typeof(Image), typeof(CellView));
            go.SetActive(false);
            go.transform.SetParent(transform, false);

            var img = go.GetComponent<Image>();
            img.sprite = ClosedSprite;
            img.type = Image.Type.Simple;

            var cellView = go.GetComponent<CellView>();
            cellView.ClosedSprite = ClosedSprite;
            cellView.HoverSprite = HoverSprite;
            cellView.PressedSprite = PressedSprite;
            cellView.RevealedSprite = RevealedSprite;
            cellView.ExplodedSprite = ExplodedSprite;
            cellView.FlagSprite = FlagSprite;
            cellView.MineSprite = MineSprite;
            cellView.FalseFlagSprite = FalseFlagSprite;

            // Icon Child
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.sizeDelta = new Vector2(24, 24);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.raycastTarget = false;
            iconImg.preserveAspect = true;
            iconGo.SetActive(false);

            // Number Text Child
            var textGo = new GameObject("NumberText", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var tmp = textGo.GetComponent<TMPro.TextMeshProUGUI>();
            if (FontAsset != null) tmp.font = FontAsset;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontSize = 18;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            tmp.overflowMode = TMPro.TextOverflowModes.Overflow;
            textGo.SetActive(false);

            // Wire serialized fields on CellView
            var bgField = typeof(CellView).GetField("_bgImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bgField?.SetValue(cellView, img);

            var iconField = typeof(CellView).GetField("_iconImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            iconField?.SetValue(cellView, iconImg);

            var textField = typeof(CellView).GetField("_numberText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            textField?.SetValue(cellView, tmp);

            return go;
        }
    }
}

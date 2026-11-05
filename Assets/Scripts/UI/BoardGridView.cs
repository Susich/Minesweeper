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
            // Tactical Console Deck width is fixed to 940px (reference max-w-5xl)
            float consoleWidth = 940f;
            float spacing = 2f;

            float maxCellW = (consoleWidth - 64f - (spacing * (width - 1))) / width;
            float maxCellH = (440f - (spacing * (height - 1))) / height;

            // Clamped: between 20 and 44 px with integer pixel snap
            float cellSize = Mathf.Floor(Mathf.Clamp(Mathf.Min(maxCellW, maxCellH), 20f, 44f));

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

                // Adjust wrapper to wrap tightly without huge empty gaps
                if (_gridContainer.parent != null)
                {
                    var wrapperRt = _gridContainer.parent.GetComponent<RectTransform>();
                    if (wrapperRt != null)
                    {
                        wrapperRt.sizeDelta = new Vector2(totalGridW, totalGridH);
                        var le = wrapperRt.GetComponent<LayoutElement>();
                        if (le != null)
                        {
                            le.preferredHeight = totalGridH + 16f;
                        }
                    }
                }
            }

            // Maintain wide command deck frame (940px)
            if (_consoleFrame != null)
            {
                float frameW = Mathf.Max(consoleWidth, totalGridW + 64f);
                float frameH = totalGridH + 212f; // Header telemetry 34 + Status bay 76 + Footer 32 + Padding & Margins 70
                _consoleFrame.sizeDelta = new Vector2(frameW, frameH);
            }

            return cellSize;
        }

        private void OnCellUpdated(CellData cellData)
        {
            if (cellData == null) return;
            if (cellData.X >= 0 && cellData.X < _currentWidth && cellData.Y >= 0 && cellData.Y < _currentHeight)
            {
                _cellViews[cellData.X, cellData.Y]?.UpdateVisuals(cellData);
            }
        }

        private void OnMultipleCellsUpdated(List<CellData> cells)
        {
            if (cells == null) return;
            foreach (var c in cells)
            {
                OnCellUpdated(c);
            }
        }

        private void OnDetonated(CellData detonatedCell)
        {
            if (ScreenShake.Instance != null)
            {
                ScreenShake.Instance.Shake(0.4f, 15f);
            }
        }

        private GameObject CreateCellTemplate()
        {
            var go = new GameObject("CellTemplate", typeof(RectTransform), typeof(Image), typeof(CellView));
            go.transform.SetParent(transform, false);
            go.SetActive(false);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(48, 48);

            var img = go.GetComponent<Image>();
            img.sprite = ClosedSprite;
            img.type = Image.Type.Simple;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.sizeDelta = new Vector2(32, 32);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.raycastTarget = false;
            iconGo.SetActive(false);

            var numGo = new GameObject("NumberText", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            numGo.transform.SetParent(go.transform, false);
            var numRt = numGo.GetComponent<RectTransform>();
            numRt.anchorMin = Vector2.zero;
            numRt.anchorMax = Vector2.one;
            numRt.offsetMin = Vector2.zero;
            numRt.offsetMax = Vector2.zero;
            var numTmp = numGo.GetComponent<TMPro.TextMeshProUGUI>();
            if (FontAsset != null) numTmp.font = FontAsset;
            numTmp.fontSize = 24;
            numTmp.fontStyle = TMPro.FontStyles.Bold;
            numTmp.alignment = TMPro.TextAlignmentOptions.Center;
            numTmp.raycastTarget = false;
            numTmp.text = "";

            var cv = go.GetComponent<CellView>();
            cv.Configure(img, iconImg, numTmp, ClosedSprite, HoverSprite, PressedSprite, RevealedSprite, ExplodedSprite, FlagSprite, MineSprite, FalseFlagSprite);

            return go;
        }
    }
}

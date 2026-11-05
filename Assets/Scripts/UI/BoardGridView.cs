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
            // Calculate optimal cell size so board is prominent, comfortable, and fits screen
            float availW = 900f;
            float availH = 480f;

            float spacing = 2f;
            float maxCellW = (availW - (spacing * (width - 1))) / width;
            float maxCellH = (availH - (spacing * (height - 1))) / height;

            // Clamped: between 22 and 54 px
            float cellSize = Mathf.Clamp(Mathf.Min(maxCellW, maxCellH), 22f, 54f);

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
            }

            // Dynamically adjust console frame to snugly wrap around grid (GDD 2.1.3)
            if (_consoleFrame != null)
            {
                float frameW = Mathf.Max(620f, totalGridW + 48f);
                float frameH = totalGridH + 195f; // Header telemetry 36 + Status bay 78 + Footer 32 + Margins 49
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
    }
}

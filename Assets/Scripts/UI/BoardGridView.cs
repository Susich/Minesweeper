using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Minesweeper.Core;

namespace Minesweeper.UI
{
    public class BoardGridView : MonoBehaviour
    {
        [Header("Grid Container")]
        [SerializeField] private RectTransform _gridContainer;
        [SerializeField] private GridLayoutGroup _gridLayout;

        [Header("Cell Prefab / Template")]
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

            AdjustGridSize(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cellObj = Instantiate(_cellPrefab, _gridContainer);
                    cellObj.name = $"Cell_{x}_{y}";
                    cellObj.SetActive(true);

                    var cellView = cellObj.GetComponent<CellView>();
                    cellView.Initialize(x, y);
                    _cellViews[x, y] = cellView;
                }
            }
        }

        private void AdjustGridSize(int width, int height)
        {
            if (_gridContainer == null || _gridLayout == null) return;

            // Total available display area inside HUD
            Rect parentRect = _gridContainer.rect;
            float availW = parentRect.width > 100 ? parentRect.width : 800f;
            float availH = parentRect.height > 100 ? parentRect.height : 520f;

            float spacing = 2f;
            float maxCellW = (availW - (spacing * (width - 1))) / width;
            float maxCellH = (availH - (spacing * (height - 1))) / height;
            float cellSize = Mathf.Clamp(Mathf.Min(maxCellW, maxCellH), 16f, 44f);

            _gridLayout.cellSize = new Vector2(cellSize, cellSize);
            _gridLayout.spacing = new Vector2(spacing, spacing);
            _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _gridLayout.constraintCount = width;

            // Center grid inside container
            float totalW = width * cellSize + (width - 1) * spacing;
            float totalH = height * cellSize + (height - 1) * spacing;
            _gridContainer.sizeDelta = new Vector2(totalW, totalH);
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

using System;
using System.Collections.Generic;

namespace Minesweeper.Core
{
    public struct RevealResult
    {
        public bool IsMineDetonated;
        public bool IsVictory;
        public List<CellData> RevealedCells;
        public CellData DetonatedCell;
    }

    public class MinesweeperBoard
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TotalMines { get; private set; }
        public int FlagsCount { get; private set; }
        public int RevealedSafeCount { get; private set; }
        public bool IsGenerated { get; private set; }

        private CellData[,] _grid;

        public MinesweeperBoard(int width, int height, int mines)
        {
            Initialize(width, height, mines);
        }

        public void Initialize(int width, int height, int mines)
        {
            Width = width;
            Height = height;
            TotalMines = mines;
            FlagsCount = 0;
            RevealedSafeCount = 0;
            IsGenerated = false;

            _grid = new CellData[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _grid[x, y] = new CellData(x, y);
                }
            }
        }

        public CellData GetCell(int x, int y)
        {
            if (IsInBounds(x, y))
                return _grid[x, y];
            return null;
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public void GenerateBoard(int firstClickX, int firstClickY, Random customRng = null)
        {
            var rng = customRng ?? new Random();
            var eligibleCells = new List<CellData>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // FR-004: Safe first click: clicked cell and all 8 neighbors cannot contain a mine
                    bool isProtected = Math.Abs(x - firstClickX) <= 1 && Math.Abs(y - firstClickY) <= 1;
                    if (!isProtected)
                    {
                        eligibleCells.Add(_grid[x, y]);
                    }
                }
            }

            // Fallback for edge cases where mines > eligible
            if (eligibleCells.Count < TotalMines)
            {
                eligibleCells.Clear();
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (x != firstClickX || y != firstClickY)
                            eligibleCells.Add(_grid[x, y]);
                    }
                }
            }

            // Fisher-Yates shuffle
            for (int i = eligibleCells.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var temp = eligibleCells[i];
                eligibleCells[i] = eligibleCells[j];
                eligibleCells[j] = temp;
            }

            int placedMines = 0;
            int minesToPlace = Math.Min(TotalMines, eligibleCells.Count);
            for (int i = 0; i < minesToPlace; i++)
            {
                eligibleCells[i].IsMine = true;
                placedMines++;
            }
            TotalMines = placedMines;

            // Calculate adjacent mines (FR-005)
            CalculateAdjacentMines();

            IsGenerated = true;
        }

        public void CalculateAdjacentMines()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_grid[x, y].IsMine)
                    {
                        _grid[x, y].AdjacentMines = -1;
                        continue;
                    }

                    int count = 0;
                    foreach (var neighbor in GetNeighbors(x, y))
                    {
                        if (neighbor.IsMine)
                            count++;
                    }
                    _grid[x, y].AdjacentMines = count;
                }
            }
        }

        public IEnumerable<CellData> GetNeighbors(int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (IsInBounds(nx, ny))
                    {
                        yield return _grid[nx, ny];
                    }
                }
            }
        }

        public RevealResult Reveal(int x, int y)
        {
            var result = new RevealResult
            {
                IsMineDetonated = false,
                IsVictory = false,
                RevealedCells = new List<CellData>()
            };

            if (!IsInBounds(x, y)) return result;
            var cell = _grid[x, y];

            // FR-009: Flagged cell blocked from opening
            if (cell.State != CellState.Closed)
                return result;

            if (cell.IsMine)
            {
                cell.State = CellState.Exploded;
                result.IsMineDetonated = true;
                result.DetonatedCell = cell;
                result.RevealedCells.Add(cell);

                RevealAllMinesOnDefeat(x, y);

                for (int gx = 0; gx < Width; gx++)
                {
                    for (int gy = 0; gy < Height; gy++)
                    {
                        var c = _grid[gx, gy];
                        if (c.State == CellState.Exploded || c.State == CellState.FalseFlag || (c.IsMine && c.State == CellState.Revealed))
                        {
                            if (!result.RevealedCells.Contains(c))
                            {
                                result.RevealedCells.Add(c);
                            }
                        }
                    }
                }
                return result;
            }

            // BFS Cascade reveal for empty cells (FR-007)
            var queue = new Queue<CellData>();
            cell.State = CellState.Revealed;
            RevealedSafeCount++;
            result.RevealedCells.Add(cell);

            if (cell.AdjacentMines == 0)
            {
                queue.Enqueue(cell);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in GetNeighbors(current.X, current.Y))
                {
                    if (neighbor.State == CellState.Closed && !neighbor.IsMine)
                    {
                        neighbor.State = CellState.Revealed;
                        RevealedSafeCount++;
                        result.RevealedCells.Add(neighbor);

                        if (neighbor.AdjacentMines == 0)
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            // Check victory condition (FR-019)
            int totalSafeCells = (Width * Height) - TotalMines;
            if (RevealedSafeCount >= totalSafeCells)
            {
                result.IsVictory = true;
                FlagAllMinesOnVictory();
                for (int gx = 0; gx < Width; gx++)
                {
                    for (int gy = 0; gy < Height; gy++)
                    {
                        var c = _grid[gx, gy];
                        if (c.IsMine && !result.RevealedCells.Contains(c))
                        {
                            result.RevealedCells.Add(c);
                        }
                    }
                }
            }

            return result;
        }

        public int ToggleFlag(int x, int y)
        {
            if (!IsInBounds(x, y)) return 0;
            var cell = _grid[x, y];

            if (cell.State == CellState.Closed)
            {
                cell.State = CellState.Flagged;
                FlagsCount++;
                return 1;
            }
            else if (cell.State == CellState.Flagged)
            {
                cell.State = CellState.Closed;
                FlagsCount--;
                return -1;
            }
            return 0;
        }

        public RevealResult Chord(int x, int y)
        {
            var result = new RevealResult
            {
                IsMineDetonated = false,
                IsVictory = false,
                RevealedCells = new List<CellData>()
            };

            if (!IsInBounds(x, y)) return result;
            var cell = _grid[x, y];

            if (cell.State != CellState.Revealed || cell.AdjacentMines <= 0)
                return result;

            int flagCount = 0;
            var closedNeighbors = new List<CellData>();

            foreach (var neighbor in GetNeighbors(x, y))
            {
                if (neighbor.State == CellState.Flagged)
                    flagCount++;
                else if (neighbor.State == CellState.Closed)
                    closedNeighbors.Add(neighbor);
            }

            if (flagCount == cell.AdjacentMines)
            {
                foreach (var neighbor in closedNeighbors)
                {
                    var subResult = Reveal(neighbor.X, neighbor.Y);
                    if (subResult.RevealedCells != null)
                    {
                        result.RevealedCells.AddRange(subResult.RevealedCells);
                    }
                    if (subResult.IsMineDetonated)
                    {
                        result.IsMineDetonated = true;
                        result.DetonatedCell = subResult.DetonatedCell;
                        return result;
                    }
                    if (subResult.IsVictory)
                    {
                        result.IsVictory = true;
                    }
                }
            }

            return result;
        }

        public void RevealAllMinesOnDefeat(int detonatedX, int detonatedY)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var c = _grid[x, y];
                    if (c.X == detonatedX && c.Y == detonatedY)
                    {
                        c.State = CellState.Exploded;
                    }
                    else if (c.IsMine && c.State != CellState.Flagged)
                    {
                        c.State = CellState.Revealed;
                    }
                    else if (!c.IsMine && c.State == CellState.Flagged)
                    {
                        // FR-021: ошибочно выставленные флаги перечеркиваются красным крестиком
                        c.State = CellState.FalseFlag;
                    }
                }
            }
        }

        public void FlagAllMinesOnVictory()
        {
            FlagsCount = TotalMines;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var c = _grid[x, y];
                    if (c.IsMine)
                    {
                        c.State = CellState.Flagged;
                    }
                }
            }
        }

        // For restoring saved state (FR-037)
        public void RestoreState(int width, int height, int mines, int flagsCount, int revealedCount, CellData[,] grid)
        {
            Width = width;
            Height = height;
            TotalMines = mines;
            FlagsCount = flagsCount;
            RevealedSafeCount = revealedCount;
            _grid = grid;
            IsGenerated = true;
        }
    }
}

using System;
using System.Collections.Generic;
using Minesweeper.Core;

namespace Minesweeper.Storage
{
    [Serializable]
    public class SavedCellRecord
    {
        public int X;
        public int Y;
        public bool IsMine;
        public int AdjacentMines;
        public int State;
    }

    [Serializable]
    public class SavedGameState
    {
        public DifficultyLevel Difficulty;
        public int Width;
        public int Height;
        public int TotalMines;
        public int FlagsCount;
        public int RevealedSafeCount;
        public float ElapsedTime;
        public List<SavedCellRecord> Cells = new List<SavedCellRecord>();

        public static SavedGameState Capture(MinesweeperBoard board, DifficultyLevel diff, float elapsedTime)
        {
            var state = new SavedGameState
            {
                Difficulty = diff,
                Width = board.Width,
                Height = board.Height,
                TotalMines = board.TotalMines,
                FlagsCount = board.FlagsCount,
                RevealedSafeCount = board.RevealedSafeCount,
                ElapsedTime = elapsedTime
            };

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var c = board.GetCell(x, y);
                    state.Cells.Add(new SavedCellRecord
                    {
                        X = c.X,
                        Y = c.Y,
                        IsMine = c.IsMine,
                        AdjacentMines = c.AdjacentMines,
                        State = (int)c.State
                    });
                }
            }
            return state;
        }

        public void RestoreTo(MinesweeperBoard board)
        {
            var grid = new CellData[Width, Height];
            foreach (var r in Cells)
            {
                var c = new CellData(r.X, r.Y)
                {
                    IsMine = r.IsMine,
                    AdjacentMines = r.AdjacentMines,
                    State = (CellState)r.State
                };
                grid[r.X, r.Y] = c;
            }
            board.RestoreState(Width, Height, TotalMines, FlagsCount, RevealedSafeCount, grid);
        }
    }
}

using System;

namespace Minesweeper.Core
{
    [Serializable]
    public class CellData
    {
        public int X;
        public int Y;
        public bool IsMine;
        public int AdjacentMines;
        public CellState State = CellState.Closed;

        public CellData(int x, int y)
        {
            X = x;
            Y = y;
            IsMine = false;
            AdjacentMines = 0;
            State = CellState.Closed;
        }

        public bool IsClosed => State == CellState.Closed;
        public bool IsFlagged => State == CellState.Flagged;
        public bool IsRevealed => State == CellState.Revealed;
    }
}

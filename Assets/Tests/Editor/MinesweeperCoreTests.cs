using System;
using System.Collections.Generic;
using NUnit.Framework;
using Minesweeper.Core;
using Minesweeper.Storage;

namespace Minesweeper.Tests
{
    [TestFixture]
    public class MinesweeperCoreTests
    {
        [Test]
        public void TestBoardInitialization()
        {
            var board = new MinesweeperBoard(9, 9, 10);
            Assert.AreEqual(9, board.Width);
            Assert.AreEqual(9, board.Height);
            Assert.AreEqual(10, board.TotalMines);
            Assert.AreEqual(0, board.FlagsCount);
            Assert.AreEqual(0, board.RevealedSafeCount);
            Assert.IsFalse(board.IsGenerated);

            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    var cell = board.GetCell(x, y);
                    Assert.IsNotNull(cell);
                    Assert.AreEqual(CellState.Closed, cell.State);
                    Assert.IsFalse(cell.IsMine);
                }
            }
        }

        [Test]
        public void TestSafeFirstClick_Guarantee3x3SafeZone()
        {
            // FR-004: Safe first click ensures clicked cell and all 8 neighbors are mine-free
            var board = new MinesweeperBoard(16, 16, 40);
            int startX = 5;
            int startY = 5;

            board.GenerateBoard(startX, startY, new Random(12345));
            Assert.IsTrue(board.IsGenerated);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = startX + dx;
                    int ny = startY + dy;
                    var cell = board.GetCell(nx, ny);
                    Assert.IsFalse(cell.IsMine, $"Cell ({nx},{ny}) in 3x3 zone must NOT be a mine!");
                }
            }

            // Verify total mine count placed matches requested
            int mineCount = 0;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    if (board.GetCell(x, y).IsMine) mineCount++;
                }
            }
            Assert.AreEqual(40, mineCount);
        }

        [Test]
        public void TestAdjacentMinesCalculation()
        {
            var board = new MinesweeperBoard(3, 3, 1);
            board.GetCell(0, 0).IsMine = true;
            board.CalculateAdjacentMines();

            Assert.AreEqual(-1, board.GetCell(0, 0).AdjacentMines);
            Assert.AreEqual(1, board.GetCell(1, 0).AdjacentMines);
            Assert.AreEqual(1, board.GetCell(0, 1).AdjacentMines);
            Assert.AreEqual(1, board.GetCell(1, 1).AdjacentMines);
            Assert.AreEqual(0, board.GetCell(2, 2).AdjacentMines);
        }

        [Test]
        public void TestBFSCascadeReveal()
        {
            // 6x6 board with mines at (0, 5) and (5, 5)
            // Clicking at (0, 0) cascades empty 0s and stops at numbers without triggering victory
            var board = new MinesweeperBoard(6, 6, 2);
            board.GetCell(0, 5).IsMine = true;
            board.GetCell(5, 5).IsMine = true;
            board.CalculateAdjacentMines();

            Assert.AreEqual(0, board.GetCell(0, 0).AdjacentMines);
            var result = board.Reveal(0, 0);

            Assert.IsFalse(result.IsMineDetonated);
            Assert.IsTrue(result.RevealedCells.Count > 1, "BFS should reveal connected cluster");

            // Mines must not be detonated
            Assert.AreNotEqual(CellState.Exploded, board.GetCell(0, 5).State);
            Assert.AreNotEqual(CellState.Exploded, board.GetCell(5, 5).State);
            // (0, 0) must be revealed
            Assert.AreEqual(CellState.Revealed, board.GetCell(0, 0).State);
        }

        [Test]
        public void TestFlagToggleAndCounter()
        {
            var board = new MinesweeperBoard(9, 9, 10);
            var cell = board.GetCell(2, 2);

            Assert.AreEqual(CellState.Closed, cell.State);
            Assert.AreEqual(0, board.FlagsCount);

            int delta1 = board.ToggleFlag(2, 2);
            Assert.AreEqual(1, delta1);
            Assert.AreEqual(CellState.Flagged, cell.State);
            Assert.AreEqual(1, board.FlagsCount);

            int delta2 = board.ToggleFlag(2, 2);
            Assert.AreEqual(-1, delta2);
            Assert.AreEqual(CellState.Closed, cell.State);
            Assert.AreEqual(0, board.FlagsCount);
        }

        [Test]
        public void TestFlaggedCellCannotBeRevealed()
        {
            var board = new MinesweeperBoard(9, 9, 10);
            board.ToggleFlag(3, 3);
            Assert.AreEqual(CellState.Flagged, board.GetCell(3, 3).State);

            var result = board.Reveal(3, 3);
            Assert.AreEqual(0, result.RevealedCells.Count);
            Assert.AreEqual(CellState.Flagged, board.GetCell(3, 3).State);
        }

        [Test]
        public void TestVictoryCondition()
        {
            // 2x2 board, 1 mine at (1, 1) -> 3 safe cells
            var board = new MinesweeperBoard(2, 2, 1);
            board.GetCell(1, 1).IsMine = true;
            board.CalculateAdjacentMines();

            board.Reveal(0, 0);
            board.Reveal(0, 1);
            var res = board.Reveal(1, 0);

            Assert.IsTrue(res.IsVictory, "Victory condition should trigger when all safe cells are revealed");
            Assert.AreEqual(CellState.Flagged, board.GetCell(1, 1).State, "Mine should be auto-flagged on victory (FR-020)");
        }

        [Test]
        public void TestDefeatConditionAndFalseFlagCrossout()
        {
            var board = new MinesweeperBoard(3, 3, 1);
            board.GetCell(0, 0).IsMine = true;
            board.CalculateAdjacentMines();

            // Set false flag on (2, 2) which is safe
            board.ToggleFlag(2, 2);

            // Detonate mine at (0, 0)
            var result = board.Reveal(0, 0);

            Assert.IsTrue(result.IsMineDetonated);
            Assert.AreEqual(CellState.Exploded, board.GetCell(0, 0).State);
            Assert.AreEqual(CellState.FalseFlag, board.GetCell(2, 2).State, "Incorrect flag must become FalseFlag (FR-021)");
        }

        [Test]
        public void TestChording()
        {
            // 3x3 board with 1 mine at (0, 0)
            var board = new MinesweeperBoard(3, 3, 1);
            board.GetCell(0, 0).IsMine = true;
            board.CalculateAdjacentMines();

            // Reveal (1, 1) which has adjacentMines = 1
            board.Reveal(1, 1);
            Assert.AreEqual(CellState.Revealed, board.GetCell(1, 1).State);
            Assert.AreEqual(1, board.GetCell(1, 1).AdjacentMines);

            // Flag the mine at (0, 0)
            board.ToggleFlag(0, 0);

            // Chord on (1, 1) -> should open all other 7 neighbors safely
            var chordResult = board.Chord(1, 1);
            Assert.IsFalse(chordResult.IsMineDetonated);
            Assert.IsTrue(chordResult.RevealedCells.Count >= 7);
        }

        [Test]
        public void TestCustomDifficultyValidationClamping()
        {
            int w = 5;  // below min 9
            int h = 30; // above max 24
            int m = 5;  // below min 10
            GameDifficulty.ValidateCustom(ref w, ref h, ref m);

            Assert.AreEqual(9, w);
            Assert.AreEqual(24, h);
            Assert.AreEqual(10, m);

            int w2 = 100;
            int h2 = 100;
            int m2 = 9999;
            GameDifficulty.ValidateCustom(ref w2, ref h2, ref m2);

            Assert.AreEqual(30, w2);
            Assert.AreEqual(24, h2);
            Assert.AreEqual(30 * 24 - 9, m2);
        }

        [Test]
        public void TestHighScoresRankingAndCustomExclusion()
        {
            var scores = new HighScoresData();
            scores.ResetToDefaults();

            // Custom difficulty must not be ranked (FR-025)
            Assert.IsFalse(scores.IsEligibleForHighScore(DifficultyLevel.Custom, 5.0f));
            Assert.AreEqual(-1, scores.AddScore(DifficultyLevel.Custom, new HighScoreEntry("TEST", 5.0f, "date")));

            // Beginner difficulty
            Assert.IsTrue(scores.IsEligibleForHighScore(DifficultyLevel.Beginner, 5.0f));
            int rank = scores.AddScore(DifficultyLevel.Beginner, new HighScoreEntry("NEW_RECORD", 5.0f, "date"));
            Assert.AreEqual(1, rank);
            Assert.AreEqual("NEW_RECORD", scores.BeginnerScores[0].PlayerName);
            Assert.AreEqual(5, scores.BeginnerScores.Count);
        }
    }
}

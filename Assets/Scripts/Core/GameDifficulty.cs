using System;
using UnityEngine;

namespace Minesweeper.Core
{
    [Serializable]
    public class GameDifficulty
    {
        public DifficultyLevel Level = DifficultyLevel.Beginner;
        public int Width = 9;
        public int Height = 9;
        public int Mines = 10;

        public static GameDifficulty Create(DifficultyLevel level, int customWidth = 9, int customHeight = 9, int customMines = 10)
        {
            var diff = new GameDifficulty { Level = level };
            switch (level)
            {
                case DifficultyLevel.Beginner:
                    diff.Width = 9;
                    diff.Height = 9;
                    diff.Mines = 10;
                    break;
                case DifficultyLevel.Intermediate:
                    diff.Width = 16;
                    diff.Height = 16;
                    diff.Mines = 40;
                    break;
                case DifficultyLevel.Expert:
                    diff.Width = 30;
                    diff.Height = 16;
                    diff.Mines = 99;
                    break;
                case DifficultyLevel.Custom:
                    diff.Width = Mathf.Clamp(customWidth, 9, 30);
                    diff.Height = Mathf.Clamp(customHeight, 9, 24);
                    int maxMines = diff.Width * diff.Height - 9;
                    diff.Mines = Mathf.Clamp(customMines, 10, Mathf.Max(10, maxMines));
                    break;
            }
            return diff;
        }

        public static void ValidateCustom(ref int width, ref int height, ref int mines)
        {
            width = Mathf.Clamp(width, 9, 30);
            height = Mathf.Clamp(height, 9, 24);
            int maxMines = width * height - 9;
            mines = Mathf.Clamp(mines, 10, Mathf.Max(10, maxMines));
        }

        public float MineDensity => (Width * Height > 0) ? (float)Mines / (Width * Height) : 0f;
    }
}

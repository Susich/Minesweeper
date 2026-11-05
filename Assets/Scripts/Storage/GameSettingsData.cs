using System;
using Minesweeper.Core;

namespace Minesweeper.Storage
{
    [Serializable]
    public class GameSettingsData
    {
        public float SoundVolume = 0.8f;
        public bool IsMuted = false;
        public bool TimerTickEnabled = false;
        public DifficultyLevel Difficulty = DifficultyLevel.Beginner;
        public int CustomWidth = 16;
        public int CustomHeight = 16;
        public int CustomMines = 40;
        public string LastPlayerName = "CYBER_OPERATOR";
    }
}

using System;
using System.Collections.Generic;
using Minesweeper.Core;

namespace Minesweeper.Storage
{
    [Serializable]
    public class HighScoreEntry
    {
        public string PlayerName;
        public float TimeSeconds;
        public string Date;
        public float Accuracy;
        public float Efficiency;

        public HighScoreEntry() { }

        public HighScoreEntry(string name, float time, string date, float accuracy = 98.5f, float efficiency = 4.2f)
        {
            PlayerName = name;
            TimeSeconds = time;
            Date = date;
            Accuracy = accuracy;
            Efficiency = efficiency;
        }
    }

    [Serializable]
    public class HighScoresData
    {
        public List<HighScoreEntry> BeginnerScores = new List<HighScoreEntry>();
        public List<HighScoreEntry> IntermediateScores = new List<HighScoreEntry>();
        public List<HighScoreEntry> ExpertScores = new List<HighScoreEntry>();

        public static HighScoresData CreateDefault()
        {
            var data = new HighScoresData();
            data.ResetToDefaults();
            return data;
        }

        public void ResetToDefaults()
        {
            string dateNow = DateTime.UtcNow.ToString("yyyy.MM.dd");

            BeginnerScores = new List<HighScoreEntry>
            {
                new HighScoreEntry("CYBER_VIPER", 8.2f, dateNow, 99.1f, 5.10f),
                new HighScoreEntry("NEXUS_9", 11.4f, dateNow, 98.4f, 4.80f),
                new HighScoreEntry("GHOST_RUNNER", 14.0f, dateNow, 96.2f, 4.10f),
                new HighScoreEntry("ZERO_COOL", 18.5f, dateNow, 95.0f, 3.50f),
                new HighScoreEntry("VOID_WALKER", 22.1f, dateNow, 93.7f, 3.10f)
            };

            IntermediateScores = new List<HighScoreEntry>
            {
                new HighScoreEntry("CYBER_VIPER", 28.5f, dateNow, 99.0f, 4.50f),
                new HighScoreEntry("NEXUS_9", 35.2f, dateNow, 97.8f, 4.10f),
                new HighScoreEntry("GHOST_RUNNER", 42.6f, dateNow, 96.5f, 3.80f),
                new HighScoreEntry("ZERO_COOL", 51.0f, dateNow, 94.2f, 3.20f),
                new HighScoreEntry("VOID_WALKER", 63.4f, dateNow, 92.1f, 2.90f)
            };

            ExpertScores = new List<HighScoreEntry>
            {
                new HighScoreEntry("CYBER_VIPER", 85.0f, dateNow, 98.9f, 4.20f),
                new HighScoreEntry("NEXUS_9", 98.6f, dateNow, 97.1f, 3.90f),
                new HighScoreEntry("GHOST_RUNNER", 115.3f, dateNow, 95.5f, 3.40f),
                new HighScoreEntry("ZERO_COOL", 132.8f, dateNow, 93.0f, 3.00f),
                new HighScoreEntry("VOID_WALKER", 154.2f, dateNow, 91.5f, 2.70f)
            };
        }

        public List<HighScoreEntry> GetListForDifficulty(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Beginner: return BeginnerScores;
                case DifficultyLevel.Intermediate: return IntermediateScores;
                case DifficultyLevel.Expert: return ExpertScores;
                default: return null;
            }
        }

        public bool IsEligibleForHighScore(DifficultyLevel level, float time)
        {
            if (level == DifficultyLevel.Custom) return false;
            var list = GetListForDifficulty(level);
            if (list == null) return false;
            if (list.Count < 5) return true;
            return time < list[list.Count - 1].TimeSeconds;
        }

        public int AddScore(DifficultyLevel level, HighScoreEntry entry)
        {
            if (level == DifficultyLevel.Custom) return -1;
            var list = GetListForDifficulty(level);
            if (list == null) return -1;

            list.Add(entry);
            list.Sort((a, b) => a.TimeSeconds.CompareTo(b.TimeSeconds));
            if (list.Count > 5)
            {
                list.RemoveRange(5, list.Count - 5);
            }
            return list.IndexOf(entry) + 1; // 1-based rank
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Minesweeper.Storage;
using Minesweeper.Audio;

namespace Minesweeper.Core
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }

        public event Action<GameStatus> OnGameStatusChanged;
        public event Action<int> OnMineCounterChanged;
        public event Action<int> OnTimerChanged;
        public event Action<CellData> OnCellUpdated;
        public event Action<List<CellData>> OnMultipleCellsUpdated;
        public event Action<CellData> OnDetonated;
        public event Action<GameDifficulty> OnGameInitialized;
        public event Action OnBoardReset;

        public GameStatus Status { get; private set; } = GameStatus.WaitingForStart;
        public MinesweeperBoard Board { get; private set; }
        public GameDifficulty CurrentDifficulty { get; private set; }
        public float ElapsedTime { get; private set; } = 0f;
        public int ElapsedSeconds => Mathf.Clamp((int)ElapsedTime, 0, 999);
        public int RemainingMines => Board != null ? Board.TotalMines - Board.FlagsCount : 0;

        // Statistics for current round
        public int TotalClicks { get; private set; } = 0;
        public int TotalSafeClicks { get; private set; } = 0;
        public CellData LastDetonatedCell { get; private set; }

        private SaveManager _saveManager;
        private GameSettingsData _settings;
        private HighScoresData _highScores;
        private int _lastTickedSecond = -1;
        private bool _isMouseDownOnBoard = false;

        public bool IsMouseDownOnBoard => _isMouseDownOnBoard;
        public SaveManager SaveMgr => _saveManager;
        public GameSettingsData Settings => _settings;
        public HighScoresData HighScores => _highScores;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _saveManager = new SaveManager();
            _settings = _saveManager.LoadSettings();
            _highScores = _saveManager.LoadHighScores();
        }

        private void Start()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.InitializeSettings(_settings.SoundVolume, _settings.IsMuted, _settings.TimerTickEnabled);
            }

            // Check for saved unfinished session (FR-037)
            var savedSession = _saveManager.LoadSession();
            if (savedSession != null && savedSession.Cells != null && savedSession.Cells.Count > 0)
            {
                RestoreSavedSession(savedSession);
            }
            else
            {
                StartNewGame(_settings.Difficulty, _settings.CustomWidth, _settings.CustomHeight, _settings.CustomMines);
            }
        }

        private void Update()
        {
            if (Status == GameStatus.InProgress)
            {
                ElapsedTime += Time.deltaTime;
                if (ElapsedTime > 999f) ElapsedTime = 999f;

                int currentSec = ElapsedSeconds;
                if (currentSec != _lastTickedSecond)
                {
                    _lastTickedSecond = currentSec;
                    OnTimerChanged?.Invoke(currentSec);

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayTimerTick();
                    }
                }
            }

            // F2 quick restart hotkey (FR-026, GDD 2.1.2)
            if (Input.GetKeyDown(KeyCode.F2))
            {
                RestartGame();
            }
        }

        public void StartNewGame(DifficultyLevel level, int customW = 9, int customH = 9, int customMines = 10)
        {
            CurrentDifficulty = GameDifficulty.Create(level, customW, customH, customMines);
            _settings.Difficulty = level;
            if (level == DifficultyLevel.Custom)
            {
                _settings.CustomWidth = CurrentDifficulty.Width;
                _settings.CustomHeight = CurrentDifficulty.Height;
                _settings.CustomMines = CurrentDifficulty.Mines;
            }
            _saveManager.SaveSettings(_settings);

            ResetGame();
        }

        public void RestartGame()
        {
            ResetGame();
        }

        private void ResetGame()
        {
            _saveManager.ClearSession();

            Board = new MinesweeperBoard(CurrentDifficulty.Width, CurrentDifficulty.Height, CurrentDifficulty.Mines);
            ElapsedTime = 0f;
            _lastTickedSecond = 0;
            TotalClicks = 0;
            TotalSafeClicks = 0;
            LastDetonatedCell = null;
            _isMouseDownOnBoard = false;

            Status = GameStatus.WaitingForStart;

            OnBoardReset?.Invoke();
            OnGameInitialized?.Invoke(CurrentDifficulty);
            OnMineCounterChanged?.Invoke(RemainingMines);
            OnTimerChanged?.Invoke(0);
            OnGameStatusChanged?.Invoke(Status);
        }

        public void SetMouseDownOnBoard(bool isDown)
        {
            if (Status == GameStatus.WaitingForStart || Status == GameStatus.InProgress)
            {
                _isMouseDownOnBoard = isDown;
                OnGameStatusChanged?.Invoke(Status);
            }
        }

        public void OnCellLeftClick(int x, int y)
        {
            if (Status == GameStatus.Paused)
            {
                ResumeGame();
            }

            if (Status != GameStatus.WaitingForStart && Status != GameStatus.InProgress)
                return;

            TotalClicks++;

            // First click: safe first click generation (FR-003, FR-004)
            if (Status == GameStatus.WaitingForStart)
            {
                Board.GenerateBoard(x, y);
                Status = GameStatus.InProgress;
                _lastTickedSecond = 0;
                OnGameStatusChanged?.Invoke(Status);
            }

            var result = Board.Reveal(x, y);
            ProcessRevealResult(result);
        }

        public void OnCellRightClick(int x, int y)
        {
            if (Status == GameStatus.Paused)
            {
                ResumeGame();
            }

            if (Status != GameStatus.WaitingForStart && Status != GameStatus.InProgress)
                return;

            TotalClicks++;
            int delta = Board.ToggleFlag(x, y);
            if (delta != 0)
            {
                var cell = Board.GetCell(x, y);
                OnCellUpdated?.Invoke(cell);
                OnMineCounterChanged?.Invoke(RemainingMines);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play(delta > 0 ? SoundType.Flag : SoundType.Unflag);
                }
            }
        }

        public void OnCellChordClick(int x, int y)
        {
            if (Status != GameStatus.InProgress) return;

            TotalClicks++;
            var result = Board.Chord(x, y);
            ProcessRevealResult(result);
        }

        private void ProcessRevealResult(RevealResult result)
        {
            if (result.RevealedCells != null && result.RevealedCells.Count > 0)
            {
                if (result.IsMineDetonated)
                {
                    LastDetonatedCell = result.DetonatedCell;
                    Status = GameStatus.Defeat;
                    _isMouseDownOnBoard = false;
                    _saveManager.ClearSession();

                    OnDetonated?.Invoke(LastDetonatedCell);
                    OnMultipleCellsUpdated?.Invoke(result.RevealedCells);
                    OnGameStatusChanged?.Invoke(Status);

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.Play(SoundType.Explosion);
                    }
                }
                else
                {
                    TotalSafeClicks += result.RevealedCells.Count;
                    OnMultipleCellsUpdated?.Invoke(result.RevealedCells);

                    if (result.IsVictory)
                    {
                        Status = GameStatus.Victory;
                        _isMouseDownOnBoard = false;
                        _saveManager.ClearSession();
                        OnMineCounterChanged?.Invoke(RemainingMines);
                        OnGameStatusChanged?.Invoke(Status);

                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.Play(SoundType.Victory);
                        }
                    }
                    else
                    {
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.Play(result.RevealedCells.Count > 1 ? SoundType.Cascade : SoundType.Click);
                        }
                    }
                }
            }
        }

        public void PauseGame()
        {
            if (Status == GameStatus.InProgress)
            {
                Status = GameStatus.Paused;
                OnGameStatusChanged?.Invoke(Status);
                SaveCurrentSession();
            }
        }

        public void ResumeGame()
        {
            if (Status == GameStatus.Paused)
            {
                Status = GameStatus.InProgress;
                OnGameStatusChanged?.Invoke(Status);
            }
        }

        public void TogglePause()
        {
            if (Status == GameStatus.InProgress)
                PauseGame();
            else if (Status == GameStatus.Paused)
                ResumeGame();
        }

        private void SaveCurrentSession()
        {
            if (Status == GameStatus.InProgress || Status == GameStatus.Paused)
            {
                var session = SavedGameState.Capture(Board, CurrentDifficulty.Level, ElapsedTime);
                _saveManager.SaveSession(session);
            }
            else
            {
                _saveManager.ClearSession();
            }
        }

        private void RestoreSavedSession(SavedGameState session)
        {
            CurrentDifficulty = GameDifficulty.Create(session.Difficulty, session.Width, session.Height, session.TotalMines);
            Board = new MinesweeperBoard(session.Width, session.Height, session.TotalMines);
            session.RestoreTo(Board);

            ElapsedTime = session.ElapsedTime;
            _lastTickedSecond = ElapsedSeconds;

            // Restored game starts in Paused state until player interaction (FR-037)
            Status = GameStatus.Paused;

            OnBoardReset?.Invoke();
            OnGameInitialized?.Invoke(CurrentDifficulty);
            OnMineCounterChanged?.Invoke(RemainingMines);
            OnTimerChanged?.Invoke(ElapsedSeconds);
            OnGameStatusChanged?.Invoke(Status);

            // Notify UI of restored cells
            var allCells = new List<CellData>();
            for (int x = 0; x < Board.Width; x++)
            {
                for (int y = 0; y < Board.Height; y++)
                {
                    allCells.Add(Board.GetCell(x, y));
                }
            }
            OnMultipleCellsUpdated?.Invoke(allCells);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                PauseGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveCurrentSession();
        }

        public float CalculateAccuracy()
        {
            if (TotalClicks == 0) return 100f;
            float acc = ((float)TotalSafeClicks / Mathf.Max(1, TotalClicks)) * 100f;
            return Mathf.Clamp(acc, 0f, 100f);
        }

        public float CalculateEfficiency()
        {
            if (ElapsedTime <= 0.05f) return 0f;
            return (float)Math.Round(TotalClicks / ElapsedTime, 2);
        }

        public bool CheckHighScoreEligibility()
        {
            return _highScores.IsEligibleForHighScore(CurrentDifficulty.Level, ElapsedTime);
        }

        public int RegisterHighScore(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                playerName = "CYBER_OPERATOR";

            _settings.LastPlayerName = playerName;
            _saveManager.SaveSettings(_settings);

            var entry = new HighScoreEntry(
                playerName,
                (float)Math.Round(ElapsedTime, 2),
                DateTime.UtcNow.ToString("yyyy.MM.dd"),
                (float)Math.Round(CalculateAccuracy(), 1),
                CalculateEfficiency()
            );

            int rank = _highScores.AddScore(CurrentDifficulty.Level, entry);
            _saveManager.SaveHighScores(_highScores);
            return rank;
        }
    }
}

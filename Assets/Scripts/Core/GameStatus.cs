namespace Minesweeper.Core
{
    public enum GameStatus
    {
        WaitingForStart = 0, // Ожидание старта (BM-001)
        InProgress = 1,      // В процессе
        Paused = 2,          // Пауза (окно настроек/справки или сворачивание)
        Victory = 3,         // Победа
        Defeat = 4           // Поражение
    }
}

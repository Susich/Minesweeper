namespace Minesweeper.Core
{
    public enum CellState
    {
        Closed = 0,     // Закрытая ячейка
        Flagged = 1,    // Помечена флагом
        Revealed = 2,   // Открыта (безопасная цифра или пустая)
        Exploded = 3,   // Подорванная мина (красный фон / взрыв)
        FalseFlag = 4   // Ложный флаг (перечеркнут крестом при поражении)
    }
}

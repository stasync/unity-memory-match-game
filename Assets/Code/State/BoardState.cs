using System;

namespace Code.State
{
    /// <summary>
    /// A wrapper around the <see cref="BoardCellState"/> array which is a state of the game (board).
    /// Also used to save/load the game state.
    /// Could be used to mock the data (for unit test).
    /// </summary>
    public sealed class BoardState
    {
        public readonly int Rows;
        public readonly int Columns;

        private readonly BoardCellState[,] _internalState;

        public BoardState(BoardCellState[,] state)
        {
            _internalState = state ?? throw new ArgumentNullException(nameof(state));

            Rows = _internalState.GetLength(dimension: 0);
            Columns = _internalState.GetLength(dimension: 1);
        }

        public ref BoardCellState GetStateRef(int row, int column)
        {
            if (row < 0 || row >= Rows || column < 0 || column >= Columns)
                throw new IndexOutOfRangeException(
                    $"Invalid cell address r:{row} c:{column}. Coordinates must be in the following range: [0, {Rows - 1}] for rows and [0, {Columns - 1}] for columns.");

            return ref _internalState[row, column];
        }
    }
}
public class BoardIterator : IIterator<Cell>
{
    private readonly Cell[][] _grid;
    private int _row = 0;
    private int _column = 0;

    public BoardIterator(Cell[][] grid)
    {
        _grid = grid;
    }

    public bool HasNext()
    {
        return _row < _grid.Length && _column < _grid[_row].Length;
    }

    public Cell Next()
    {
        if (!HasNext())
            throw new InvalidOperationException("No more elements in the grid.");

        var cell = _grid[_row][_column];
        _column++;

        if (_column >= _grid[_row].Length)
        {
            _column = 0;
            _row++;
        }

        return cell;
    }
}
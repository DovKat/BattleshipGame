public class Board
{
    public Cell[][] Grid { get; set; }
    public List<Ship> Ships { get; set; } = new List<Ship>();

    // Method to return a new iterator for the board
    public IIterator<Cell> GetIterator()
    {
        return new BoardIterator(Grid);
    }
}


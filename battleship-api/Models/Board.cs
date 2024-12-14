public class Board : IEnumerable<Cell>
{
    public Cell[][] Grid { get; set; }
    public List<Ship> Ships { get; set; } = new List<Ship>();

    public IEnumerator<Cell> GetEnumerator()
    {
        for (int row = 0; row < Grid.Length; row++)
        {
            for (int col = 0; col < Grid[row].Length; col++)
            {
                yield return Grid[row][col];
            }
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}


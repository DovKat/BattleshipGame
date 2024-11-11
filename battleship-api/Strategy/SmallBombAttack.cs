namespace battleship_api.Strategy;

public class SmallBombAttack : IAttackStrategy
{
    public List<Coordinate> GetAffectedCoordinates(Coordinate start)
    {
        return GenerateSquare(start, 1); // 5x5 area
    }
    public List<Coordinate> GenerateSquare(Coordinate center, int radius)
    {
        var affectedCoordinates = new List<Coordinate>();

        for (int row = center.Row - radius; row <= center.Row + radius; row++)
        {
            for (int col = center.Column - radius; col <= center.Column + radius; col++)
            {
                affectedCoordinates.Add(new Coordinate { Row = row, Column = col });
            }
        }

        return affectedCoordinates;
    }
}

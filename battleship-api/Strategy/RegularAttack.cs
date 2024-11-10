namespace battleship_api.Strategy;

public class RegularAttack : IAttackStrategy
{
    public List<Coordinate> GetAffectedCoordinates(Coordinate start)
    {
        return new List<Coordinate> { start };
    }
}
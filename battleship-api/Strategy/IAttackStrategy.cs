namespace battleship_api.Strategy;

public interface IAttackStrategy
{
    List<Coordinate> GetAffectedCoordinates(Coordinate start);
}
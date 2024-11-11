namespace battleship_api.Strategy.Decorator;
public class LoggingDecorator : AttackDecorator
{
    public LoggingDecorator(IAttackStrategy wrappedAttack) : base(wrappedAttack) { }

    public override List<Coordinate> GetAffectedCoordinates(Coordinate startCoordinate)
    {
        var coordinates = base.GetAffectedCoordinates(startCoordinate);
        Console.WriteLine($"Attack performed at ({startCoordinate.Row}, {startCoordinate.Column}).");
        return coordinates;
    }
}
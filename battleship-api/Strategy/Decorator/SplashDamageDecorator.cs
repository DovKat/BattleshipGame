namespace battleship_api.Strategy.Decorator;
public class SplashDamageDecorator : AttackDecorator
{
    public SplashDamageDecorator(IAttackStrategy wrappedAttack) : base(wrappedAttack) { }

    public override List<Coordinate> GetAffectedCoordinates(Coordinate startCoordinate)
    {
        var baseCoordinates = base.GetAffectedCoordinates(startCoordinate);
        var additionalSplash = new List<Coordinate>();

        foreach (var coord in baseCoordinates)
        {
            additionalSplash.Add(new Coordinate { Row = coord.Row + 1, Column = coord.Column });
            additionalSplash.Add(new Coordinate { Row = coord.Row - 1, Column = coord.Column });
            additionalSplash.Add(new Coordinate { Row = coord.Row, Column = coord.Column + 1 });
            additionalSplash.Add(new Coordinate { Row = coord.Row, Column = coord.Column - 1 });
        }
        
        baseCoordinates.AddRange(additionalSplash);
        Console.WriteLine("Applying splash damage effect.");
        return baseCoordinates;
    }
}
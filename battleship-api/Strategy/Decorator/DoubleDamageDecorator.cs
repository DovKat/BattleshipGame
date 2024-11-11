namespace battleship_api.Strategy.Decorator;
public class DoubleDamageDecorator : AttackDecorator
{
    public DoubleDamageDecorator(IAttackStrategy wrappedAttack) : base(wrappedAttack) { }

    public override List<Coordinate> GetAffectedCoordinates(Coordinate startCoordinate)
    {
        var baseCoordinates = base.GetAffectedCoordinates(startCoordinate);
        Console.WriteLine("Applying double damage to affected coordinates.");
        return baseCoordinates;
    }
}

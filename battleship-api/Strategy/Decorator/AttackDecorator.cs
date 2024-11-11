namespace battleship_api.Strategy.Decorator;

public abstract class AttackDecorator : IAttackStrategy
{
    protected readonly IAttackStrategy _wrappedAttack;

    public AttackDecorator(IAttackStrategy wrappedAttack)
    {
        _wrappedAttack = wrappedAttack;
    }

    public virtual List<Coordinate> GetAffectedCoordinates(Coordinate startCoordinate)
    {
        return _wrappedAttack.GetAffectedCoordinates(startCoordinate);
    }
}

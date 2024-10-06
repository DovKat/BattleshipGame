using backend.Models.Entity.Bombs;
using backend.Models.Entity.Bombs.SmallBomb;
using backend.Strategies;
using backend.Strategies.Attacks;
using backend.Strategies.Ships;
using backend.Visitor;
using Shared;

namespace backend.Models.Entity.Ships;

public class SmallShip : Ship
{
    public SmallShip()
    {
        Stats = new Statistics(1000);
    }

    public SmallShip(SmallShip ship) {
        Size = ship.Size;
        IsVertical = ship.IsVertical;
        ShipType = ship.ShipType;
        Stats = ship.Stats;
        PlacedX = ship.PlacedX;
        PlacedY = ship.PlacedY;
        Price = ship.Price;
        ShootsLeft = ship.ShootsLeft;

        foreach (ShipCoordinate coordinate in ship.GetCoordinates())
        {
            AddCoordinate(coordinate);
        }
    }

    public override Ship DeepCopy()
    {
        SmallShip ship = new SmallShip(this);
        List<ShipCoordinate> coords = GetCoordinates();

        ship.RemoveAllCoordinates();

        foreach (ShipCoordinate coordinate in coords)
        {
            ship.AddCoordinate(new ShipCoordinate(coordinate.X, coordinate.Y));
        }

        return ship;
    }

    public override AttackTemplate GetAttackTemplate()
    {
        return new SmallShipAttack();
    }

    public override Ship ShallowCopy()
    {
        return new SmallShip(this);
    }

    public override int Accept(ShipInspector shipInspector)
    {
        return shipInspector.visit(this);
    }
}
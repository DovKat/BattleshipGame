using backend.Strategies.Ships;
using backend.Strategies;
using Shared;
using backend.Models.Entity.Bombs;
using backend.Models.Entity.Bombs.SmallBomb;
using backend.Models.Entity.Bombs.MediumBomb;
using backend.Strategies.Attacks;
using backend.Visitor;

namespace backend.Models.Entity.Ships
{
    public class MediumShip : Ship
    {
        public MediumShip()
        {
            Stats = new Statistics(5000);

        }

        public MediumShip(MediumShip ship)
        {
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
            MediumShip ship = new MediumShip(this);
            List<ShipCoordinate> coords = GetCoordinates();
            ship.Stats = new Statistics(Stats.HealthCount);

            ship.RemoveAllCoordinates();
            foreach (ShipCoordinate coordinate in coords)
            {
                ship.AddCoordinate(new ShipCoordinate(coordinate.X, coordinate.Y));
            }
            return ship;
        }

        public override AttackTemplate GetAttackTemplate()
        {
            return new MediumShipAttack();
        }

        public override Ship ShallowCopy()
        {
            return new MediumShip(this);
        }

        public override int Accept(ShipInspector shipInspector)
        {
            return shipInspector.visit(this);
        }
    }
}

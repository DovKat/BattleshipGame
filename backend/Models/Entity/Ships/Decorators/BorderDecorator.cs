using backend.Models.Entity.Bombs;
using backend.Strategies;
using backend.Strategies.Attacks;
using backend.Visitor;
using Shared;

namespace backend.Models.Entity.Ships.Decorators
{
    public class BorderDecorator : Ship
    {
        private Ship _ship;
        private Color _color;

        public BorderDecorator(BorderDecorator decorator)
        {
            _ship = decorator._ship;
            _color = decorator._color;
            ShipType = decorator.ShipType;
            IsVertical = decorator._ship.IsVertical;
            Size = decorator._ship.Size;
        }

        public BorderDecorator(Ship ship, Color color)
        {
            _ship = ship;
            _color = color;
            ShipType = ship.ShipType;
            IsVertical = ship.IsVertical;
            Size = ship.Size;
        }

        public override AttackTemplate GetAttackTemplate()
        {
            return _ship.GetAttackTemplate();
        }

        public override List<ShipCoordinate> GetCoordinates()
        {
            List<ShipCoordinate> coords = _ship.GetCoordinates();
            foreach (ShipCoordinate coord in coords)
            {
                if (coord.IsHit)
                {
                    continue;
                }

                coord.BorderColor = _color;
            }

            return coords;
        }

        public override Ship DeepCopy()
        {
            return new BorderDecorator(this);
        }

        public override Ship ShallowCopy()
        {
            return new BorderDecorator(this);
        }

        public override int Accept(ShipInspector shipInspector)
        {
            return 0;
        }
    }
}

using Shared;

namespace backend.Models.Entity.Ships.Factory
{
    public abstract class ShipFactory
    {
        public abstract Ship GetShip(ShipType Ship);
    }
}

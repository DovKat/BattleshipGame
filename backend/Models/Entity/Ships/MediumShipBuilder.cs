using Shared;

namespace backend.Models.Entity.Ships
{
    public class MediumShipBuilder : IShipBuilder
    {
        private MediumShip mediumShip = new();

        public Ship Build()
        {
            return mediumShip;
        }

        public IShipBuilder SetSize(int size)
        {
            mediumShip.Size = size;
            return this;
        }

        public IShipBuilder SetIsVertical(bool isVerical)
        {
            mediumShip.IsVertical = isVerical;
            return this;
        }

        public IShipBuilder SetType(ShipType type)
        {
            mediumShip.ShipType = type;
            return this;
        }

        public IShipBuilder SetPrice(int price)
        {
            mediumShip.Price = price;
            return this;
        }

        public IShipBuilder SetShootsLeft(int shootsLeft)
        {
            mediumShip.ShootsLeft = shootsLeft;
            return this;
        }
    }
}

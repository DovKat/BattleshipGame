using Shared;

namespace backend.Models.Entity.Ships
{
    public class BigShipBuilder : IShipBuilder
    {
        private BigShip bigShip = new();

        public Ship Build()
        {
            return bigShip;
        }

        public IShipBuilder SetSize(int size)
        {
            bigShip.Size = size;
            return this;
        }

        public IShipBuilder SetIsVertical(bool isVerical)
        {
            bigShip.IsVertical = isVerical;
            return this;
        }

        public IShipBuilder SetType(ShipType type)
        {
            bigShip.ShipType = type;
            return this;
        }

        public IShipBuilder SetPrice(int price)
        {
            bigShip.Price = price;
            return this;
        }

        public IShipBuilder SetShootsLeft(int shootsLeft)
        {
            bigShip.ShootsLeft = shootsLeft;
            return this;
        }
    }
}

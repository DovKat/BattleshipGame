namespace backend.Models.Entity.Ships
{
    public class Director
    {
        private IShipBuilder _builder = null;

        public Director(IShipBuilder builder)
        {
            ChangeBuilder(builder);
        }

        public void ChangeBuilder(IShipBuilder builder)
        {
            _builder = builder;
        }

        public Ship MakeSmallShip()
        {
            return _builder
                .SetSize(1)
                .SetIsVertical(false)
                .SetType(Shared.ShipType.SmallShip)
                .SetPrice(1)
                .SetShootsLeft(4)
                .Build();
        }

        public Ship MakeMediumShip()
        {
            return _builder
                .SetSize(2)
                .SetIsVertical(false)
                .SetType(Shared.ShipType.MediumShip)
                .SetPrice(2)
                .SetShootsLeft(3)
                .Build();
        }

        public Ship MakeBigShip()
        {
            return _builder
                .SetSize(3)
                .SetIsVertical(false)
                .SetType(Shared.ShipType.BigShip)
                .SetPrice(3)
                .SetShootsLeft(2)
                .Build();
        }
    }
}

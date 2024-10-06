using backend.Models.Entity.Ships;

namespace Shared.Transfer
{
    public class SetupShipResponse
    {
        public bool CanPlace { get; set; }
        public List<ShipCoordinate> ShipCoordinates { get; private set; }
        public ShipType TypeOfShip { get; set; }

        public int ID { get; set; }

        public SetupShipResponse(bool canPlace, List<ShipCoordinate> shipCoordinates, ShipType typeOfShip, int id)
        {
            CanPlace = canPlace;
            TypeOfShip = typeOfShip;
            ID = id;
            ShipCoordinates = new List<ShipCoordinate>();
            foreach(ShipCoordinate coord in shipCoordinates)
            {
                ShipCoordinate shipCoordinateClone = new ShipCoordinate(coord.X, coord.Y);
                shipCoordinateClone.Icon = coord.Icon;
                if (coord.IsHit)
                {
                    shipCoordinateClone.Hit();
                }
                
                ShipCoordinates.Add(shipCoordinateClone);
            }
        }
    }
}

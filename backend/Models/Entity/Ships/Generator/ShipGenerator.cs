using backend.Models.Entity.Ships.Factory;
using Shared;

namespace backend.Models.Entity.Ships.Generator
{
    public class ShipGenerator
    {
        private Random random;
        private ShipFactory _shipFactory;
        public ShipGenerator()
        {
            _shipFactory = new ConcreteShipFactory();
            random = new Random();
        }

        public List<Ship> GenerateRandomShips()
        {
            List<Ship> ships = new List<Ship>();
            int boardSize = 10;

            // Define the ship types and their sizes
            Dictionary<ShipType, int> shipSizes = new Dictionary<ShipType, int>
            {
                { ShipType.SmallShip, 1 },
                { ShipType.MediumShip, 2 },
                { ShipType.BigShip, 3 }
            };

            // Iterate through each ship type
            foreach (var shipType in Enum.GetValues(typeof(ShipType)))
            {
                for (int i = 0; i < 3; i++) // Create a maximum of 3 ships of each type
                {
                    ShipType type = (ShipType)shipType;
                    int size = shipSizes[type];
                    bool isVertical = random.Next(2) == 0; // Randomly choose orientation

                    // Randomly select a starting coordinate
                    int startX = random.Next(boardSize);
                    int startY = random.Next(boardSize);

                    // Check if the ship can be placed without overlapping
                    if (CanPlaceShip(ships, startX, startY, size, isVertical))
                    {
                        Ship newShip = CreateShip(type, size, isVertical, startX, startY);
                        newShip.ID = ships.Count;
                        ships.Add(newShip);
                    }
                }
            }

            return ships;
        }

        private Ship CreateShip(ShipType type, int size, bool isVertical, int startX, int startY)
        {
            Ship ship = _shipFactory.GetShip(type);

            if (isVertical && type != ShipType.SmallShip)
            {
                ship.SetVertical();
            }

            for (int i = 0; i < size; i++)
            {
                if (isVertical)
                {
                    ship.AddCoordinate(startX, startY + i);
                }
                else
                {
                    ship.AddCoordinate(startX + i, startY);
                }
            }

            return ship;
        }

        private bool CanPlaceShip(List<Ship> ships, int startX, int startY, int size, bool isVertical)
        {
            // Check if the ship's coordinates overlap with existing ships
            foreach (var ship in ships)
            {
                foreach (var coordinate in ship.GetCoordinates())
                {
                    for (int i = 0; i < size; i++)
                    {
                        int x = isVertical ? startX : startX + i;
                        int y = isVertical ? startY + i : startY;

                        if (coordinate.X == x && coordinate.Y == y)
                        {
                            return false;
                        }
                    }
                }
            }

            // Ensure the ship stays within the 10x10 board
            if (startX + (isVertical ? 0 : size - 1) >= 10 || startY + (isVertical ? size - 1 : 0) >= 10)
            {
                return false;
            }

            return true;
        }
    }
}

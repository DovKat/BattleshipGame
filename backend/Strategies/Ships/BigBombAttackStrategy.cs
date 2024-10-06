using backend.Models.Entity.Bombs;
using backend.Models.Entity.Bombs.SmallBomb;
using backend.Models.Entity.Ships;
using System.Drawing;

namespace backend.Strategies.Ships
{
    public class BigBombAttackStrategy : IAttackStrategy
    {
        public List<ShipCoordinate> TargetShip(int x, int y, MissileBomb bigMissileBomb, int gameBoardSizeX, int gameBoardSizeY)
        {
            List<ShipCoordinate> coordinates = new List<ShipCoordinate>();

            for (int i = 0; i < bigMissileBomb.Size; i++)
            {
                int newX = x - 1 + i;
                if (newX < gameBoardSizeX && newX >= 0)
                {
                    coordinates.Add(new ShipCoordinate(newX, y));
                }
            }

            return coordinates;
        }

        public List<ShipCoordinate> TargetShip(int x, int y, AtomicBomb bigAtomicBomb, int gameBoardSizeX, int gameBoardSizeY)
        {
            List<ShipCoordinate> coordinates = new List<ShipCoordinate>();

            for (int i = 0; i < bigAtomicBomb.Horizontal; i++)
            {
                for (int j = 0; j < bigAtomicBomb.Vertical; j++)
                {
                    int newX = x - 1 + i;
                    int newY = y - 1 + j;
                    if (newX < gameBoardSizeX && newY < gameBoardSizeY && newX >= 0 && newY >= 0)
                    {
                        coordinates.Add(new ShipCoordinate(newX, newY));
                    }
                }
            }

            return coordinates;
        }
    }
}

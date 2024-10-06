using backend.Models.Entity.Bombs;
using backend.Models.Entity.Ships;

namespace backend.Strategies.Ships
{
    public class DefaultAttackStrategy : IAttackStrategy
    {
        public List<ShipCoordinate> TargetShip(int x, int y, MissileBomb missileBomb, int gameBoardSizeX, int gameBoardSizeY)
        {
            List<ShipCoordinate> coordinates = new List<ShipCoordinate>();

            coordinates.Add(new ShipCoordinate(x, y)); // 1x1

            return coordinates;
        }

        public List<ShipCoordinate> TargetShip(int x, int y, AtomicBomb atomicBomb, int gameBoardSizeX, int gameBoardSizeY)
        {
            List<ShipCoordinate> coordinates = new List<ShipCoordinate>();

            coordinates.Add(new ShipCoordinate(x, y)); // 1x1

            return coordinates;
        }
    }
}

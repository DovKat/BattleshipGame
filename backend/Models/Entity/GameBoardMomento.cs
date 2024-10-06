using backend.Models.Entity.GameBoardExtensions;
using backend.Models.Entity.Iterator;
using backend.Models.Entity.Ships;

namespace backend.Models.Entity
{

    public class GameBoardMomento
    {
        public ShipCollection Battleships { get; }
        public HashSet<ShipCoordinate> MissedCoordinates { get; }
        public Ship? EnemyAttackShip { get; }
        public ThemeAbstraction Theme { get; }

        public GameBoardMomento(ShipCollection battleships, HashSet<ShipCoordinate> missedCoordinates, Ship? enemyAttackShip, ThemeAbstraction theme)
        {
            Battleships = battleships ?? throw new ArgumentNullException(nameof(battleships));
            MissedCoordinates = missedCoordinates ?? throw new ArgumentNullException(nameof(missedCoordinates));
            EnemyAttackShip = enemyAttackShip;
            Theme = theme ?? throw new ArgumentNullException(nameof(theme));
        }
    }
}

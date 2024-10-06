using backend.Models.Entity;
using backend.Models.Entity.Bombs;
using backend.Models.Entity.Bombs.BigBomb;
using backend.Models.Entity.Bombs.SmallBomb;
using backend.Models.Entity.Ships;
using backend.Strategies.Attacks.Damage;
using backend.Strategies.Ships;
using Shared;

namespace backend.Strategies.Attacks
{
    public sealed class BigShipAttack : AttackTemplate
    {

        public override void AttackTarget()
        {

            RemoveHealth();
            //RemoveArmour();
        }
        public override List<ShipCoordinate> GetHitCoordinates(int x, int y, List<ShipCoordinate> hitableCordinates)
        {
            List<ShipCoordinate> hitableCoordinates = hitableCordinates;
            List<ShipCoordinate> hitShipCoordinates = new List<ShipCoordinate>();


            foreach (var ship in gameBoard.GetShips())
            {
                foreach (var hitableCoord in hitableCoordinates)
                {
                    if (ship.CanHitCoordinate(hitableCoord.X, hitableCoord.Y))
                    {
                        ship.HitCoordinate(hitableCoord.X, hitableCoord.Y);
                        hitShipCoordinates.Add(hitableCoord);
                        targetShip = ship;
                        AttackTarget();
                    }
                    else
                    {
                        gameBoard.AddMissed(hitableCoord);
                    }
                }
            }

            foreach (var miss in gameBoard.GetMissedCoordinates())
            {
                if (hitShipCoordinates.Contains(miss))
                {
                    gameBoard.RemoveMissed(miss);
                }
            }


            return hitShipCoordinates;
        }

        public override void SetupWeapons()
        {
            bombFactory = new BigBombFactory();
            attackStrategy = new BigBombAttackStrategy();

            var h1 = new AttackDamageHandler();
            var h2 = new FleetDamageHandler();
            var h3 = new ShipDamageAttackHandler();
            var h4 = new ThemeDamageHandler();

            h1.SetNext(h2);
            h2.SetNext(h3);
            h3.SetNext(h4);

            HealthDamage = h1.GetDamage(gameBoard, HealthDamage);
        }

    }
}

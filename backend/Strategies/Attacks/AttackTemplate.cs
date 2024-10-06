using backend.Models.Entity;
using backend.Models.Entity.Bombs;
using backend.Models.Entity.Ships;
using backend.Strategies.Attacks.Damage;
using Shared;

namespace backend.Strategies.Attacks
{
    public abstract class AttackTemplate
    {
        protected BombType bombType;
        protected Ship? attackShip = null;
        protected Ship? targetShip = null;
        protected BombFactory? bombFactory = null;
        protected IAttackStrategy? attackStrategy = null;
        protected int HealthDamage = 0;
        protected int ArmourDamage = 0;
        protected GameBoard? gameBoard = null;
        protected BaseDamageHandler? damageHandler = null;

        public List<ShipCoordinate> Attack( BombType attackBomb, int x, int y, GameBoard gameBoard)
        {
            this.bombType = attackBomb;
            this.gameBoard = gameBoard;
            this.attackShip = gameBoard.GetEnemyAttackShip();
            SetupWeapons();

            if (attackBomb == BombType.MissileBomb)
            {
                MissileBomb missileBomb = bombFactory.CreateMissileBomb();
                var hitableCoordinates = attackStrategy.TargetShip(x, y, missileBomb, gameBoard.maxSizeX, gameBoard.maxSizeY);
                return GetHitCoordinates(x, y, hitableCoordinates);

            }

            if (attackBomb == BombType.AtomicBomb)
            {
                AtomicBomb atomicBomb = bombFactory.CreateAtomicBomb();
                var hitableCoordinates = attackStrategy.TargetShip(x, y, atomicBomb, gameBoard.maxSizeX, gameBoard.maxSizeY);
                return GetHitCoordinates(x, y, hitableCoordinates);
            }

            throw new Exception("Bomb unknown in attack template");
        }

        public virtual List<ShipCoordinate> GetHitCoordinates(int x, int y, List<ShipCoordinate> hitableCordinates)
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

        public abstract void SetupWeapons();
        public abstract void AttackTarget();
        //public virtual void RemoveArmour()
        //{
        //    if (targetShip.IsSunk()) targetShip.Stats.ArmourCount = 0;
        //    else
        //    {
        //        targetShip.Stats.ArmourCount -= ArmourDamage;

        //    }
        //}

        public virtual void RemoveHealth()
        {
            if (targetShip.IsSunk()) targetShip.Stats.HealthCount = 0;
            else
            {
                targetShip.Stats.HealthCount -= HealthDamage;

            }
        }
    }
}

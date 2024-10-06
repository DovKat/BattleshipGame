using backend.Models.Entity;
using Shared;

namespace backend.Strategies.Attacks.Damage
{
    public class ThemeDamageHandler : BaseDamageHandler
    {
        protected override int CalculateDamage(GameBoard gameBoard, int damageSum)
        {
            int damage = damageSum;
            switch (gameBoard.GetEnemyAttackShip().ShipType)
            {
                case ShipType.BigShip:
                    damage += 300;
                    break;
                case ShipType.MediumShip:
                    damage += 100;
                    break;
                case ShipType.SmallShip:
                    damage += 10;
                    break;
                default:
                    throw new InvalidOperationException("Unknown attack type");
            }
            return GetDamage(gameBoard, damage);
        }
    }
}

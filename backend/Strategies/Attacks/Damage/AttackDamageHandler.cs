using backend.Models.Entity;
using Shared;

namespace backend.Strategies.Attacks.Damage
{
    public class AttackDamageHandler : BaseDamageHandler
    {
        protected override int CalculateDamage(GameBoard gameBoard, int damageSum)
        {
            int damage = damageSum;

            switch (gameBoard.GetEnemyAttackShip().GetAttackTemplate().GetType().Name)
            {
                case "BigShipAttack":
                    damage += 100;
                    break;
                case "MediumShipAttack":
                    damage += 200;
                    break;
                case "SmallShipAttack":
                    damage += 50;
                    break;
                default:
                    throw new InvalidOperationException("Unknown ship type");
            }

            return GetDamage(gameBoard, damage);
        }
    }
}

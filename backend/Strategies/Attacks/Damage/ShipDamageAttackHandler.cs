using backend.Models.Entity;
using backend.Models.Entity.GameBoardExtensions;
using Shared;

namespace backend.Strategies.Attacks.Damage
{
    public class ShipDamageAttackHandler : BaseDamageHandler
    {
        protected override int CalculateDamage(GameBoard gameBoard, int damageSum)
        {
            int damage = damageSum;

            switch (gameBoard.GetTheme())
            {
                case LightTheme:
                    damage += 300;
                    break;
                case DarkTheme:
                    damage -= new Random().Next(0, 100);
                    break;
                default:
                    throw new InvalidOperationException("Unknown theme type");
            }

            return GetDamage(gameBoard, damage);
        }
    }
}

using backend.Models.Entity;
using Shared;

namespace backend.Strategies.Attacks.Damage
{
    public class FleetDamageHandler : BaseDamageHandler
    {
        protected override int CalculateDamage(GameBoard gameBoard, int damageSum)
        {
            int damage = damageSum;
            int standingShipcount = gameBoard.GetStandingShipCount();

            if(standingShipcount <= 1)
            {
                return _next.GetDamage(gameBoard, damage);
            }

            if (standingShipcount == 2)
            {
                damage *= 2;
            }
            else 
            {
                damage *= 3;
            }

            return GetDamage(gameBoard, damage);
        }
    }
}

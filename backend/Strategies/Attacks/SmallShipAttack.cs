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
    public sealed class SmallShipAttack : AttackTemplate
    {

        public override void AttackTarget()
        {
            if (attackShip.Stats.HealthCount > 100)
            {
                RemoveHealth();
            }
            else
            {
                attackShip.Stats.HealthCount = 0;
            }

        }

        public override void SetupWeapons()
        {
            bombFactory = new SmallBombFactory();
            attackStrategy = new SmallBombAttackStrategy();

            var h1 = new AttackDamageHandler();
            var h3 = new ShipDamageAttackHandler();

            h1.SetNext(h3);

            HealthDamage = h1.GetDamage(gameBoard,HealthDamage);
        }


    }
}

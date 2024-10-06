using backend.Models.Entity;
using backend.Models.Entity.Bombs;
using backend.Models.Entity.Bombs.BigBomb;
using backend.Models.Entity.Bombs.MediumBomb;
using backend.Models.Entity.Bombs.SmallBomb;
using backend.Models.Entity.Ships;
using backend.Strategies.Attacks.Damage;
using backend.Strategies.Ships;
using Shared;

namespace backend.Strategies.Attacks
{
    public sealed class MediumShipAttack : AttackTemplate
    {

        public override void AttackTarget()
        {
            if (attackShip.Stats.HealthCount > 500)
            {
                RemoveHealth();
            }
            else
            {
                RemoveHealth();
                //RemoveArmour();
            }

        }


        public override void SetupWeapons()
        {
            bombFactory = new MediumBombFactory();
            attackStrategy = new MediumBombAttackStrategy();

            var h1 = new AttackDamageHandler();
            var h3 = new ShipDamageAttackHandler();

            h1.SetNext(h3);

            HealthDamage = h1.GetDamage(gameBoard, HealthDamage);
        }
    }
}

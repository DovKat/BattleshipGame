namespace backend.Models.Entity.Bombs.MediumBomb
{
    public class MediumBombFactory : BombFactory
    {
        public override AtomicBomb CreateAtomicBomb()
        {
            return new MediumAtomicBomb();
        }

        public override MissileBomb CreateMissileBomb()
        {
            return new MediumMissileBomb();
        }
    }
}

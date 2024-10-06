namespace backend.Models.Entity.Bombs.BigBomb
{
    public class BigBombFactory : BombFactory
    {
        public override AtomicBomb CreateAtomicBomb()
        {
            return new BigAtomicBomb();
        }

        public override MissileBomb CreateMissileBomb()
        {
            return new BigMissileBomb();
        }
    }
}

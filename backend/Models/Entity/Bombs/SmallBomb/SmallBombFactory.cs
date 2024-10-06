namespace backend.Models.Entity.Bombs.SmallBomb
{
    public class SmallBombFactory : BombFactory
    {
        public override AtomicBomb CreateAtomicBomb()
        {
            return new SmallAtomicBomb();
        }

        public override MissileBomb CreateMissileBomb()
        {
            return new SmallMissileBomb();
        }
    }
}

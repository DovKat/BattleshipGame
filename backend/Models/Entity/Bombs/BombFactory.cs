namespace backend.Models.Entity.Bombs
{
    public abstract class BombFactory
    {
        public abstract AtomicBomb CreateAtomicBomb();

        public abstract MissileBomb CreateMissileBomb();
    }
}

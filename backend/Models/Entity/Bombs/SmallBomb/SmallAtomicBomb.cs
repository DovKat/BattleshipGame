using backend.Models.Entity.Ships;

namespace backend.Models.Entity.Bombs.SmallBomb
{
    public class SmallAtomicBomb : AtomicBomb
    {
        public override int Horizontal { get { return 2; } }
        public override int Vertical { get { return 2; } }

    }
}

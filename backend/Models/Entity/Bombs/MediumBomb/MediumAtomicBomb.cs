using backend.Models.Entity.Ships;

namespace backend.Models.Entity.Bombs.MediumBomb
{
    public class MediumAtomicBomb : AtomicBomb
    {
        public override int Horizontal { get { return 3; } }
        public override int Vertical { get { return 2; } }

    }
}

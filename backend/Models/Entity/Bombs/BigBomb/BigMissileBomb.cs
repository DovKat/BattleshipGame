using backend.Models.Entity.Ships;

namespace backend.Models.Entity.Bombs.BigBomb
{
    public class BigMissileBomb : MissileBomb
    {
        public override int Size { get { return 3; } }
        public override Orientation OrientationOf { get { return Orientation.Horizontal; } }

    }
}

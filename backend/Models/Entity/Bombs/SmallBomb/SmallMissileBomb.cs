using backend.Models.Entity.Ships;

namespace backend.Models.Entity.Bombs.SmallBomb
{
    public class SmallMissileBomb : MissileBomb
    {
        public override int Size { get { return 1; } }
        public override Orientation OrientationOf { get { return Orientation.Dot; } }

    }
}

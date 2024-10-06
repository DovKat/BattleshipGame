using backend.Models.Entity.Ships;

namespace backend.Models.Entity.Bombs.MediumBomb
{
    public class MediumMissileBomb : MissileBomb
    {
        public override int Size { get { return 2; } }
        public override Orientation OrientationOf { get { return Orientation.Vertical; } }

    }
}

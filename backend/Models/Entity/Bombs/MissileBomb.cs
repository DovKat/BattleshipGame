using backend.Models.Entity.Ships;

namespace backend.Models.Entity.Bombs
{
    /// <summary>
    /// MissileBomb that has only 1 width, 
    /// but has different length and orientation
    /// </summary>
    public abstract class MissileBomb
    {
        public abstract int Size { get; }

        public abstract Orientation OrientationOf { get; }
    }
}

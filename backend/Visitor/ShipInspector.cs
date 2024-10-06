using backend.Models.Entity.Ships;

namespace backend.Visitor
{
    public interface ShipInspector
    {
        int visit(SmallShip smallShip);
        int visit(MediumShip mediumShip);
        int visit(BigShip bigShip);
    }
}

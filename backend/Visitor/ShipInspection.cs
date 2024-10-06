using backend.Models.Entity.Ships;

namespace backend.Visitor
{
    public class ShipInspection : ShipInspector
    {
        public int visit(SmallShip smallShip)
        {
            int points = 175;
            if (smallShip.IsVertical)
            {
                points += 25;
            }
            return points;
        }

        public int visit(MediumShip mediumShip)
        {
            int points = 275;
            if (mediumShip.IsVertical)
            {
                points += 25;
            }
            return points;
        }

        public int visit(BigShip bigShip)
        {
            int points = 375;
            if (bigShip.IsVertical)
            {
                points += 25;
            }
            return points;
        }
    }
}

using backend.Models.Entity.Ships;

namespace backend.Models.Entity;

public class ArrangementDto
{
    public int x { get; set; }
    public int y { get; set; }
    public Ship Ship { get; set; }

    public ArrangementDto(Ship ship, int x, int y)
    {
        this.x = x;
        this.y = y;
        Ship = ship;
    }
}
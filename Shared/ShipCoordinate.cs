using Shared;

namespace backend.Models.Entity.Ships;


[Serializable]
public class ShipCoordinate
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsHit { get; private set; } = false;
    public string Icon { get; set; } = ShipCoordinateIcon.Ship;
    public Color Background { get; set; } = Color.Inherit;
    public Color BorderColor { get; set; } = Color.Inherit;

    public ShipCoordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Hit()
    {
        IsHit = true;
    }
}
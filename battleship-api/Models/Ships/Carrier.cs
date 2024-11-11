public class Carrier : Ship
{
    public Carrier()
    {
        Name = "Carrier";
        Length = 5;
        Orientation = "horizontal";
        isPlaced = false;
        HitCount = 0;
        IsSunk = false;
    }

    public override Ship ShallowCopy()
    {
        return (Ship)this.MemberwiseClone();
    }

    public override Ship DeepCopy()
    {
        var deepCopyShip = (Ship)this.MemberwiseClone();
        deepCopyShip.Coordinates = Coordinates.Select(coord => new Coordinate { Row = coord.Row, Column = coord.Column }).ToList();
        return deepCopyShip;
    }
}
public class TestPrototypeCopying{
    public void StartTest()
    {
        var originalShip = new BlueDestroyer
        {
            Name = "Destroyer",
            Length = 2,
            Coordinates = new List<Coordinate> { new Coordinate { Row = 1, Column = 1 } },
            Orientation = "horizontal",
            isPlaced = true
        };

        var shallowCopyShip = originalShip.ShallowCopy();
        var deepCopyShip = originalShip.DeepCopy();

        Console.WriteLine($"Original Ship HashCode: {originalShip.GetHashCode()}");
        Console.WriteLine($"Shallow Copy Ship HashCode: {shallowCopyShip.GetHashCode()}");
        Console.WriteLine($"Deep Copy Ship HashCode: {deepCopyShip.GetHashCode()}");

        Console.WriteLine($"Original Ship Coordinates HashCode: {originalShip.Coordinates.GetHashCode()}");
        Console.WriteLine($"Shallow Copy Ship Coordinates HashCode: {shallowCopyShip.Coordinates.GetHashCode()}");
        Console.WriteLine($"Deep Copy Ship Coordinates HashCode: {deepCopyShip.Coordinates.GetHashCode()}");
 main
    }
}

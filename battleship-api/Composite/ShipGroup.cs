using battleship_api.Command;

public class ShipGroup : IPlaceable
{
    private readonly List<IPlaceable> _ships = new();

    public void Add(IPlaceable ship) => _ships.Add(ship);
    public void Remove(IPlaceable ship) => _ships.Remove(ship);

    public void Place(Board board, CommandManager manager)
    {
        foreach (var ship in _ships)
        {
            ship.Place(board, manager);
        }
    }
}
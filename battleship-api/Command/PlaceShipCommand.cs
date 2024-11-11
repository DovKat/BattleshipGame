namespace battleship_api.Command;

public class PlaceShipCommand : ICommand
{
    private Board _board;
    private Ship _ship;
    private List<Coordinate> _shipCoordinates;

    public PlaceShipCommand(Board board, Ship ship, List<Coordinate> shipCoordinates)
    {
        _board = board;
        _ship = ship;
        _shipCoordinates = shipCoordinates;
    }

    public void Execute()
    {
        _board.Ships.Add(_ship);
        foreach (var coord in _shipCoordinates)
        {
            _board.Grid[coord.Row][coord.Column].HasShip = true;
        }
    }

    public void Undo()
    {
        _board.Ships.Remove(_ship);
        foreach (var coord in _shipCoordinates)
        {
            _board.Grid[coord.Row][coord.Column].HasShip = false;
        }
    }
}


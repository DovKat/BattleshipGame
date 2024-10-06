using backend.Models.Entity;
using backend.Models.Entity.Ships;

namespace backend.Service;

public class GameService
{
    public void AddShipToPlayer(Player player, Ship ship)
    {
        var playerBoard = player.OwnBoard;
        playerBoard.AddShip(ship);
    }

    public void CalculateShipCoordinates(Ship ship, int x, int y)
    {
        int tempX = x;
        int tempY = y;
        int size = ship.Size;
        bool isVertical = ship.IsVertical;

        for (int i = 0; i < size; i++)
        {
            ship.AddCoordinate(tempX, tempY);
            if (isVertical)
            {
                tempY++;
            } else
            {
                tempX++;
            }
        }
    }
    public bool CheckIfShipDoesNotFit(Ship ship)
    {
        foreach (ShipCoordinate coord in ship.GetCoordinates())
        {
            if (coord.X > 10 || coord.Y > 10)
            {
                return true;
            }
        }
        return false;
    }
}
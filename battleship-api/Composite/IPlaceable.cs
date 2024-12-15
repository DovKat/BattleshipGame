using battleship_api.Command;

public interface IPlaceable
{
    void Place(Board board, CommandManager manager);
}
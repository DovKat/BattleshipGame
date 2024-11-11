namespace battleship_api.Command;

public interface ICommand
{
    void Execute();

    void Undo();
}
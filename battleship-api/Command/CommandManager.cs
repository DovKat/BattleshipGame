namespace battleship_api.Command;

public class CommandManager
{
    private readonly Stack<ICommand> _commandHistory = new Stack<ICommand>();

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _commandHistory.Push(command);
    }

    public void UndoLastCommand()
    {
        if (_commandHistory.Count > 0)
        {
            var command = _commandHistory.Pop();
            command.Undo();
        }
        else
        {
            throw new InvalidOperationException("No commands to undo.");
        }
    }


    public bool HasCommands() => _commandHistory.Count > 0;
}

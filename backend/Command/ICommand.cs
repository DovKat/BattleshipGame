namespace backend.Command
{
    public interface ICommand
    {
        Task Execute();
        Task Undo();
    }
}

namespace battleship_api.State;

public interface IGameState
{
    Task StartGame(Game game, GameHub hub);
    Task PauseGame(Game game, GameHub hub);
    Task ResumeGame(Game game, GameHub hub);
    Task EndGame(Game game, GameHub hub);
}
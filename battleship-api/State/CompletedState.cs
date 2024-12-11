namespace battleship_api.State;

public class CompletedState : IGameState
{
    public Task StartGame(Game game, GameHub hub) => throw new InvalidOperationException("Cannot start a completed game.");

    public Task PauseGame(Game game, GameHub hub) => throw new InvalidOperationException("Cannot pause a completed game.");

    public Task ResumeGame(Game game, GameHub hub) => throw new InvalidOperationException("Cannot resume a completed game.");

    public Task EndGame(Game game, GameHub hub) => throw new InvalidOperationException("Game already ended.");
}
using Microsoft.AspNetCore.SignalR;

namespace battleship_api.State;

public class InProgressState : IGameState
{
    public Task StartGame(Game game, GameHub hub) => throw new InvalidOperationException("Game already in progress.");

    public async Task PauseGame(Game game, GameHub hub)
    {
        game.GameState = new PausedState(); // Transition to Paused
        await hub.Clients.Group(game.GameId).SendAsync("GamePaused", game);
    }

    public Task ResumeGame(Game game, GameHub hub) => throw new InvalidOperationException("Game is already in progress.");

    public async Task EndGame(Game game, GameHub hub)
    {
        game.GameState = new CompletedState(); // Transition to Completed
        await hub.Clients.Group(game.GameId).SendAsync("GameEnded", game);
    }
}
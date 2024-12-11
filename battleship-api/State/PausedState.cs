using Microsoft.AspNetCore.SignalR;

namespace battleship_api.State;

public class PausedState : IGameState
{
    public Task StartGame(Game game, GameHub hub) => throw new InvalidOperationException("Cannot start a paused game.");

    public async Task PauseGame(Game game, GameHub hub) => throw new InvalidOperationException("Game is already paused.");

    public async Task ResumeGame(Game game, GameHub hub)
    {
        game.State = "InProgress";
        game.GameState = new InProgressState(); // Transition to InProgress
        await hub.Clients.Group(game.GameId).SendAsync("GameResumed", game);
    }

    public async Task EndGame(Game game, GameHub hub)
    {
        game.State = "EndGame";
        game.GameState = new CompletedState(); // Transition to Completed
        await hub.Clients.Group(game.GameId).SendAsync("GameEnded", game);
    }
}
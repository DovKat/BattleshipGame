using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace battleship_api.State;

public class WaitingState : IGameState
{
    public async Task StartGame(Game game, GameHub hub)
    {
        
        game.State = "InProgress";
        Random random = new Random();
        game.CurrentTurn = (random.Next(2) == 0) ? "Red" : "Blue"; // Determine starting team
        game.CurrentPlayerIndex = 0;
        game.GameState = new InProgressState(); // Transition to InProgress
        game.NotifyObservers("GameStarted", game);

        Console.WriteLine($"GameStarted STATE");

        await hub.Clients.Group(game.GameId).SendAsync("UpdateGameState", game);
    }

    public Task PauseGame(Game game, GameHub hub) =>
        throw new InvalidOperationException("Cannot pause while waiting.");

    public Task ResumeGame(Game game, GameHub hub) =>
        throw new InvalidOperationException("Cannot resume while waiting.");

    public Task EndGame(Game game, GameHub hub) =>
        throw new InvalidOperationException("Cannot end while waiting.");
}
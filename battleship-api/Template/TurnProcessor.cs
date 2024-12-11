using Microsoft.AspNetCore.SignalR;

public abstract class TurnProcessor
{
    // Template Method
    public async Task ProcessTurn(GameHub hub, string gameId, string playerId, int row, int col, string attackType)
    {
        if (!await ValidateTurn(hub, gameId, playerId))
        {
            await hub.Clients.Caller.SendAsync("MoveNotAllowed", "Invalid move.");
            return;
        }
        await ExecuteTurn(hub, gameId, playerId, row, col, attackType);
        await EndTurn(hub, gameId);
    }

    // Steps of the Template Method
    protected virtual async Task<bool> ValidateTurn(GameHub hub, string gameId, string playerId)
    {
        var game = hub.GetGame(gameId);
        if (game == null || game.State != "InProgress")
        {
            await hub.Clients.Caller.SendAsync("MoveNotAllowed", "Game not in progress.");
            return false;
        }

        var player = game.Players.GetValueOrDefault(playerId);
        if (player == null || game.CurrentTurn != player.Team)
        {
            await hub.Clients.Caller.SendAsync("MoveNotAllowed", "Not your turn.");
            return false;
        }

        return true;
    }

    protected abstract Task ExecuteTurn(GameHub hub, string gameId, string playerId, int row, int col, string attackType);

    protected virtual async Task EndTurn(GameHub hub, string gameId)
    {
        var game = hub.GetGame(gameId);
        if (game != null)
        {
            hub.AdvanceTurn(game);
            await  hub.Clients.Group(gameId).SendAsync("UpdateGameState", game);
        }
    }
    
}

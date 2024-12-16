using Microsoft.AspNetCore.SignalR;

public abstract class TurnProcessor
{
    // Template Method
    public async Task<TurnResult> ProcessTurn(GameHub hub, string gameId, string playerId, string targetedPlayersId,int row, int col, string attackType)
    {
        if (!await ValidateTurn(hub, gameId, playerId))
        {
            await hub.Clients.Caller.SendAsync("MoveNotAllowed", "Invalid move.");
            return null;
        }
        var result = await ExecuteTurn(hub, gameId, playerId, targetedPlayersId, row, col, attackType);
        await EndTurn(hub, gameId);
        return result;
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

    protected virtual async Task<TurnResult> ExecuteTurn(GameHub hub, string gameId, string playerId,
        string targetedPlayerId, int row, int col, string attackType)
    {
        var game = hub.GetGame(gameId);
        var player = game?.Players.GetValueOrDefault(playerId);
        var opponentPlayer = game.Players.GetValueOrDefault(targetedPlayerId);
        var attackResult = hub.ProcessMove(opponentPlayer, row, col);
        await hub.Clients.Group(gameId).SendAsync("MoveResult", new
        {
            PlayerId = playerId,
            Row = row,
            Col =  col,
            Result = attackResult
        });
        var results = new List<string>();
        results.Add(attackResult);
        return new TurnResult
        {
            AffectedCoordinates = new List<Coordinate>
                {
                    new Coordinate { Row = row, Column = col }
                } 
                ,
            Results = results
        };
    }

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

using battleship_api.Strategy;
using battleship_api.Strategy.Decorator;
using Microsoft.AspNetCore.SignalR;

public class StandardTurnProcessor : TurnProcessor
{
    private readonly AttackStrategyFactory _strategyFactory = new();

    protected sealed override async Task<TurnResult> ExecuteTurn(GameHub hub, string gameId, string playerId, string targetedPlayerId, int row, int col, string attackType)
    {
        var game = hub.GetGame(gameId);
        var player = game.Players.GetValueOrDefault(playerId);
        var opponentPlayer = game.Players.GetValueOrDefault(targetedPlayerId);

        if (game == null && player == null && opponentPlayer == null)
        {
            return null;
        }
        IAttackStrategy baseAttack = _strategyFactory.GetStrategy(attackType.ToLower());

        // Apply decorators to the base attack strategy
        IAttackStrategy decoratedAttack = new LoggingDecorator(baseAttack);

        if (attackType == "bigbomb" || attackType == "megabomb")
        {
            decoratedAttack = new SplashDamageDecorator(decoratedAttack);
        }
        if (attackType == "smallbomb") 
        {
            decoratedAttack = new DoubleDamageDecorator(decoratedAttack);
        }
     
        if (player == null || game.State != "InProgress" || game.CurrentTurn != player.Team)
        {
            await hub.Clients.Caller.SendAsync("MoveNotAllowed", "It's not your turn.");
            return null;
        }

        var team = game.Teams.First(t => t.Name == player.Team);
        if (team.Players[game.CurrentPlayerIndex].Id != playerId)
        {
            await hub.Clients.Caller.SendAsync("MoveNotAllowed", "It's not your turn.");
            return null;
        }

        var startCoordinate = new Coordinate { Row = row, Column = col };
        var affectedCoordinates = decoratedAttack.GetAffectedCoordinates(startCoordinate);

        /*   foreach (var coord in affectedCoordinates)
        {
            _logger.LogInformation($"- Row: {coord.Row}, Column: {coord.Column}");
        } */

        var results = new List<string>();

        foreach (var coord in affectedCoordinates)
        {
            if (coord.Row >= 0 && coord.Row < 10 && coord.Column >= 0 && coord.Column < 10)
            {
                var hitResult = hub.ProcessMove(opponentPlayer, coord.Row, coord.Column);
                results.Add(hitResult);

                await hub.Clients.Group(gameId).SendAsync("MoveResult", new
                {
                    PlayerId = playerId,
                    Row = coord.Row,
                    Col = coord.Column,
                    Result = hitResult
                });
            }
        }

        return new TurnResult
        {
            AffectedCoordinates = affectedCoordinates,
            Results = results
        };
    }
}

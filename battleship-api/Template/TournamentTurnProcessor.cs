using battleship_api.Strategy;
using battleship_api.Strategy.Decorator;
using Microsoft.AspNetCore.SignalR;

public class TournamentTurnProcessor : TurnProcessor
{
    private readonly AttackStrategyFactory _strategyFactory = new();

    protected sealed override async Task<TurnResult> ExecuteTurn(GameHub hub, string gameId, string playerId, string targetedPlayerId, int row, int col, string attackType)
    {
        var game = hub.GetGame(gameId);
        var player = game?.Players.GetValueOrDefault(playerId);
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
        if (attackType == "smallbomb") // Example: specific player has double damage
        {
            decoratedAttack = new DoubleDamageDecorator(decoratedAttack);
        }

        // Process the move using the decorated attack strategy
        var affectedCoordinates = decoratedAttack.GetAffectedCoordinates(new Coordinate { Row = row, Column = col });

        var results = new List<string>();

        foreach (var coord in affectedCoordinates)
        {
            var attackResult = hub.ProcessMove(opponentPlayer, coord.Row, coord.Column);
            results.Add(attackResult);


            // Send the result of the move
            await hub.Clients.Group(gameId).SendAsync("MoveResult", new
            {
                PlayerId = playerId,
                Row = coord.Row,
                Col = coord.Column,
                Result = attackResult
            });
        }

        return new TurnResult
        {
            TargetPlayerId = targetedPlayerId, 
            AffectedCoordinates = affectedCoordinates,
            Results = results
        };
    }

    protected override async Task EndTurn(GameHub hub, string gameId)
    {
        await base.EndTurn(hub, gameId);

        // Tournament-specific end turn logic
        Console.WriteLine("Additional tournament-specific end turn logic.");
    }
}

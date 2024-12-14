using battleship_api.Strategy;
using battleship_api.Strategy.Decorator;
using Microsoft.AspNetCore.SignalR;

public class StandardTurnProcessor : TurnProcessor
{
    private readonly AttackStrategyFactory _strategyFactory = new();
    protected override async Task ExecuteTurn(GameHub hub, string gameId, string playerId, int row, int col, string attackType)
    {
        var game = hub.GetGame(gameId);
        var player = game.Players.GetValueOrDefault(playerId);
        var opponentTeam = game.Teams.FirstOrDefault(t => t.Name != player?.Team);

        if (game != null && player != null && opponentTeam != null)
        {
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
            
            if (player == null || game.State != "InProgress" || game.CurrentTurn != player.Team)
            {
                await hub.Clients.Caller.SendAsync("MoveNotAllowed", "It's not your turn.");
                return;
            }
    
            var team = game.Teams.First(t => t.Name == player.Team);
            if (team.Players[game.CurrentPlayerIndex].Id != playerId)
            {
                await hub.Clients.Caller.SendAsync("MoveNotAllowed", "It's not your turn.");
                return;
            }
    
            var startCoordinate = new Coordinate { Row = row, Column = col };
            var affectedCoordinates = decoratedAttack.GetAffectedCoordinates(startCoordinate);
    
         /*   foreach (var coord in affectedCoordinates)
            {
                _logger.LogInformation($"- Row: {coord.Row}, Column: {coord.Column}");
            } */
    
            foreach (var coord in affectedCoordinates)
            {
                if (coord.Row >= 0 && coord.Row < 10 && coord.Column >= 0 && coord.Column < 10) // Ensure within board bounds
                {
                    var hitResult = hub.ProcessMove(opponentTeam, coord.Row, coord.Column);
                    await  hub.Clients.Group(gameId).SendAsync("MoveResult", new { PlayerId = playerId, Row = coord.Row, Col = coord.Column, Result = hitResult });
                }
            }
        }
    }
}

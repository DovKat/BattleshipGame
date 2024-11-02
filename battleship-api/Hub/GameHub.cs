using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;



public class GameHub : Hub
{
    private static readonly Dictionary<string, Game> _games = new Dictionary<string, Game>();

    public async Task JoinTeam(string gameId, string team, string playerName, string playerId)
    {
        try
        {
            Console.WriteLine($"JoinTeam called with gameId: {gameId}, team: {team}, playerName: {playerName}, playerId: {playerId}");

            var game = _games.GetValueOrDefault(gameId) ?? CreateNewGame(gameId);

            var playerTeam = game.Teams.FirstOrDefault(t => t.Name.Equals(team, StringComparison.OrdinalIgnoreCase));
            if (playerTeam == null)
            {
                await Clients.Caller.SendAsync("JoinTeamFailed", "The team does not exist.");
                return;
            }

            if (playerTeam.Players.Count < 2)
            {
                var player = new Player
                {
                    Id = playerId,
                    Name = playerName,
                    Team = team,
                    Board = InitializeBoard(),
                    IsReady = false
                };

                playerTeam.Players.Add(player);
                game.Players[playerId] = player;

                _games[gameId] = game;

                await Groups.AddToGroupAsync(Context.ConnectionId, gameId); // Add player to the game-specific group
                await Clients.Group(gameId).SendAsync("UpdateTeams", game.Teams); // Update only players in this game
                await Clients.Group(gameId).SendAsync("PlayerJoined", player);

                await CheckStartGame(game);
            }
            else
            {
                await Clients.Caller.SendAsync("JoinTeamFailed", "The team is full.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinTeam: {ex.Message}");
            await Clients.Caller.SendAsync("JoinTeamFailed", "An error occurred while joining the team.");
        }
    }

    // Method to create a new game
    private Game CreateNewGame(string gameId)
    {
        var newGame = new Game
        {
            GameId = gameId,
            Teams = new List<Team>
            {
                new Team { Name = "Red", Players = new List<Player>() },
                new Team { Name = "Blue", Players = new List<Player>() }
            },
            State = "Waiting",
            Players = new Dictionary<string, Player>()
        };
        _games[gameId] = newGame; // Store the new game
        return newGame;
    }
    public async Task SetPlayerReady(string gameId, string playerId)
{
    Console.WriteLine($"SetPlayerReady called with GameId: {gameId}, PlayerId: {playerId}");

    if (_games.TryGetValue(gameId, out var game) && game.Players.TryGetValue(playerId, out var player))
    {
        player.IsReady = true;
        await Clients.Group(gameId).SendAsync("PlayerReady", playerId);

        // Check if all players in both teams are ready
        if (game.Teams.All(t => t.Players.All(p => p.IsReady)))
        {
            game.State = "InProgress";
            game.CurrentTurn = "Red";  // Assuming Red team starts
            game.CurrentPlayerIndex = 0;

            // Send the initial state of each player's board to them
            foreach (var team in game.Teams)
            {
                foreach (var p in team.Players)
                {
                    await Clients.Client(p.Id).SendAsync("GameStarted", p.Board);  // Each player receives their own board
                }
            }

            await Clients.Group(gameId).SendAsync("UpdateGameState", game); // Notify all players the game is starting
            await CheckStartGame(game);
        }
    }
}



public async Task PlaceShip(string gameId, string playerId, List<Ship> shipCoordinates)
{
    if (_games.TryGetValue(gameId, out var game) && game.Players.TryGetValue(playerId, out var player))
    {
        player.Board.Ships.Clear();
        

       
        
        Console.WriteLine("NICE");
        // If all checks pass, place the ships
        
        foreach (var ship in shipCoordinates)
        {   
            Ship newShip = ship;
            player.Board.Ships.Add(newShip);
            foreach (var coord in ship.Coordinates)
            {
                player.Board.Grid[coord.Row][coord.Column].HasShip = true;
            }
        }
        await Clients.Caller.SendAsync("ShipPlaced", player.Board); // Send updated board to player
        await Clients.Group(gameId).SendAsync("UpdateGameState", game); // Notify all clients in the group of the update
    }
}

    // Method to check if the game can start
    private async Task CheckStartGame(Game game)
{
    Console.WriteLine($"Checking if game {game.GameId} can start...");
    if (game.Teams.All(t => t.Players.Count == 2 && t.Players.All(p => p.IsReady)))
    {
        game.State = "InProgress";
        game.CurrentTurn = "Red";
        game.CurrentPlayerIndex = 0;
        Console.WriteLine($"Game started: {game.GameId} with initial turn: {game.CurrentTurn}");
        await Clients.Group(game.GameId).SendAsync("GameStarted", game);
    }
    else
    {
        Console.WriteLine("Game cannot start: Not all players are ready or teams do not have required players.");
    }
}


    // Method to handle player moves
    public async Task UpdatePlayerState(string gameId, Player playerState)
    {
        var game = _games.GetValueOrDefault(gameId);
        if (game == null)
        {
            await Clients.Caller.SendAsync("GameStateUpdateFailed", "Game not found.");
            return;
        }

        // Update the player's state in the game
        if (game.Players.TryGetValue(playerState.Id, out var player))
        {
            player.Board = playerState.Board; // Update player's board
            // Add more player state updates here if needed
            await Clients.Group(gameId).SendAsync("UpdateGameState", game); // Notify all clients in the group
        }
        else
        {
            await Clients.Caller.SendAsync("GameStateUpdateFailed", "Player not found.");
        }
    }

    public async Task MakeMove(string gameId, string playerId, int row, int col)
{
    if (_games.TryGetValue(gameId, out var game))
    {
        var player = game.Players.GetValueOrDefault(playerId);
        if (player == null || game.State != "InProgress" || game.CurrentTurn != player.Team)
        {
            Console.WriteLine($"Move not allowed: PlayerId: {playerId}, CurrentTurn: {game.CurrentTurn}, Team: {player?.Team}, State: {game.State}");
            await Clients.Caller.SendAsync("MoveNotAllowed", "It's not your turn.");
            return;
        }

        // Check if it's the player's turn within their team
        var team = game.Teams.First(t => t.Name == player.Team);
        if (team.Players[game.CurrentPlayerIndex].Id != playerId)
        {
            await Clients.Caller.SendAsync("MoveNotAllowed", "It's not your turn.");
            return;
        }

        var opponentTeam = game.Teams.First(t => t.Name != player.Team);
        var hitResult = ProcessMove(opponentTeam, row, col);

        Console.WriteLine($"Move made by {playerId} at ({row}, {col}): {hitResult}");
        await Clients.Group(gameId).SendAsync("MoveResult", new { PlayerId = playerId, Row = row, Col = col, Result = hitResult });

        // Check if the opponent team lost all ships
        if (opponentTeam.Players.All(p => p.Board.Ships.All(s => s.IsSunk)))
        {
            game.State = "Ended";
            await Clients.Group(gameId).SendAsync("GameEnded", $"{player.Team} team wins!");
            Console.WriteLine($"Game ended: {player.Team} team wins!");
        }
        else
        {
            AdvanceTurn(game);  // Advance to the next player's turn
            await Clients.Group(gameId).SendAsync("UpdateGameState", game);
        }
    }
}




    private string ProcessMove(Team opponentTeam, int row, int col)
    {
        foreach (var player in opponentTeam.Players)
        {
            var cell = player.Board.Grid[row][col];
            if (cell.HasShip && !cell.IsHit)
            {
                cell.IsHit = true;
                var ship = player.Board.Ships.First(s => s.Coordinates.Any(c => c.Row == row && c.Column == col));
                ship.HitCount++;

                if (ship.IsSunk)
                {
                    return "Sunk";
                }

                return "Hit";
            }
        }
        return "Miss";
    }

    private void AdvanceTurn(Game game)
{
    var currentTeam = game.Teams.First(t => t.Name == game.CurrentTurn);
    game.CurrentPlayerIndex = (game.CurrentPlayerIndex + 1) % currentTeam.Players.Count;

    if (game.CurrentPlayerIndex == 0) // After each player in the team has taken a turn, switch teams
    {
        game.CurrentTurn = game.CurrentTurn == "Red" ? "Blue" : "Red";
    }

    Console.WriteLine($"Advanced turn: {game.CurrentTurn}, Current Player Index: {game.CurrentPlayerIndex}");
}



    // Method to get connection ID
    public string GetConnectionId()
    {
        return Context.ConnectionId;
    }

    // Override for disconnection logic
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var playerToRemove = _games.Values
            .SelectMany(g => g.Teams)
            .SelectMany(t => t.Players)
            .FirstOrDefault(p => p.Id == Context.ConnectionId);

        if (playerToRemove != null)
        {
            var game = _games.Values.First(g => g.Teams.Any(t => t.Players.Contains(playerToRemove)));
            var team = game.Teams.First(t => t.Name == playerToRemove.Team);

            team.Players.Remove(playerToRemove);
            game.Players.Remove(playerToRemove.Id);


            await Clients.Group(game.GameId).SendAsync("UpdateTeams", game.Teams);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, game.GameId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Method to initialize a player's board
    private Board InitializeBoard()
    {
        var cells = new Cell[10][];
        for (int i = 0; i < 10; i++)
        {
            cells[i] = new Cell[10];
            for (int j = 0; j < 10; j++)
            {
                cells[i][j] = new Cell { HasShip = false, IsHit = false };
            }
        }
        return new Board { Grid = cells, Ships = new List<Ship>() };
    }
}

using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using backend.GameManager;
using backend.ShipFactory;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using battleship_api.Builder;


using battleship_api.Command;


using battleship_api.Strategy.Decorator;
using battleship_api.Strategy;
using battleship_api.Bridge;


public class GameHub : Hub, IGameObserver
{
    private readonly ILogger<GameHub> _logger;
    private readonly Dictionary<string, AbstractFactory> _teamShipFactories;
    private static readonly Dictionary<string, Game> _games = new Dictionary<string, Game>();

    private static readonly Dictionary<string, CommandManager> _commandManagers = new Dictionary<string, CommandManager>();

    private static readonly Dictionary<string, string> _gameModes = new Dictionary<string, string>(); // Stores selected game modes per gameId
    private static string selectedMode = "";
    private static IScoringSystem _scoringSystem;

    public GameHub(ILogger<GameHub> logger)
    {
        _teamShipFactories = new Dictionary<string, AbstractFactory>
        {
            { "Blue", new BlueTeamShipFactory() },
            { "Red", new RedTeamShipFactory() }
        };
        _logger = logger;
    }
    public async Task Update(Game game, string messageType, object data)
    {
        switch (messageType)
        {
            case "PlayerJoined":
                var player = (Player)data;
                await Clients.Group(game.GameId).SendAsync("PlayerJoined", player);
                break;
            case "UpdateTeams":
                var teams = (List<Team>)data;
                await Clients.Group(game.GameId).SendAsync("UpdateTeams", teams);
                break;
            case "GameStarted":
                await Clients.Group(game.GameId).SendAsync("GameStarted", game);
                break;
            case "UpdateGameState":
                await Clients.Group(game.GameId).SendAsync("UpdateGameState", game);
                break;
            // Add more cases as needed for different notifications
        }
    }

    public async Task JoinTeam(string gameId, string team, string playerName, string playerId, string gameMode)
    {
        try
        {
            Console.WriteLine($"JoinTeam called with gameId: {gameId}, team: {team}, playerName: {playerName}, playerId: {playerId}, gameMode: {gameMode}");

            // Ensure that the game exists
            if (selectedMode != "")
            {
                gameMode = selectedMode;
            }
            var game = _games.GetValueOrDefault(gameId) ?? CreateGame(gameId, gameMode); // Pass gameMode here to create the correct game type
            _logger.LogInformation("Selected mode {0}", gameMode);
            game.AddObserver(this);

            var playerTeam = game.Teams.FirstOrDefault(t => t.Name.Equals(team, StringComparison.OrdinalIgnoreCase));
            if (playerTeam == null)
            {
                await Clients.Caller.SendAsync("JoinTeamFailed", "The team does not exist.");
                return;
            }

            if (playerTeam.Players.Count < 2)
            {

                var player = CreateNewPlayer(playerId, playerName, team, InitializeBoard(), false);

                player.IsReady = false;
                playerTeam.Players.Add(player);
                game.Players[playerId] = player;

                _games[gameId] = game;

                await Groups.AddToGroupAsync(Context.ConnectionId, gameId); // Add player to the game-specific group
                
                game.PlayerJoined(player);
                
                //await Clients.Group(gameId).SendAsync("UpdateTeams", game.Teams); // Update only players in this game
               // await Clients.Group(gameId).SendAsync("PlayerJoined", player);

                await CheckStartGame(game);
            }
            else
            {
                await Clients.Caller.SendAsync("JoinTeamFailed", "The team is full.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JoinTeam: {Message}", ex.Message);
            await Clients.Caller.SendAsync("JoinTeamFailed", "An error occurred while joining the team.");
        }
    }


    public Game CreateGame(string gameId, string gameMode)
    {
        selectedMode = gameMode;
        if (gameMode == "tournament")
        {
            _scoringSystem = new TournamentScoringSystem(1);
            return CreateTournamentNewGame(gameId);
        }
        else
        {
            _scoringSystem = new StandardScoringSystem();
            return CreateStandartNewGame(gameId);
        }
    }

    // Method to create a new game
    public Game CreateTournamentNewGame(string gameId)
    {
        IGameBuilder gameBuilder = new TournamentGameBuilder();
        gameBuilder.SetGameId(gameId);
        gameBuilder.SetTeams(new List<Team>
        {
            new Team { Name = "Red", Players = new List<Player>() },
            new Team { Name = "Blue", Players = new List<Player>() }
        });
        gameBuilder.SetState("Waiting");
        gameBuilder.SetMode();
        var game = gameBuilder.Build();
        _games[gameId] = game;
        return game;
    }
    public Game CreateStandartNewGame(string gameId)

    {
        IGameBuilder gameBuilder = new StandardGameBuilder();
        gameBuilder.SetGameId(gameId);
        gameBuilder.SetTeams(new List<Team>
        {
            new Team { Name = "Red", Players = new List<Player>() },
            new Team { Name = "Blue", Players = new List<Player>() }
        });
        gameBuilder.SetState("Waiting");

        var game = gameBuilder.Build();
        _games[gameId] = game;
        return game;
    }
    public Player CreateNewPlayer(string playerId, string playerName, string team, Board board, bool isAI = false)
    {
        IPlayerBuilder playerBuilder = isAI ? new AIPlayerBuilder() : new StandardPlayerBuilder();
        playerBuilder.SetPlayerId(playerId);
        playerBuilder.SetPlayerName(playerName);
        playerBuilder.SetTeam(team);
        playerBuilder.SetBoard(board);

        return playerBuilder.Build();
    }
  
    public async Task SetPlayerReady(string gameId, string playerId)
    {
        Console.WriteLine($"SetPlayerReady called with GameId: {gameId}, PlayerId: {playerId}");

        if (_games.TryGetValue(gameId, out var game) && game.Players.TryGetValue(playerId, out var player))
        {
            player.IsReady = true;
        
            await Clients.Group(gameId).SendAsync("PlayerReady", playerId);
            game.NotifyObservers("PlayerReady", playerId);  // Notify observers about the player ready status

            if (game.Teams.All(t => t.Players.All(p => p.IsReady)))
            {
                game.State = "InProgress";
                game.CurrentTurn = "Red";  // Assuming Red team starts
                game.CurrentPlayerIndex = 0;

                foreach (var team in game.Teams)
                {
                    foreach (var p in team.Players)
                    {
                        await Clients.Client(p.Id).SendAsync("GameStarted", p.Board);
                    }
                }

                await Clients.Group(gameId).SendAsync("UpdateGameState", game);
                game.NotifyObservers("GameStarted", game);  // Notify observers about the game starting
            }
        }
    }

    public async Task GenerateRandomShips(string gameId, string playerId)
    {
        if (_games.TryGetValue(gameId, out var game) && game.Players.TryGetValue(playerId, out var player))
        {
            var commandManager = GetCommandManagerForPlayer(gameId, playerId);
            var shipFactory = new ShipFactory();
            var randomPlacer = new RandomShipPlacer(shipFactory, boardSize: 10);
            randomPlacer.FillBoardWithRandomShips(player.Board, commandManager);
            await Clients.Caller.SendAsync("ShipPlaced", player.Board); // Send updated board to player
            await Clients.Group(gameId).SendAsync("UpdateGameState", game); // Notify all clients in the group of the update
        }
        else
        {
            await Clients.Caller.SendAsync("ShipPlacementFailed", "Game or player not found.");
        }
    }


    public async Task PlaceShip(string gameId, string playerId, string shipType, int row, int col, string orientation)
    {
        if (_games.TryGetValue(gameId, out var game) && game.Players.TryGetValue(playerId, out var player))
        {

            var commandManager = GetCommandManagerForPlayer(gameId, playerId);

            // Determine the correct factory based on the team
            AbstractFactory shipFactory = player.Team == "Blue" ? new BlueTeamShipFactory() : new RedTeamShipFactory();

            // Use the factory to create the ship based on shipType
            Ship newShip = shipType switch
            {
                "Destroyer" => shipFactory.CreateDestroyer(),
                "Submarine" => shipFactory.CreateSubmarine(),
                "Cruiser" => shipFactory.CreateCruiser(),
                "Battleship" => shipFactory.CreateBattleship(),
                "Carrier" => shipFactory.CreateCarrier(),
                _ => throw new ArgumentException("Invalid ship type")
            };

            newShip.Orientation = orientation;
            newShip.isPlaced = true;

            Coordinate startCoordinate = new Coordinate { Row = row, Column = col };
            var shipCoordinates = CalculateShipCoordinates(startCoordinate, newShip.Length, orientation);

            if (!IsPlacementValid(player.Board, shipCoordinates))
            {
                await Clients.Caller.SendAsync("ShipPlacementFailed", "Invalid ship placement.");
                return;
            }

            newShip.Coordinates = shipCoordinates;

            newShip.isPlaced = true;    
            //player.Board.Ships.Add(newShip);

            //foreach (var coord in shipCoordinates)
            //{
            //    player.Board.Grid[coord.Row][coord.Column].HasShip = true;
            //}

            var placeCommand = new PlaceShipCommand(player.Board, newShip, shipCoordinates);
            commandManager.ExecuteCommand(placeCommand);


            await Clients.Caller.SendAsync("ShipPlaced", player.Board);
            await Clients.Group(gameId).SendAsync("UpdateGameState", game);
            game.NotifyObservers("ShipPlaced", new { PlayerId = playerId, Board = player.Board });
        }
        else
        {
            await Clients.Caller.SendAsync("ShipPlacementFailed", "Game or player not found.");
        }
    }


    public async Task UndoLastPlacement(string gameId, string playerId)
    {
        if (_games.TryGetValue(gameId, out var game) && game.Players.TryGetValue(playerId, out var player))
        {
            var commandManager = GetCommandManagerForPlayer(gameId, playerId);
            if (commandManager.HasCommands())
            {
                commandManager.UndoLastCommand();

                await Clients.Caller.SendAsync("ShipPlacementUndone", player.Board);
                await Clients.Group(gameId).SendAsync("UpdateGameState", game);
            }
            else
            {
                await Clients.Caller.SendAsync("ShipPlacementFailed", "No actions to undo.");
            }
        }
        else
        {
            await Clients.Caller.SendAsync("ShipPlacementFailed", "Game or player not found.");
        }
    }



    private CommandManager GetCommandManagerForPlayer(string gameId, string playerId)
    {
        var key = $"{gameId}-{playerId}";
        if (!_commandManagers.TryGetValue(key, out var commandManager))
        {
            commandManager = new CommandManager();
            _commandManagers[key] = commandManager; // Store the CommandManager persistently
        }
        return commandManager;
    }



    private List<Coordinate> CalculateShipCoordinates(Coordinate start, int length, string orientation)
    {
        var coordinates = new List<Coordinate>();

        for (int i = 0; i < length; i++)
        {
            int row = orientation == "horizontal" ? start.Row : start.Row + i;
            int col = orientation == "horizontal" ? start.Column + i : start.Column;
            coordinates.Add(new Coordinate { Row = row, Column = col });
        }

        return coordinates;
    }
    public async Task PlaceRandomShipsForDemo(string gameId)
{
    if (_games.TryGetValue(gameId, out var game))
    {
        var random = new Random();
        var shipTypes = new List<string> { "Carrier", "Battleship", "Destroyer", "Submarine", "PatrolBoat" };
        
        foreach (var team in game.Teams)
        {
            foreach (var player in team.Players)
            {
                foreach (var shipType in shipTypes)
                {
                    ShipFactory shipFactory = new ShipFactory();
                    var newShip = shipFactory.CreateShip(shipType);
                    
                    bool placed = false;
                    while (!placed)
                    {
                        int row = random.Next(0, 10);
                        int col = random.Next(0, 10);
                        string orientation = random.Next(0, 2) == 0 ? "horizontal" : "vertical";
                        var startCoordinate = new Coordinate { Row = row, Column = col };
                        var shipCoordinates = CalculateShipCoordinates(startCoordinate, newShip.Length, orientation);

                        if (IsPlacementValid(player.Board, shipCoordinates))
                        {
                            newShip.Coordinates = shipCoordinates;
                            newShip.Orientation = orientation;
                            newShip.isPlaced = true;
                            player.Board.Ships.Add(newShip);

                            foreach (var coord in shipCoordinates)
                            {
                                player.Board.Grid[coord.Row][coord.Column].HasShip = true;
                            }

                            placed = true;
                        }
                    }
                }

                await Clients.Client(player.Id).SendAsync("ShipPlacementComplete", player.Board);
            }
        }
        
        await Clients.Group(gameId).SendAsync("UpdateGameState", game);
    }
}
    private bool IsPlacementValid(Board board, List<Coordinate> coordinates)
    {
        foreach (var coord in coordinates)
        {
            // Check bounds
            if (coord.Row < 0 || coord.Row >= board.Grid.Length ||
                coord.Column < 0 || coord.Column >= board.Grid[0].Length)
            {
                return false;
            }

            // Check for overlap
            if (board.Grid[coord.Row][coord.Column].HasShip)
            {
                return false;
            }
        }
        return true;
    }

    // Method to check if the game can start
    private async Task CheckStartGame(Game game)
    {

        if (game.Teams.All(t => t.Players.Count == 2 && t.Players.All(p => p.IsReady)))
        {
            game.State = "InProgress";
            game.CurrentTurn = "Red";  // Assuming Red team starts
            game.CurrentPlayerIndex = 0;

            foreach (var team in game.Teams)
            {
                foreach (var p in team.Players)
                {
                    await Clients.Client(p.Id).SendAsync("GameStarted", p.Board);
                    //await Clients.Caller.SendAsync("ReceiveGameMode", selectedMode);

                }
            }

            await Clients.Group(game.GameId).SendAsync("UpdateGameState", game);
            game.NotifyObservers("GameStarted", game);  // Notify observers about the game starting
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

    public async Task UpdatePlayerScore(string playerID, string hitResult, string gameId, string attackType)
    {
        int playerScore = GameManager.Instance.GetPlayerScore(playerID);
        int points = _scoringSystem.CalculateScore(hitResult);

        GameManager.Instance.UpdatePlayerScore(playerID, points);

        // Send each property separately
        await Clients.Group(gameId).SendAsync("ReceiveUpdatedScore", 
            playerID, 
            GameManager.Instance.GetPlayerScore(playerID), 
            points, 
            hitResult);

        await Console.Out.WriteLineAsync($"{playerID} current score is {playerScore}. Points received: {points}. Shot result: {hitResult}");
    }
    public async Task MakeMove(string gameId, string playerId, int row, int col, string attackType)
    {

    IAttackStrategy baseAttack = attackType switch
    {
        "smallbomb" => new SmallBombAttack(),
        "bigbomb" => new BigBombAttack(),
        "megabomb" => new MegaBombAttack(),
        _ => new RegularAttack()
    };

    // Dynamically add decorators
    IAttackStrategy decoratedAttack = new LoggingDecorator(baseAttack);
    if (attackType == "bigbomb" || attackType == "megabomb")
    {
        decoratedAttack = new SplashDamageDecorator(decoratedAttack);
    }
    if (attackType == "smallbomb") // Example: specific player has double damage
    {
        decoratedAttack = new DoubleDamageDecorator(decoratedAttack);
    }

    await MakeMove(gameId, playerId, row, col, decoratedAttack);

    }

    private async Task MakeMove(string gameId, string playerId, int row, int col, IAttackStrategy attackStrategy)
    {
        if (_games.TryGetValue(gameId, out var game))
        {
            var player = game.Players.GetValueOrDefault(playerId);
            if (player == null || game.State != "InProgress" || game.CurrentTurn != player.Team)
            {
                await Clients.Caller.SendAsync("MoveNotAllowed", "It's not your turn.");
                return;
            }
    
            var team = game.Teams.First(t => t.Name == player.Team);
            if (team.Players[game.CurrentPlayerIndex].Id != playerId)
            {
                await Clients.Caller.SendAsync("MoveNotAllowed", "It's not your turn.");
                return;
            }
    
            var opponentTeam = game.Teams.First(t => t.Name != player.Team);
    
            // Get affected coordinates based on the attack strategy
            var startCoordinate = new Coordinate { Row = row, Column = col };
            var affectedCoordinates = attackStrategy.GetAffectedCoordinates(startCoordinate);
    
            // Log affected coordinates
            _logger.LogInformation($"Player {playerId} attacked at ({row}, {col}) using {attackStrategy.GetType().Name}. Affected coordinates:");
            foreach (var coord in affectedCoordinates)
            {
                _logger.LogInformation($"- Row: {coord.Row}, Column: {coord.Column}");
            }
    
            foreach (var coord in affectedCoordinates)
            {
                if (coord.Row >= 0 && coord.Row < 10 && coord.Column >= 0 && coord.Column < 10) // Ensure within board bounds
                {
                    var hitResult = ProcessMove(opponentTeam, coord.Row, coord.Column);
                    await Clients.Group(gameId).SendAsync("MoveResult", new { PlayerId = playerId, Row = coord.Row, Col = coord.Column, Result = hitResult });
                }
            }
    
            AdvanceTurn(game);
            await Clients.Group(gameId).SendAsync("UpdateGameState", game);
        }
    }





    private string ProcessMove(Team opponentTeam, int row, int col)
    {
        foreach (var player in opponentTeam.Players)
        {
            var cell = player.Board.Grid[row][col];  // Get the cell being attacked
            if (cell.HasShip && !cell.IsHit)
            {
                cell.IsHit = true;  // Mark the cell as hit
                var ship = player.Board.Ships.First(s => s.Coordinates.Any(c => c.Row == row && c.Column == col));
                ship.HitCount++;  // Increment the ship's hit count

                if (ship.IsSunk)
                {
                    return "Sunk";  // Ship is sunk
                }

                return "Hit";  // Ship is hit but not yet sunk
            }
        }

        // If no ship was hit, mark it as a miss
        opponentTeam.Players.ForEach(player =>
        {
            var missedCell = player.Board.Grid[row][col];
            missedCell.IsMiss = true;  // Mark this cell as a miss
        });

        return "Miss";  // No ship, it's a miss
    }

    private async void AdvanceTurn(Game game)
    {
        var currentTeam = game.Teams.First(t => t.Name == game.CurrentTurn);
        game.CurrentPlayerIndex = (game.CurrentPlayerIndex + 1) % currentTeam.Players.Count;

        if (game.CurrentPlayerIndex == 0) // After each player in the team has taken a turn, switch teams
        {
            game.CurrentTurn = game.CurrentTurn == "Red" ? "Blue" : "Red";
        }

        await Clients.Group(game.GameId).SendAsync("UpdateGameState", new { game.CurrentTurn, game.CurrentPlayerIndex });

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
            
            if (game != null)
            {
                game.RemoveObserver(this);
            }
            
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

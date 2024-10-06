using backend.Command;
using backend.Manager;
using backend.Models.Entity;
using backend.Models.Entity.GameBoardExtensions;
using backend.Models.Entity.Proxy;
using backend.Models.Entity.Ships;
using backend.Models.Entity.Ships.Decorators;
using backend.Models.Entity.Ships.Factory;
using backend.Models.Entity.Ships.Generator;
using backend.Observer;
using backend.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Shared;
using Shared.Transfer;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace backend.Hubs;

//todo: add disconnect logic

public class GameHub : Hub
{
    private class HitObject
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public bool IsHit { get; set; }
    }

    public async Task JoinGame()
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        var playerName = gameFacade.EmptyGameExist() ? "Player2" : "Player1"; // Player1 if hes the first to join the game
        var player = new Player { Id = Context.ConnectionId, Name = playerName };
        
        ProxyPlayerImage proxyPlayerImage = new ProxyPlayerImage(player);
        DisplayPlayerImage(proxyPlayerImage);
        await Clients.Client(player.Id).SendAsync("SetIcon", player.Icon);

        Game game = new Game();

        ThemeImplementor themeImplementor = new ConcreteImplementorDark();
        ThemeAbstraction themeAbstraction = new DarkTheme(themeImplementor);
        player.OwnBoard.SetTheme(themeAbstraction);
        Color background = themeAbstraction.Background();
        string text = themeAbstraction.Text();
        Color textColor = themeAbstraction.TextColor();
        Color buttonBackgroundColor = themeAbstraction.ButtonBackgroundColor();
        await Clients.Client(player.Id).SendAsync("SetTheme", background, text, textColor, buttonBackgroundColor);

        if (!gameFacade.EmptyGameExist()) // if no empty games
        {
            game.Player1 = player;
            var groupId = Guid.NewGuid().ToString();
            game.Group.Id = groupId;
            gameFacade.AddGame(game);
            await Clients.Client(player.Id).SendAsync("WaitingForOpponent", player.Name);

            return;
        }

        game = gameFacade.GetFirstEmptyGame(); //get the first game where player is waiting for an opponent
        game.Player2 = player;
        game.WaitingForOpponent = false;

        await Groups.AddToGroupAsync(game.Player1.Id, game.Group.Id);
        await Groups.AddToGroupAsync(game.Player2.Id, game.Group.Id);
        await Clients.Client(player.Id).SendAsync("WaitingForOpponent", player.Name);
        await SetupShips(game);
    }

    public void DisplayPlayerImage(IGameAsset playerImage)
    {
        // Display player image using the common interface
        playerImage.GetImage();
    }

    public async Task GenerateRandomShips()
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        await Clients.Client(gameFacade.GetCurrentPlayer().Id).SendAsync("RandomShipsResponse", gameFacade.SetupPlayerRandomShips());
    }

    public async Task SetTheme()
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        ThemeAbstraction themeAbstraction = gameFacade.SetupTheme();
        Color background = themeAbstraction.Background();
        string text = themeAbstraction.Text();
        Color textColor = themeAbstraction.TextColor();
        Color buttonBackgroundColor = themeAbstraction.ButtonBackgroundColor();
        await Clients.Client(gameFacade.GetCurrentPlayer().Id).SendAsync("SetTheme", background, text, textColor, buttonBackgroundColor);
    }

    public async Task ClientGlobalMessage(string message)
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        var currentPlayer = gameFacade.GetCurrentPlayer();
        var enemyPlayer = gameFacade.GetEnemyPlayer();

        if (currentPlayer != null)
        {
            await Clients.Client(currentPlayer.Id).SendAsync("GlobalMessage", string.Format("{0}: {1}", currentPlayer.Name, message));
        }

        if (enemyPlayer != null)
        {
            await Clients.Client(enemyPlayer.Id).SendAsync("GlobalMessage", string.Format("{0}: {1}", currentPlayer.Name, message));
        }
    }

    private async Task SetupShips(Game game)
    {

        Subject s = new Subject(Clients);
        s.AddObserver(new Observer.Observer(game.Player1.Id));
        s.AddObserver(new Observer.Observer(game.Player2.Id));
        s.SubjectState = "SetupShips";
        s.SubjectMessage = "";

        await s.NotifyObservers();

        await Clients.Client(game.Player1.Id).SendAsync("SetCoins", game.Player1.Coins);
        await Clients.Client(game.Player2.Id).SendAsync("SetCoins", game.Player2.Coins);

        //send to frontend for players to setup ships
        //await Clients.Group(gameId).SendAsync("SetupShips", "");
    }
    // _connection.SendAsync("SetShip", size, x, y, vertical);
    public async Task AddShipToPlayer(int x, int y, ShipType shipType, bool isVertical, int price)
    {

        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        var currentPlayer = gameFacade.GetCurrentPlayer();
        if (currentPlayer.Coins >= price)
        {
            var addedShip = gameFacade.AddShipToPlayer(shipType, isVertical, x, y);
            currentPlayer.Coins -= price;

            await Clients.Client(currentPlayer.Id).SendAsync("SetCoins", currentPlayer.Coins);
            await Clients.Client(currentPlayer.Id).SendAsync("SetupShipResponse", new SetupShipResponse(true, addedShip.GetCoordinates(), shipType, addedShip.ID));
        }
        else
        {
            await Clients.Client(currentPlayer.Id).SendAsync("SetupShipResponse", new SetupShipResponse(false, new List<ShipCoordinate>(), ShipType.SmallShip, -1));
        }
    }
    
    public async Task ClientMessage(string message)
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        var currentPlayer = gameFacade.GetCurrentPlayer();

        Console.WriteLine(message);

        await Clients.Client(currentPlayer.Id).SendAsync("ServerDebuggerMessageResponse", message + " (succeeded)");
    }

    public async Task FlagShip(int x, int y)
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        var currentPlayer = gameFacade.GetCurrentPlayer();
        var currentGame = gameFacade.GetCurrentGame();

        if (currentPlayer == null || currentGame == null)
        {
            return;
        }
        var enemyPlayer = gameFacade.GetEnemyPlayer();

        List<Ship> ships = gameFacade.GetShips();
        int index = 0;
        Ship? existingShip = getShipByCoordinate(ships, x, y, index);

        if (existingShip == null)
        {
            return;
        }

        existingShip = new FlagDecorator(existingShip);
        await Clients.Client(currentPlayer.Id).SendAsync("AddFlags", existingShip.GetCoordinates());
        await Clients.Client(enemyPlayer.Id).SendAsync("AddEnemyFlags", existingShip.GetCoordinates());
    }

    public async Task SetShipYellowBackground(int x, int y)
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        var currentPlayer = gameFacade.GetCurrentPlayer();
        var currentGame = gameFacade.GetCurrentGame();

        if (currentPlayer == null || currentGame == null)
        {
            return;
        }

        List<Ship> ships = gameFacade.GetShips();
        int index = 0;
        Ship? existingShip = getShipByCoordinate(ships, x, y, index);

        if (existingShip == null)
        {
            return;
        }

        existingShip = new ColoredDecorator(existingShip, Color.Yellow);

        await Clients.Client(currentPlayer.Id).SendAsync("RerenderCoordinates", existingShip.GetCoordinates());
    }

    public async Task SetShipBlueBorder(int x, int y)
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        var currentPlayer = gameFacade.GetCurrentPlayer();
        var currentGame = gameFacade.GetCurrentGame();

        if (currentPlayer == null || currentGame == null)
        {
            return;
        }

        List<Ship> ships = gameFacade.GetShips();
        int index = 0;
        Ship? existingShip = getShipByCoordinate(ships, x, y, index);

        if (existingShip == null)
        {
            return;
        }

        existingShip = new BorderDecorator(existingShip, Color.Blue);

        await Clients.Client(currentPlayer.Id).SendAsync("RerenderCoordinates", existingShip.GetCoordinates());
    }

    private Ship? getShipByCoordinate(List<Ship> ships, int x, int y, int index = 0) 
    {
        Ship? existingShip = null;
        foreach (Ship ship in ships)
        {
            foreach (ShipCoordinate coordinate in ship.GetCoordinates())
            {
                if (coordinate.X == x && coordinate.Y == y)
                {
                    existingShip = ship;
                    break;
                }
            }

            if (existingShip != null)
            {
                break;
            }

            index++;
        }

        return existingShip?.DeepCopy();
    }


    public async Task DoneShipSetup()
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        var currentPlayer = gameFacade.GetCurrentPlayer();
        var currentGame = gameFacade.GetCurrentGame();

        currentPlayer.HasSetupShips = true;

        if (currentGame.HavePlayersSetupShips())
        {
            await StartGame(currentGame);
        }
    }

    public async Task RestartGame()
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        var currentPlayer = gameFacade.GetCurrentPlayer();
        var enemyPlayer = gameFacade.GetEnemyPlayer();

        var currentGame = gameFacade.GetCurrentGame();

        currentGame.RestoreFromRestorePoint();

        var initialPlayerShips = currentPlayer.OwnBoard.GetShips();
        var initialEnemyPlayerShips = enemyPlayer.OwnBoard.GetShips();

        await Clients.Client(currentPlayer.Id).SendAsync("GameReset", true);
        await Clients.Client(enemyPlayer.Id).SendAsync("GameReset", false);

        foreach(var ship in initialPlayerShips)
        {
            await Clients.Client(currentPlayer.Id).SendAsync("ResetGameShip", new RestartGame {  ShipType = ship.ShipType, PlacedX = ship.PlacedX, PlacedY = ship.PlacedY, Coordinates = ship.GetCoordinates(), ID = ship.ID});
        }

        foreach (var ship in initialEnemyPlayerShips)
        {
            await Clients.Client(enemyPlayer.Id).SendAsync("ResetGameShip", new RestartGame { ShipType = ship.ShipType, PlacedX = ship.PlacedX, PlacedY = ship.PlacedY, Coordinates = ship.GetCoordinates(), ID = ship.ID });
        }

        await ShipsStats();
        await ShipsStatsEnemy();
    }

    private async Task StartGame(Game _currentGame)
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);

        var currentGame = gameFacade.GetCurrentGame();

        currentGame.CreateRestorePoint();

        await Clients.Group(_currentGame.Group.Id).SendAsync("GameStarted", "GameStarted");
        await Clients.Client(_currentGame.Player1.Id).SendAsync("SetMoves", _currentGame.Player1.Moves);
        await Clients.Client(_currentGame.Player2.Id).SendAsync("SetMoves", _currentGame.Player2.Moves);

        await Task.Delay(TimeSpan.FromSeconds(1)); // wait a second

        await Clients.Client(_currentGame.Player1.Id).SendAsync("YourTurn", "YourTurn"); // Player1 starts the game
    }


    //public async Task MakeMove(int x, int y, ShipType shipType, bool isVertical)
    public async Task MakeMove(MakeMove move)
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        var currentPlayer = gameFacade.GetCurrentPlayer();
        var enemyPlayer = gameFacade.GetEnemyPlayer();
        int test = currentPlayer.OwnBoard.GetShips()[move.ID].ID;
        currentPlayer.OwnBoard.GetShips()[move.ID].ShootsLeft--;
        // Create a new instance of ShotCommand and execute it
        var shotCommand = new ShotCommand(currentPlayer, enemyPlayer, move.X, move.Y, move.TypeOfShip,
            move.IsVertical, move.AttackBomb, GameManager.Instance, Clients.Client(currentPlayer.Id),
            Clients.Client(enemyPlayer.Id), enemyPlayer.OwnBoard);
        currentPlayer.Moves -= move.AttackBomb == BombType.MissileBomb ? 1 : 2;
        await Clients.Client(currentPlayer.Id).SendAsync("SetMoves", currentPlayer.Moves);
        await Clients.Client(currentPlayer.Id).SendAsync("SetLeftShoots", currentPlayer.OwnBoard.GetShips()[move.ID].ID, currentPlayer.OwnBoard.GetShips()[move.ID].ShootsLeft);

        await shotCommand.Execute();
        await ShipsStats();
        await ShipsStatsEnemy();

        bool left_ships = true;
        foreach(var ships in currentPlayer.OwnBoard.GetShips())
        {
            if(ships.ShootsLeft > 0)
            {
                left_ships = false;
                break;
            }
        }
        if(left_ships)
        {
            await Clients.Client(currentPlayer.Id).SendAsync("GameOver", true);
            await Clients.Client(enemyPlayer.Id).SendAsync("GameOver", false);
        }

    }

    public async Task UndoMove(MakeMove move)

    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);
        var currentPlayer = gameFacade.GetCurrentPlayer();
        var enemyPlayer = gameFacade.GetEnemyPlayer();

        // Create a new instance of ShotCommand and execute it
        var shotCommand = new ShotCommand(currentPlayer, enemyPlayer, move.X, move.Y, move.TypeOfShip,
            move.IsVertical, move.AttackBomb, GameManager.Instance, Clients.Client(currentPlayer.Id),
            Clients.Client(enemyPlayer.Id), enemyPlayer.OwnBoard);

        await shotCommand.Undo();

    }

    public async Task GiveMoveToPlayer()
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);

        var enemyPlayer = gameFacade.GetEnemyPlayer();

        await Clients.Client(enemyPlayer.Id).SendAsync("YourTurn", "YourTurn");
    }

    public async Task ShipsStats()
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);

        var currentPlayer = gameFacade.GetCurrentPlayer();
        var playerShips = currentPlayer.OwnBoard.GetShips();

        var shipStats = playerShips.Select(s => new ShipStats {ShipType = s.ShipType, Stats = new Statistics(s.Stats.HealthCount) });

        await Clients.Client(currentPlayer.Id).SendAsync("ShipsStats", shipStats);
    }

    public async Task ShipsStatsEnemy()
    {
        GameFacade gameFacade = new GameFacade(Context.ConnectionId);

        var enemyPlayer = gameFacade.GetEnemyPlayer();
        var playerShips = enemyPlayer.OwnBoard.GetShips();

        var shipStats = playerShips.Select(s => new ShipStats { ShipType = s.ShipType, Stats = new Statistics(s.Stats.HealthCount) });

        await Clients.Client(enemyPlayer.Id).SendAsync("ShipsStats", shipStats);
    }

    // public override Task OnDisconnectedAsync(Exception exception)
    // {
    //     var _currentPlayer = _gameManager.GetPlayer(Context.ConnectionId);
    //     var _currentGame = _gameManager.GetPlayerGame(Context.ConnectionId);
    //     
    //     _currentGame.RemovePlayerFromGame(_currentPlayer);
    //     Clients.Group(_currentGame.Group.Id).SendAsync("GameOverPlayerLeft");
    //
    //     return base.OnDisconnectedAsync(exception);
    // }
}
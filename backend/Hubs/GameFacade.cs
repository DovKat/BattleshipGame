using backend.Manager;
using backend.Models.Entity.Ships.Factory;
using backend.Models.Entity.Ships;
using Shared.Transfer;
using backend.Service;
using backend.Models.Entity;
using Shared;
using backend.Models.Entity.Ships.Generator;
using backend.Models.Entity.GameBoardExtensions;
using System.Numerics;

namespace backend.Hubs
{
    public class GameFacade
    {
        private GameManager _gameManager;
        private GameService _gameService;
        private ShipFactory _shipFactory;
        private ShipGenerator _shipGenerator;
        private string CurrentPlayerConnectionId;

        public GameFacade(string connectionId)
        {
            CurrentPlayerConnectionId = connectionId;
            _shipFactory = new ConcreteShipFactory();
            _gameManager = GameManager.Instance;
            _gameService = new GameService();
            _shipGenerator = new ShipGenerator();
        }

        public Player GetCurrentPlayer()
        {
            return _gameManager.GetPlayer(CurrentPlayerConnectionId);
        }
        public Game GetCurrentGame()
        {
            return _gameManager.GetPlayerGame(CurrentPlayerConnectionId);
        }
        public Player GetEnemyPlayer()
        {
            var currentPlayer = GetCurrentPlayer();
            var currentGame = GetCurrentGame();

            return currentGame.GetEnemyPlayer(currentPlayer);
        }

        //public List<ShipCoordinate> MakeMove(MakeMove move)
        //{
        //    Ship ship = _shipFactory.GetShip(move.TypeOfShip);
        //    if (move.IsVertical)
        //    {
        //        ship.SetVertical();
        //    }

        //    var enemyPlayer = GetEnemyPlayer();
        //    var enemyBoard = enemyPlayer.OwnBoard;

        //    enemyBoard.SetEnemyAttackShip(ship);
        //    enemyBoard.ClearMissedCoordinates();//clear missed coordinates so they dont duplicate

        //    GameBoardService gameBoardService = new GameBoardService(enemyBoard);

        //    return gameBoardService.GetHitCoordinates(move.X, move.Y, move.AttackBomb);
        //}

        public bool HasGameEnded()
        {
            var enemyPlayer = GetEnemyPlayer();
            var enemyBoard = enemyPlayer.OwnBoard;

            return enemyBoard.HaveAllShipsSunk;
        }
        public List<Ship> GetShips()
        {
            return GetCurrentPlayer().OwnBoard.GetShips();
        }

        public Ship AddShipToPlayer(ShipType shipType, bool isVerticalShip, int x, int y)
        {
            Ship ship = _shipFactory.GetShip(shipType);

            if (isVerticalShip)
            {
                ship.SetVertical();
            }

            _gameService.CalculateShipCoordinates(ship, x, y);

            var currentPlayer = GetCurrentPlayer();

            ship.PlacedX = x;
            ship.PlacedY = y;

            currentPlayer.OwnBoard.AddShip(ship);

            return ship;
        }
        public List<ShipCoordinate> GetMissedCoordinates()
        {

            var enemyPlayer = GetEnemyPlayer();
            var enemyBoard = enemyPlayer.OwnBoard;

            return enemyBoard.GetMissedCoordinates();
        }
        public List<ShipCoordinate> GetEnemyShipCoordinates()
        {
            var enemyPlayer = GetEnemyPlayer();
            var enemyBoard = enemyPlayer.OwnBoard;
            var enemyShips = enemyBoard.GetShips();

            List<ShipCoordinate> shipCoordinates = new List<ShipCoordinate>();
            foreach (var ship in enemyShips)
            {
                foreach (var shipCoordinate in ship.GetCoordinates())
                {
                    shipCoordinates.Add(shipCoordinate);
                }
            }

            return shipCoordinates;
        }
        public Game GetFirstEmptyGame()
        {
            return _gameManager.Games.First(x => x.WaitingForOpponent == true);
        }
        public void AddGame(Game game)
        {
            _gameManager.Games.Add(game);
        }
        public bool EmptyGameExist()
        {
            return _gameManager.EmptyGameExist();
        }
        public List<SetupShipResponse> SetupPlayerRandomShips()
        {
            var currentPlayer = GetCurrentPlayer();
            var randomShips = _shipGenerator.GenerateRandomShips();
            List<SetupShipResponse> randomShipsTest = new List<SetupShipResponse>();
            foreach (var randomShip in randomShips)
            {
                randomShipsTest.Add(new SetupShipResponse(true, randomShip.GetCoordinates(), randomShip.ShipType, randomShip.ID));
                currentPlayer.OwnBoard.AddShip(randomShip);
            }

            return randomShipsTest;
        }

        public ThemeAbstraction SetupTheme()
        {
            var currentPlayer = GetCurrentPlayer();

            if (currentPlayer.OwnBoard.GetTheme() is LightTheme)
            {
                currentPlayer.OwnBoard.SetTheme(new DarkTheme(new ConcreteImplementorDark()));
            }
            else
            {
                currentPlayer.OwnBoard.SetTheme(new LightTheme(new ConcreteImplementorLight()));
            }
            return currentPlayer.OwnBoard.GetTheme();
        }
    }
}

using backend.Manager;
using backend.Models.Entity.Ships.Factory;
using backend.Models.Entity.Ships;
using backend.Models.Entity;
using Shared.Transfer;
using Shared;
using Microsoft.AspNetCore.SignalR;
using backend.Service;
using backend.Strategies.Attacks;
using backend.Visitor;

namespace backend.Command
{
    public class ShotCommand : ICommand
    {
        private readonly Player attacker;
        private readonly Player opponent;
        private readonly int x;
        private readonly int y;
        private readonly ShipType shipType;
        private readonly bool isVertical;
        private readonly BombType attackBomb;
        private readonly GameManager gameManager;
        private readonly IClientProxy attackerClient;
        private readonly IClientProxy opponentClient;
        private readonly GameBoard enemyBoard;

        public ShotCommand(Player attacker, Player opponent, int x, int y, ShipType shipType, bool isVertical, BombType attackBomb, GameManager gameManager, IClientProxy attackerClient, IClientProxy opponentClient, GameBoard enemyBoard)
        {
            this.attacker = attacker;
            this.opponent = opponent;
            this.x = x;
            this.y = y;
            this.shipType = shipType;
            this.isVertical = isVertical;
            this.attackBomb = attackBomb;
            this.gameManager = gameManager;
            this.attackerClient = attackerClient;
            this.opponentClient = opponentClient;
            this.enemyBoard = enemyBoard;
        }

        public async Task Execute()
        {
            ShipFactory shipFactory = new ConcreteShipFactory();
            Ship ship = shipFactory.GetShip(shipType);
            if (isVertical)
            {
                ship.SetVertical();
            }

            //var enemyBoard = gameManager.GetPlayerBoard(opponentClient.ConnectionId);
            enemyBoard.SetEnemyAttackShip(ship);
            enemyBoard.ClearMissedCoordinates();

            AttackTemplate attack = ship.GetAttackTemplate();

            
            List<ShipCoordinate> hitShipCoordinates = attack.Attack(attackBomb, x, y, enemyBoard);
            List<ShipCoordinate> missedCoordinates = enemyBoard.GetMissedCoordinates();
            List<Ship> sunkShips = new List<Ship>();

            foreach(var hitCoord in hitShipCoordinates)
            {
                if(enemyBoard.GetHitShip(hitCoord) != null)
                {
                    if (enemyBoard.GetHitShip(hitCoord).IsSunk() && !sunkShips.Contains(enemyBoard.GetHitShip(hitCoord)))
                    {
                        sunkShips.Add(enemyBoard.GetHitShip(hitCoord));
                    }
                }
            }

            int total = 0;
            ShipInspector inspector = new ShipInspection();
            foreach (var hitShip in sunkShips)
            {
                total = total + hitShip.Accept(inspector);
            }
            this.attacker.Score += total;

            foreach (var hitCoord in hitShipCoordinates)
            {
                await attackerClient.SendAsync("ReturnMove", new MoveResult(hitCoord.X, hitCoord.Y, true));
                await opponentClient.SendAsync("OpponentResult", new MoveResult(hitCoord.X, hitCoord.Y, true));
            }

            foreach (var missCoord in missedCoordinates)
            {
                await attackerClient.SendAsync("ReturnMove", new MoveResult(missCoord.X, missCoord.Y, false));
                await opponentClient.SendAsync("OpponentResult", new MoveResult(missCoord.X, missCoord.Y, false));
            }

            if (enemyBoard.HaveAllShipsSunk)
            {
                await attackerClient.SendAsync("GameOver", true);
                await opponentClient.SendAsync("GameOver", false);
            }

            if (attacker.Moves == 0)
            {
                await attackerClient.SendAsync("GameOver", false);
                await opponentClient.SendAsync("GameOver", true);
            }

            if (hitShipCoordinates.Any())
            {
                await attackerClient.SendAsync("YourTurn", "YourTurn");
            }
            else
            {
                await attackerClient.SendAsync("UndoTurn", new MakeMove(x, y, ship.ShipType, isVertical, attackBomb, ship.ID));
            }
        }

        public async Task Undo()
        {
            ShipFactory shipFactory = new ConcreteShipFactory();
            Ship ship = shipFactory.GetShip(shipType);
            if (isVertical)
            {
                ship.SetVertical();
            }

            //var enemyBoard = gameManager.GetPlayerBoard(opponentClient.ConnectionId);
            enemyBoard.SetEnemyAttackShip(ship);

            //List<ShipCoordinate> hitShipCoordinates = enemyBoard.GetHitCoordinates(x, y, attackBomb);
            List<ShipCoordinate> missedCoordinates = enemyBoard.GetMissedCoordinates();

            //foreach (var hitCoord in hitShipCoordinates)
            //{
            //    await attackerClient.SendAsync("UndoReturnMove", new MoveResult(hitCoord.X, hitCoord.Y, true));
            //    await opponentClient.SendAsync("UndoOpponentResult", new MoveResult(hitCoord.X, hitCoord.Y, true));
            //}

            foreach (var missCoord in missedCoordinates)
            {
                await attackerClient.SendAsync("UndoReturnMove", new MoveResult(missCoord.X, missCoord.Y, false));
                await opponentClient.SendAsync("UndoOpponentResult", new MoveResult(missCoord.X, missCoord.Y, false));
            }
        }
    }
}

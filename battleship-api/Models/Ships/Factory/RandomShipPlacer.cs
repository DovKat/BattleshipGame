using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using backend.ShipFactory;
using battleship_api.Command;
using Newtonsoft.Json.Bson;

public class RandomShipPlacer
{
    private readonly ShipFactory _shipFactory;
    private readonly int _boardSize;
    private readonly Random _random;

    public RandomShipPlacer(ShipFactory shipFactory, int boardSize = 10)
    {
        _shipFactory = shipFactory;
        _boardSize = boardSize;
        _random = new Random();
    }

    public void FillBoardWithRandomShips(Board board, CommandManager manager)
    {
        var shipTypes = new List<string> { "Destroyer", "Submarine", "Cruiser", "Battleship", "Carrier" };
        var shipGroup = new ShipGroup();

        foreach (var shipType in shipTypes)
        {
            Ship ship;
            bool placed = false;

            while (!placed)
            {
                ship = _shipFactory.CreateShip(shipType);

                string orientation = _random.Next(2) == 0 ? "horizontal" : "vertical";
                int row = _random.Next(_boardSize);
                int col = _random.Next(_boardSize);

                var shipCoordinates = CalculateShipCoordinates(new Coordinate { Row = row, Column = col }, ship.Length, orientation);

                if (IsPlacementValid(board, shipCoordinates))
                {
                    ship.Coordinates = shipCoordinates;
                    ship.Orientation = orientation;
                    shipGroup.Add(ship); // Add ship to the group
                    placed = true;
                }
            }
        }

        // Place all ships in the group
        shipGroup.Place(board, manager);
    }


    private void clearBoard(Board board, CommandManager manager)
    {
        foreach (var ship in board.Ships)
        {
            manager.UndoLastCommand();
        }
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

    private bool IsPlacementValid(Board board, List<Coordinate> coordinates)
    {
        foreach (var coord in coordinates)
        {
            // Check if coordinates are out of bounds
            if (coord.Row < 0 || coord.Row >= _boardSize || coord.Column < 0 || coord.Column >= _boardSize)
            {
                return false;
            }

            // Check if the cell already has a ship
            if (board.Grid[coord.Row][coord.Column].HasShip)
            {
                return false;
            }
        }
        return true;
    }

    private void PlaceShipOnBoard(Board board, Ship ship)
    {
        board.Ships.Add(ship); // Add ship to board's list of ships
        foreach (var coord in ship.Coordinates)
        {
            board.Grid[coord.Row][coord.Column].HasShip = true;
        }
    }
}

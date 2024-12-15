using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using battleship_api.Command;

public abstract class Ship : IPlaceable
{
    public string Name { get; set; }
    public int Length { get; set; }
    public List<Coordinate> Coordinates { get; set; } = new List<Coordinate>();
    public string Orientation { get; set; }
    public bool isPlaced { get; set; }
    public int HitCount { get; set; }
    public bool IsSunk { get; set; }

    public abstract Ship ShallowCopy();
    public abstract Ship DeepCopy();

    public void Place(Board board, CommandManager manager)
    {
        var placeCommand = new PlaceShipCommand(board, this, Coordinates);
        manager.ExecuteCommand(placeCommand);
    }

    public void IncrementHitCount()
    {
        this.HitCount++;
        if(HitCount == Length)
        {
            IsSunk = true;
        }
    }
}
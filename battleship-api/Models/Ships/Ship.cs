using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public abstract class Ship
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
}

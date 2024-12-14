public class TurnResult
{
    public string TargetPlayerId { get; set; } 
    public List<Coordinate> AffectedCoordinates { get; set; } 
    public List<string> Results { get; set; } 
}
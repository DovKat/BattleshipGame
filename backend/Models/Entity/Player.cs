namespace backend.Models.Entity;

public class Player
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Score { get; set; } = 0;
    public bool HasTurn { get; set; } = false;
    
    public GameBoard OwnBoard { get; } = new();
    public bool HasSetupShips { get; set; } = false;
    public bool IsInTestMode { get; set; } = false;
    public string? Icon { get; set; }
    public int Coins { get; set; } = 8;
    public int Moves { get; set; } = 10;
}
public class Game
{
    public string GameId { get; set; }
    public List<Team> Teams { get; set; }
    public Dictionary<string, Player> Players { get; set; }
    public string State { get; set; } = "Waiting";
    public string CurrentTurn { get; set; } // Track which team is playing
    public int CurrentPlayerIndex { get; set; } // Track the player index within the team

    public Game()
    {
        Teams = new List<Team>();
        Players = new Dictionary<string, Player>();
    }
}

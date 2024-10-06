using backend.Models.Entity;
using backend.Models.Entity.Iterator;

namespace backend.Manager;

public class GameManager
{
    private static GameManager _instance;
    private readonly GameCollection _games = new();
    private static readonly object threadLock = new object();
    private GameManager()
    {
    }

    public static GameManager Instance
    {
        get
        {
            lock (threadLock)
            {
                if (_instance == null)
                {
                    _instance = new GameManager();
                }

                return _instance;
            }
        }
    }

    public List<Game> Games => _games.getItems();

    public bool EmptyGameExist()
    {
        return _games.getItems().Any(x => x.WaitingForOpponent == true);
    }

    public Player? GetPlayer(string playerId)
    {
        return _games.getItems().Select(game => game.GetPlayerById(playerId)).FirstOrDefault(currentPlayer => currentPlayer != null);
    }

    public Game? GetPlayerGame(string playerId)
    {
        foreach (var game in _games.getItems())
        {
            if (game.GetPlayerById(playerId) != null)
            {
                return game;
            }
        }

        return null;

    }

    // Add methods to manage games, players, and game state
}
namespace battleship_api.Builder;

public class StandardGameBuilder : IGameBuilder
{
    private Game _game = new Game();
    public void SetGameId(string gameId)
    {
        _game.GameId = gameId;
    }

    public void SetTeams(List<Team> teams)
    {
        _game.Teams = teams;
    }

    public void SetState(string state)
    {
        _game.State = state;
    }
    public void SetMode()
    {
        _game.GameMode = "Standart";
    }
    public Game Build()
    {
        return _game;
    }
}

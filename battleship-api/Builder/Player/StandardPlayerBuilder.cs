namespace battleship_api.Builder;

public class StandardPlayerBuilder : IPlayerBuilder
{
    private Player _player = new Player();

    public void SetPlayerId(string playerId)
    {
        _player.Id = playerId;
    }

    public void SetPlayerName(string playerName)
    {
        _player.Name = playerName;
    }

    public void SetTeam(string team)
    {
        _player.Team = team;
    }

    public void SetBoard(Board board)
    {
        _player.Board = board;
    }

    public Player Build()
    {
        return _player;
    }
}

// Another concrete builder for AI players
public class AIPlayerBuilder : IPlayerBuilder
{
    private Player _player = new Player();

    public void SetPlayerId(string playerId)
    {
        _player.Id = playerId;
    }

    public void SetPlayerName(string playerName)
    {
        _player.Name = playerName + " (AI)";
    }

    public void SetTeam(string team)
    {
        _player.Team = team;
    }

    public void SetBoard(Board board)
    {
        _player.Board = board;
    }

    public Player Build()
    {
        return _player;
    }
}

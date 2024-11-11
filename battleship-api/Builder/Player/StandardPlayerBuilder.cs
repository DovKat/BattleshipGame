namespace battleship_api.Builder;

public class StandardPlayerBuilder : IPlayerBuilder
{
    private string _playerId;
    private string _playerName;
    private string _team;
    private Board _board;

    public void SetPlayerId(string playerId) => _playerId = playerId;
    public void SetPlayerName(string playerName) => _playerName = playerName;
    public void SetTeam(string team) => _team = team;
    public void SetBoard(Board board) => _board = board;

    public Player Build()
    {
        return new Player
        {
            Id = _playerId,
            Name = _playerName,
            Team = _team,
            Board = _board,
            IsReady = false
        };
    }
}
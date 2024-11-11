namespace battleship_api.Builder;

public interface IPlayerBuilder
{
    void SetPlayerId(string playerId);
    void SetPlayerName(string playerName);
    void SetTeam(string team);
    void SetBoard(Board board);
    Player Build();
}
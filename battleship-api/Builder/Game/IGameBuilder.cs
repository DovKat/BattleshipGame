namespace battleship_api.Builder;

public interface IGameBuilder
{
    void SetGameId(string gameId);
    void SetTeams(List<Team> teams);
    void SetState(string state);
    public void SetMode();
    Game Build();
}
namespace battleship_api.Adapter
{
    // Target Interface
    public interface IPlayerStatsService
    {
        void UpdateStats(string playerID, int score, int hits, int misses);
    }

}

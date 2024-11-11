namespace battleship_api.Adapter
{
    // Adapter that implements the target interface and uses an instance of the Adaptee
    public class StatsTrackerAdapter : IPlayerStatsService
    {
        private readonly ExternalStatsTracker _externalStatsTracker;

        public StatsTrackerAdapter(ExternalStatsTracker externalStatsTracker)
        {
            _externalStatsTracker = externalStatsTracker;
        }

        public void UpdateStats(string playerID, int score, int hits, int misses)
        {
            // Adapts multiple methods of ExternalStatsTracker to fit into the single method of IPlayerStatsService
            _externalStatsTracker.AddScore(playerID, score);
            _externalStatsTracker.AddHits(playerID, hits);
            _externalStatsTracker.AddMisses(playerID, misses);
        }
    }
}


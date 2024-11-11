namespace battleship_api.Adapter
{
    // Adaptee class with a different interface
    public class ExternalStatsTracker
    {
        public void AddScore(string playerID, int score)
        {
            Console.WriteLine($"[External] Adding score {score} for {playerID}");
        }

        public void AddHits(string playerID, int hits)
        {
            Console.WriteLine($"[External] Adding hits {hits} for {playerID}");
        }

        public void AddMisses(string playerID, int misses)
        {
            Console.WriteLine($"[External] Adding misses {misses} for {playerID}");
        }
    }

}

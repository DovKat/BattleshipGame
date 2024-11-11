using battleship_api.Strategy;

namespace battleship_api.Bridge
{
    public class TournamentScoringSystem : IScoringSystem
    {
        private readonly int _timeBonus;

        public TournamentScoringSystem(int timeBonus)
        {
            _timeBonus = timeBonus;
        }

        public int CalculateScore(string hitResult)
        {
            int basePoints = hitResult switch
            {
                "Hit" => 25,
                "Sunk" => 200,
                _ => 0
            };

            return basePoints + _timeBonus;  // Add time-based bonus if desired
        }
    }

}

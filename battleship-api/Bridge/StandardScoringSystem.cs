using battleship_api.Strategy;

namespace battleship_api.Bridge
{
    public class StandardScoringSystem : IScoringSystem
    {
        public int CalculateScore(string hitResult)
        {
            return hitResult switch
            {
                "Hit" => 50,
                "Sunk" => 100,
                _ => 0
            };
        }
    }

}

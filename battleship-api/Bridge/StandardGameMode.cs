using battleship_api.Strategy;
using Microsoft.AspNetCore.Http;
namespace battleship_api.Bridge
{
    public class StandardGameMode : GameMode
    {
        public StandardGameMode(IScoringSystem scoringSystem) : base(scoringSystem) { }

        public override void PlayTurn(int timeTakenInSeconds, string attackType, string hitResult)
        {
            int score = scoringSystem.CalculateScore(hitResult);
            Console.WriteLine($"Classic Mode - Current Score: {score}");
        }
    }
}

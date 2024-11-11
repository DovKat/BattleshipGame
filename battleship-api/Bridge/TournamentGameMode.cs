using battleship_api.Strategy;
using Microsoft.AspNetCore.Http;
namespace battleship_api.Bridge
{
    public class TournamentGameMode : GameMode
    {
        public TournamentGameMode(IScoringSystem scoringSystem) : base(scoringSystem) { }

        public override void PlayTurn(int timeTakenInSeconds, string attackType, string hitResult)
        {
            int score = scoringSystem.CalculateScore(hitResult);
            Console.WriteLine($"Timed Mode - Current Score: {score}");
        }
    }
}

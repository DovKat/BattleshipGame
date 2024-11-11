using battleship_api.Strategy;

namespace battleship_api.Bridge
{
    public abstract class GameMode
    {
        protected IScoringSystem scoringSystem;

        protected GameMode(IScoringSystem scoringSystem)
        {
            this.scoringSystem = scoringSystem;
        }

        public abstract void PlayTurn(int timeTakenInSeconds, string attackType, string hitResult);
    }

}

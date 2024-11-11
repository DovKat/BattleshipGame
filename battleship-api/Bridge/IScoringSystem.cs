using battleship_api.Strategy;

namespace battleship_api.Bridge
{
    // Implementor Interface
    public interface IScoringSystem
    {
        int CalculateScore(string hitResult);
    }


}

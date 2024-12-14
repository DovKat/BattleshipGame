namespace battleship_api.Strategy;
public class AttackStrategyFactory
{
    private readonly Dictionary<string, IAttackStrategy> _flyweights = new();

    public IAttackStrategy GetStrategy(string strategyType)
    {
        if (!_flyweights.ContainsKey(strategyType))
        {
            _flyweights[strategyType] = strategyType switch
            {
                "regular" => new RegularAttack(),
                "smallbomb" => new SmallBombAttack(),
                "bigbomb" => new BigBombAttack(),
                "megabomb" => new MegaBombAttack(),
                _ => throw new ArgumentException($"Invalid strategy type: {strategyType}")
            };
        }

        return _flyweights[strategyType];
    }
}
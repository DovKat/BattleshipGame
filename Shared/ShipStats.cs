namespace Shared
{
    [Serializable]
    public class ShipStats
    {
        public ShipType ShipType { get; set; }
        public Statistics? Stats { get; set; }

        public override string? ToString()
        {
            return "Ship: " + ShipType.ToString() + "Health: " + Stats.HealthCount;
        }
    }
}

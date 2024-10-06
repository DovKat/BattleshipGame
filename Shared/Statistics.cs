namespace Shared
{
    [Serializable]
    public class Statistics
    {
        public double HealthCount { get; set; } = 0;
        //public double ArmourCount { get; set; } = 0;

        public Statistics(double healthCount)
        {
            HealthCount = healthCount;
            //ArmourCount = armourCount;
        }
    }
}

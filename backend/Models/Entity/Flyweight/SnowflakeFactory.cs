namespace backend.Models.Entity.Flyweight
{
    public static class SnowflakeFactory
    {
        private static readonly Dictionary<string, Snowflake> cachedSnowflakes = new Dictionary<string, Snowflake>();
        private static readonly Dictionary<string, SnowflakeType> sharedProperties = new Dictionary<string, SnowflakeType>();

        public static Snowflake GetSnowflake(SnowflakeType type)
        {
            string key = $"{type.Size}_{type.Speed}";

            if (!cachedSnowflakes.TryGetValue(key, out Snowflake snowflake))
            {
                snowflake = new Snowflake();
                snowflake.Type = type;
                cachedSnowflakes[key] = snowflake;
            }

            if (!sharedProperties.ContainsKey(key)) // Store new shared properties if not already cached
            {
                sharedProperties[key] = type; // Store type as shared property
            }

            return snowflake;
        }
    }
}

namespace backend.Models.Entity.Flyweight
{
    public class Snowflake
    {
        public double X { get; set; }
        public double Y { get; set; }

        public SnowflakeType Type { get; set; }
        // Other snowflake properties...

        public void Fall(int fallSpeed) // Modify fall behavior to accept fall speed
        {
            Y += fallSpeed;
        }
    }

    //public class Snowflake
    //{
    //    public double X { get; set; }
    //    public double Y { get; set; }
    //    public int FallSpeed { get; set; }
    //    public int Size { get; set; }
    //    // Other snowflake properties...

    //    public void Fall() // Define fall behavior if needed
    //    {
    //        Y += FallSpeed;
    //    }
    //}
}

public class CommandInterpreter
{
    public bool ParseAttackCommand(string command, out CommandDetails details)
    {
        details = null;

        try
        {
            // Split the command into parts
            var parts = command.Split(" ");
            if (parts.Length < 6)
                return false;

            // Extract required parts
            var attackType = parts[0];
            var rowPart = parts[1];
            var colPart = parts[2];

            // Extract additional information
            var gameId = parts.FirstOrDefault(p => p.StartsWith("gameId="))?.Split('=')[1];
            var currentPlayerId = parts.FirstOrDefault(p => p.StartsWith("currentPlayerId="))?.Split('=')[1];
            var targetId = parts.FirstOrDefault(p => p.StartsWith("targetId="))?.Split('=')[1];

            // Parse row and column
            if (!int.TryParse(rowPart.Substring(1), out var row) || !int.TryParse(colPart.Substring(1), out var col))
                return false;

            // Validate extracted parts
            if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(currentPlayerId) || string.IsNullOrEmpty(targetId))
                return false;

            // Populate the details object
            details = new CommandDetails
            {
                GameId = gameId,
                CurrentPlayerId = currentPlayerId,
                TargetId = targetId,
                Row = row,
                Column = col,
                AttackType = attackType,
            };

            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class CommandDetails
{
    public string GameId { get; set; }
    public string CurrentPlayerId { get; set; }
    public string TargetId { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public string AttackType { get; set; }
}

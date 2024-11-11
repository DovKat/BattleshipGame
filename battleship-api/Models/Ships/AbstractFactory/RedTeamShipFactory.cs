public class RedTeamShipFactory : AbstractFactory
{
    public override Ship CreateDestroyer() => new RedDestroyer();
    public override Ship CreateSubmarine() => new RedSubmarine();
    public override Ship CreateCruiser() => new RedCruiser();
    public override Ship CreateBattleship() => new RedBattleship();
    public override Ship CreateCarrier() => new RedCarrier();
}
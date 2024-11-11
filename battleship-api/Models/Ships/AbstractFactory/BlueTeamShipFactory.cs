public class BlueTeamShipFactory : AbstractFactory
{
    public override Ship CreateDestroyer() => new BlueDestroyer();
    public override Ship CreateSubmarine() => new BlueSubmarine();
    public override Ship CreateCruiser() => new BlueCruiser();
    public override Ship CreateBattleship() => new BlueBattleship();
    public override Ship CreateCarrier() => new BlueCarrier();
}
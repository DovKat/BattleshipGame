public abstract class AbstractFactory
{
    public abstract Ship CreateDestroyer();
    public abstract Ship CreateSubmarine();
    public abstract Ship CreateCruiser();
    public abstract Ship CreateBattleship();
    public abstract Ship CreateCarrier(); 
}
namespace backend.Models.Entity.Ships
{
    public class DebuggerMessages : IMessage
    {
        public void SendMessage(string message)
        {
            string msg = "[DEBUGGER]: " + message;
            System.Diagnostics.Debug.WriteLine(msg);
            Console.WriteLine(msg);
        }
    }
}

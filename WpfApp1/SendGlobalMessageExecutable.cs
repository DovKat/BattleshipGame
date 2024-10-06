using Microsoft.AspNetCore.SignalR.Client;

namespace WpfApp1
{
    public class SendGlobalMessageExecutable : Executable
    {
        private HubConnection _connection;
        private string _message;

        public SendGlobalMessageExecutable(HubConnection connection, string message)
        {
            _connection = connection;
            _message = message;
        }

        public void Execute()
        {
            _connection.SendAsync("ClientGlobalMessage", _message);
        }
    }
}

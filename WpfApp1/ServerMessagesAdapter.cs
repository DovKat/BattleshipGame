using Microsoft.AspNetCore.SignalR.Client;

namespace backend.Models.Entity.Ships
{
    public class ServerMessagesAdapter : IMessage
    {
        HubConnection _connection;
        ServerMessages _serverMessages = new ServerMessages();

        public ServerMessagesAdapter(HubConnection connection) {
            _connection = connection;
        }

        public void SendMessage(string message)
        {
            _serverMessages.SendMessageToServer(_connection, message);
        }
    }
}

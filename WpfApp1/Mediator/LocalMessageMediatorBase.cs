using Microsoft.AspNetCore.SignalR.Client;

namespace WpfApp1.Mediator
{
    public abstract class LocalMessageMediatorBase
    {
        public abstract void SendMessageToServer(HubConnection connection, string message);
        public abstract void SendMessage(string message);
        public abstract void Execute();
    }
}

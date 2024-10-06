using backend.Models.Entity.Ships;
using Microsoft.AspNetCore.SignalR.Client;

namespace WpfApp1.Mediator
{
    public class LocalMessageConcreteMediator : LocalMessageMediatorBase
    {
        ServerMessages _serverMessages;
        DebuggerMessages _debuggerMessages;
        SendLocalMessageExecutable _localMessageExecutable;
        public LocalMessageConcreteMediator(
            ServerMessages serverMessages, 
            DebuggerMessages debuggerMessages, 
            SendLocalMessageExecutable localMessageExecutable
        ) 
        {
            _serverMessages = serverMessages;
            _debuggerMessages = debuggerMessages;
            _localMessageExecutable = localMessageExecutable;
        }

        public override void Execute()
        {
            _localMessageExecutable.Execute();
        }

        public override void SendMessage(string message)
        {
            _debuggerMessages.SendMessage(message);
        }

        public override void SendMessageToServer(HubConnection connection, string message)
        {
            _serverMessages.SendMessageToServer(connection, message);
        }
    }
}

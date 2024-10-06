using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Windows.Interop;

namespace backend.Models.Entity.Ships
{
    public class ServerMessages
    {
        HubConnection _connection;
        public async void SendMessageToServer(HubConnection connection, string message)
        {
            _connection = connection;
            await connection.SendAsync("ClientMessage", message);
            connection.On<string>("ServerDebuggerMessageResponse", ServerDebuggerMessageResponse);
        }

        private void ServerDebuggerMessageResponse(string message)
        {
            string msg = "[SERVER]: " + message;
            System.Diagnostics.Debug.WriteLine(msg);
            Console.WriteLine(msg);
            _connection.Remove("ServerDebuggerMessageResponse");
        }
    }
}

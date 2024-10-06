using backend.Models.Entity;
using Microsoft.AspNetCore.SignalR;

namespace backend.Observer
{
    public class Observer : IObserver
    {
        private string playerId;

        public Observer(string playerId)
        {
            this.playerId = playerId;
        }

        public async Task Update(IHubCallerClients Clients, string msg1, string msg2)
        {
            await Clients.Client(playerId).SendAsync(msg1, msg2);
        }
    }
}

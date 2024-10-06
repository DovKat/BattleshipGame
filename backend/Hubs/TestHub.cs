using backend.Manager;
using backend.Models.Entity.Ships;
using backend.Models.Entity;
using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs
{
    public class TestHub : Hub
    {
        public async Task EnterTestMode(string currentPlayerConnectionId)
        {
            GameFacade gameFacaed = new GameFacade(currentPlayerConnectionId);
            var shipCoordinates = gameFacaed.GetEnemyShipCoordinates();

            await Clients.Client(Context.ConnectionId).SendAsync("ReturnEnterTestMode", shipCoordinates);
        }
    }
}

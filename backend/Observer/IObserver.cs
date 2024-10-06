using Microsoft.AspNetCore.SignalR;

namespace backend.Observer
{
    public interface IObserver
    {
        Task Update(IHubCallerClients Clients, string str1, string str2);
    }
}

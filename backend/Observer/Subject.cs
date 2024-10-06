using Microsoft.AspNetCore.SignalR;

namespace backend.Observer
{
    public class Subject : ASubject
    {
        public string SubjectState = null;
        public string SubjectMessage = null;
        private IHubCallerClients Clients;

        public Subject(IHubCallerClients Clients)
        {
            this.Clients = Clients;
        }

        public async Task NotifyObservers()
        {

            foreach (IObserver o in observersList)
            {
                await o.Update(this.Clients, this.SubjectState, this.SubjectMessage);
            }
        }
    }
}

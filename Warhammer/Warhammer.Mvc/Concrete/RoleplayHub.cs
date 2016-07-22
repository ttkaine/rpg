using Microsoft.AspNet.SignalR;

namespace Warhammer.Mvc.Concrete
{
    public class RoleplayHub : Hub
    {
        public void SessionUpdated()
        {
            Clients.AllExcept(Context.ConnectionId).updateSession();
            Clients.All.updateTextSessionsOnHomePage();
        }
    }
}
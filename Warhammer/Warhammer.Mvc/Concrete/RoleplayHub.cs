using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Warhammer.Mvc.Concrete
{
    public class RoleplayHub : Hub
    {
        public void Update()
        {
            Clients.All.update();
        }
    }
}
using System.Collections.Generic;

namespace Warhammer.Core.Abstract
{
    /// <summary>
    /// The IAuthenticatedUserProvider is declared in the CORE project but is implemented by UI projects which use the CORE.
    /// 
    /// This enables the UI project to provide authentication and session based information back to the CORE 
    /// This means that the CORE can look up the user permissions or make load personalized data
    /// The CORE doesn't need to know anything about the UI front end or how user authentication actually works
    /// 
    /// Do not pass Asp.Net, MVC, or Session classes into the CORE as this requires referencing UI assembiles in the CORE project
    /// </summary>
    public interface IAuthenticatedUserProvider
    {
        bool UserIsAuthenticated { get; }
        string UserName { get; }
        bool IsAdmin { get; } 
    }
}

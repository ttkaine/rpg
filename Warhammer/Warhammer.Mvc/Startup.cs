using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Warhammer.Mvc.Startup))]
namespace Warhammer.Mvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

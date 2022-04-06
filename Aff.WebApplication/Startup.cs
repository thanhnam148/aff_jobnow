using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Aff.WebApplication.Startup))]
namespace Aff.WebApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

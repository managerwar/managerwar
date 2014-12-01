using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebsiteCreatorMVC.Startup))]
namespace WebsiteCreatorMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

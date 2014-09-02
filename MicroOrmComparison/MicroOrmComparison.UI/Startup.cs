using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MicroOrmComparison.UI.Startup))]
namespace MicroOrmComparison.UI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

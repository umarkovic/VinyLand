using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VinyLand.Startup))]
namespace VinyLand
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

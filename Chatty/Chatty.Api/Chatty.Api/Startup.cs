
[assembly: Microsoft.Owin.OwinStartup(typeof(AdMaiora.Chatty.Api.Startup))]

namespace AdMaiora.Chatty.Api
{    
    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}
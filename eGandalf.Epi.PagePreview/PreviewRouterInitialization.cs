using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Web.Routing;
using System.Web.Configuration;
using System.Web.Routing;

namespace eGandalf.Epi.PagePreview
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class PreviewRouterInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var partialRouter = new PreviewPartialRouter();
            RouteTable.Routes.RegisterPartialRouter(partialRouter);
        }

        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
        }
    }
}
using EPiServer;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Web.Routing;
using System.Web.Routing;

namespace eGandalf.Epi.PagePreview
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class PreviewRouterInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var locator = context.Locate.Advanced;
            var partialRouter = new PreviewPartialRouter
            (
                locator.GetInstance<IPagePreview>(),
                locator.GetInstance<IPagePreviewEvents>(),
                locator.GetInstance<IContentLoader>()
            );
            RouteTable.Routes.RegisterPartialRouter(partialRouter);
        }

        void IInitializableModule.Uninitialize(InitializationEngine context) { }
    }
}
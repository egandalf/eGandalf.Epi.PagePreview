using EPiServer.ServiceLocation;
using System.Web;

namespace eGandalf.Epi.PagePreview
{
    [ServiceConfiguration(typeof(IPagePreview), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class DefaultPagePreview : IPagePreview
    {
        public bool IsAllowed()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }
    }
}
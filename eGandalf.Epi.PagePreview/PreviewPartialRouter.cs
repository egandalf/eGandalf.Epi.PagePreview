using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Editor;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using EPiServer.Web.Routing.Segments;
using System.Web.Routing;

namespace eGandalf.Epi.PagePreview
{
    public class PreviewPartialRouter : IPartialRouter<PageData, ContentVersion>
    {
        private Injected<IContentLoader> _contentLoader;
        private Injected<IPagePreview> _pagePreview;

        public PartialRouteData GetPartialVirtualPath(ContentVersion content, string language, RouteValueDictionary routeValues, RequestContext requestContext)
        {
            if (PageEditing.PageIsInEditMode) return null;

            var contentLink = requestContext.GetRouteValue("node", routeValues) as ContentReference;
            return new PartialRouteData
            {
                BasePathRoot = contentLink,
                PartialVirtualPath = $"{content}/"
            };
        }

        public object RoutePartial(PageData content, SegmentContext segmentContext)
        {
            if (!_pagePreview.Service.IsAllowed()) return null;

            var versionSegment = TryGetVersionSegment(segmentContext);
            if (string.IsNullOrEmpty(versionSegment)) return null;

            var workId = new ContentReference(versionSegment);
            if (workId == null || workId.Equals(ContentReference.EmptyReference)) return null;

            return _contentLoader.Service.Get<PageData>(workId);
        }

        private string TryGetVersionSegment(SegmentContext segmentContext)
        {
            var segment = segmentContext.GetNextValue(segmentContext.RemainingPath);
            var versionSegment = segment.Next;
            segmentContext.RemainingPath = segment.Remaining;
            return versionSegment;
        }
    }
}

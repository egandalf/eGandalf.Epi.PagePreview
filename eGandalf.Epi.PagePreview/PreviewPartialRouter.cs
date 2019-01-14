using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Editor;
using EPiServer.Web.Routing;
using EPiServer.Web.Routing.Segments;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace eGandalf.Epi.PagePreview
{
    public class PreviewPartialRouter : IPartialRouter<IContent, ContentVersion>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPagePreview _pagePreview;
        private readonly IPagePreviewEvents _pagePreviewEvents;

        public PreviewPartialRouter(IPagePreview pagePreview, IPagePreviewEvents pagePreviewEvents, IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
            _pagePreview = pagePreview;
            _pagePreviewEvents = pagePreviewEvents;
        }

        public PartialRouteData GetPartialVirtualPath(ContentVersion content, string language, RouteValueDictionary routeValues, RequestContext requestContext)
        {
            if (PageEditing.PageIsInEditMode) { return null; }
            var contentLink = requestContext.GetRouteValue("node", routeValues) as ContentReference;
            return new PartialRouteData
            {
                BasePathRoot = contentLink,
                PartialVirtualPath = $"{content}/"
            };
        }

        public object RoutePartial(IContent content, SegmentContext segmentContext)
        {
            if (PageEditing.PageIsInEditMode) { return null; }
            if (!_pagePreview.IsAllowed()) { return null; }
            var workId = TryGetVersionSegment(ref segmentContext);
            if (ContentReference.IsNullOrEmpty(workId)) { return null; }
            
            //segmentContext.RoutedContentLink = workId; // BAD - causes 404 errors for otherwise properly routed content.

            // segmentContext.ContextMode = ContextMode.Preview; // this is bad as it messes up all Urls for images, links ,etc
            _pagePreviewEvents.PreviewVersionResolved?.Invoke(workId, segmentContext);

            return _contentLoader.Get<IContent>(workId);
        }

        private ContentReference TryGetVersionSegment(ref SegmentContext segmentContext)
        {
            var segment = segmentContext.GetNextValue(segmentContext.RemainingPath);
            var versionSegment = segment.Next;
            if (string.IsNullOrEmpty(versionSegment)) { return null; }

            var re = new Regex("^[0-9]{1,10}_[0-9]{1,10}$");
            if (!re.IsMatch(versionSegment)) { return null; }

            var workId = new ContentReference(versionSegment);
            if (workId.Equals(ContentReference.EmptyReference)) { return null; }

            segmentContext.RemainingPath = segment.Remaining;
            return workId;
        }
    }
}

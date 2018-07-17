using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Editor;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using EPiServer.Web.Routing.Segments;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace eGandalf.Epi.PagePreview
{
    public class PreviewPartialRouter : IPartialRouter<IContent, ContentVersion>
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

        public object RoutePartial(IContent content, SegmentContext segmentContext)
        {
            if (PageEditing.PageIsInEditMode) return null;

            if (!_pagePreview.Service.IsAllowed()) return null;

            var workId = TryGetVersionSegment(ref segmentContext);
            if (workId == null) return null;

            return _contentLoader.Service.Get<IContent>(workId);
        }

        private ContentReference TryGetVersionSegment(ref SegmentContext segmentContext)
        {
            var segment = segmentContext.GetNextValue(segmentContext.RemainingPath);
            var versionSegment = segment.Next;

            if (string.IsNullOrEmpty(versionSegment)) return null;

            var re = new Regex("^[0-9]{1,10}_[0-9]{1,10}$");
            if (!re.IsMatch(versionSegment)) return null;

            var workId = new ContentReference(versionSegment);
            if (workId == null || workId.Equals(ContentReference.EmptyReference)) return null;

            segmentContext.RemainingPath = segment.Remaining;
            return workId;
            //return versionSegment;
        }
    }
}

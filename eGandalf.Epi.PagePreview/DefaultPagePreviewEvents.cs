using System;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing.Segments;

namespace eGandalf.Epi.PagePreview
{
    /// <summary>
    /// Singleton events class, MUST be a singleton
    /// </summary>
    [ServiceConfiguration(typeof(IPagePreviewEvents), Lifecycle = ServiceInstanceScope.Singleton)]
    public class DefaultPagePreviewEvents : IPagePreviewEvents
    {
        public Action<ContentReference, SegmentContext> PreviewVersionResolved { get; set; }
        public Func<IContent, bool?> IsRoutable { get; set; }
    }
}
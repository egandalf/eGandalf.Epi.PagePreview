using System;
using EPiServer.Core;
using EPiServer.Web.Routing.Segments;

namespace eGandalf.Epi.PagePreview
{
    /// <summary>
    /// Implementations MUST be singleton to keep state
    /// </summary>
    public interface IPagePreviewEvents
    {
        Action<ContentReference, SegmentContext> PreviewVersionResolved { get; set; }
        Func<IContent, bool?> IsRoutable { get; set; }
    }
}
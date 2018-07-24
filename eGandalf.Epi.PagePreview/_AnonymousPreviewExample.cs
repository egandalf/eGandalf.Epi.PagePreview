using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Internal;
using EPiServer.Web.Routing.Segments;
using System;
using System.Web;

namespace eGandalf.Epi.PagePreview
{
    // this class is not compiled with the project source code

    /// <summary>
    /// This is just an example class of enabling page previews for unpublished content, this example uses code not covered by Episerver semantic versioning, which means any NuGet package upgrade may break this sample!
    /// </summary>
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    [ModuleDependency(typeof(PreviewRouterInitialization))]
    public class AllowUnpublishedPreview : IConfigurableModule
    {
        public static Func<HttpContextBase> HttpContextAccessor = () => HttpContext.Current == null ? null : new HttpContextWrapper(HttpContext.Current);

        private const string PreviewKey = "egandalfPagePreview";

        void IConfigurableModule.ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IPagePreview, AnonymousPagePreview>();
            context.Services.Add<IRoutableEvaluator, PreviewRoutableEvaluator>(ServiceInstanceScope.Transient);
        }

        public void Initialize(InitializationEngine context)
        {
            var previewEvents = context.Locate.Advanced.GetInstance<IPagePreviewEvents>();
            previewEvents.PreviewVersionResolved += ResolvedPreviewId;
            previewEvents.IsRoutable += CanDisplay;
        }

        void IInitializableModule.Uninitialize(InitializationEngine context) { }

        private bool? CanDisplay(IContent content)
        {
            if (content == null) { return null; }
            if (HttpContextAccessor?.Invoke().Items[PreviewKey] != null) { return true; }

            return null;
        }

        private void ResolvedPreviewId(ContentReference previewVersion, SegmentContext segmentContext)
        {
            if (HttpContextAccessor?.Invoke() is HttpContextBase context)
            {
                context.Items[PreviewKey] = true;

            }
            //segmentContext.ContextMode = ContextMode.Preview; //messes up images, a FullPreview enum option would be super!
        }
    }

    /// <summary>
    /// Changes to IRoutableEvaluator may not work, interface is NOT covered by semantic versioning!, Use at your own risk!
    /// </summary>
    public class PreviewRoutableEvaluator : IRoutableEvaluator
    {
        private readonly IContextModeResolver _contextModeResolver;
        private readonly IPagePreviewEvents _pagePreviewEvents;
        private readonly IPublishedStateAssessor _publishedStateAssessor;
        public PreviewRoutableEvaluator(IContextModeResolver contextModeResolver, IPublishedStateAssessor publishedStateAssessor, IPagePreviewEvents pagePreviewEvents)
        {
            _contextModeResolver = contextModeResolver;
            _publishedStateAssessor = publishedStateAssessor;
            _pagePreviewEvents = pagePreviewEvents;
        }

        public bool IsRoutable(IContent content)
        {
            if (_publishedStateAssessor.IsPublished(content)) { return true; }
            var eventCheck = _pagePreviewEvents.IsRoutable?.Invoke(content); // used to check if conditions were met in partial routing
            return eventCheck ?? _contextModeResolver.CurrentMode.EditOrPreview(); // perhaps one day we can get a FullPreview context mode!
        }
    }
}
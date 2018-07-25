# Episerver Page Preview
Page Preview in Episerver. [Based on code](https://world.episerver.com/blogs/Jacob-Khan/Dates/2016/4/preview-content-in-view-mode/) originally by Jacob Khan.

This add-on is, hopefully, based on doing things the "Right Way" for Episerver development, at least for version 10. Moving away from things like iframes for components and moving forward with Dojo and includes the ability to work with the content context as well as translations within the Dojo widget. I admit that these bits took a lot of digging through documentation, developer blogs, and Dojo documentation to make it all come together and I intend to write a blog post (or three) going over the process as well as linking to as many of the docs and articles as I can remember that helped bring it together in case anyone else will find them as useful as I did.

With that, here's what this release does and does not do and how to use it.

## Installation

1. Install the package from [Episerver's nuget feed](http://nuget.episerver.com/). 
2. [Update or add translations](http://world.episerver.com/documentation/developer-guides/CMS/globalization/localizing-the-user-interface/) as desired via the included [`eGandalf.Epi.PagePreview.xml`](https://github.com/egandalf/eGandalf.Epi.PagePreview/blob/master/eGandalf.Epi.PagePreview/Resources/eGandalf.Epi.PagePreview.xml) file, which will be installed to the `~/Resources/LangaugeFiles` directory in your Episerver application.
3. As an author, add the Page Preview component to the desired location in your authoring panels.

## Functionality

This add-on component for Episerver provides a button that opens a preview for the current draft of the current page in the author view. It opens a new tab showing the page based on the draft work ID (visible in the URL) but without any of the authoring tools. This allows the author to view the content in the browser and use whatever other tools they wish, such as 3rd party mobile device preview or accessibility compliance tools, to validate the content further.

Currently, this will load only changes to content properties made on the page itself. It does **not** preview with modifications to blocks or as part of a project, thought that functionality may be added at a later time.

This is not tested or set up to work with Commerce catalog or product pages and is untested in this scenario.

For object types where Preview is unsupported, the add-on should display a message indicating that preview is unavailable. This message can be customized and translated using Episerver's translation features.

## Customizations

There are two ways to extend the use of this component I'd like to mention. First, the ability to enable non-authenticated previews, e.g. for approval by a non-author colleague. Second, the ability of a developer to limit the conditions under which the preview is available via Inversion of Control.

Through administrative configuration, the application can be set up to allow the preview to be opened by anonymous visitors. This would allow an author to generate the preview URL and share it with another party who does not have a login into Episerver as an author. An example use case for this is that an author can send the Preview URL to another person within the company for approval prior to go-live. The recipient need not be an author or logged in.

### Instructions

1. As an Episerver administrator, open the Admin portal and go to CMS > Admin > Access Rights > Set Access Rights
2. Find the content node or leaf for which Anonymous Preview should be enabled.
3. If not already present, add the Anonymous group/role to the permission group.
4. Grand Anonymous permissions for Read and Change.
5. Save.

This item and any which inherit its permissions should be available for anonymous preview.

** Note: The package is shipped with a PagePreview service that disallows preview by anonymous (unauthenticated visitors) by default. A developer will need to override this functionality using Inversion of Control (documented below) before Anonymous preview will work.

### Risks

Enabling anonymous preview requires opening up unpublished versions of content to be viewable by non-authenticated visitors. *This does not give anonymous visitors access to the authoring interface.*

It does, however, mean that a potential attacker relatively simple script could open previews of unpublished content if this functionality is enabled in production. Because of this risk, I have included a default rule that at least requires a use to be authenticated as well as the ability for a developer to write their own rules for enabling this functionality.

## Inversion of Control

Developers, in order to define your own logic for whether Page Preview is allowed, I have used Episerver's own StructureMap for dependency injection and have made it rather simple for you to insert your own logic in place of my own. Follow these steps:

1. Create a class that implements `eGandalf.Epi.PagePreview.IPagePreview`

```C#
namespace Multisite.Business.PagePreview
{
    public class MyPagePreview : eGandalf.Epi.PagePreview.IPagePreview
    {
        public bool IsAllowed()
        {
            
        }
    }
}
```
2. Write your own code for the IsAllowed method therein.
```C#
public bool IsAllowed()
{
    // Note that this role must have Read / Change permissions on the content to be previewed.
    return HttpContext.Current.User.IsInRole("Previewers");
}
```
3. Use Episerver's Initialization interface to override my implementation with your own.
```C#
using eGandalf.Epi.PagePreview;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.Compatibility;

namespace Multisite.Business.PagePreview
{
    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    public class PagePreviewInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.Configure(c =>
            {
                c.For<IPagePreview>().Use<MyPagePreview>();
            });
        }
        public void Initialize(InitializationEngine context) { }
        public void Uninitialize(InitializationEngine context) { }
    }
}
```

## Troubleshooting

If you load the widget/component in a page context and you see a button with text 'undefined', then you are likely missing a localization provider. Look for something similar to this in your `web.config` inside of the `<episerver.framework>` section:

```xml
<localization fallbackBehavior="Echo, FallbackCulture" fallbackCulture="en">
  <providers>
    <add name="languageFiles" virtualPath="~/Resources/LanguageFiles" type="EPiServer.Framework.Localization.XmlResources.FileXmlLocalizationProvider, EPiServer.Framework"/>
  </providers>
</localization>
```

## Anonymous Page Preview for unpublished content
In the default implementation, unpublished content previews are not supported. The classes below are a demonstration of how to provide previews of unpublished content. The APIs used in this code is not covered by Episerver semantic versioning, so use at your own risk! Any NuGet package upgrade for Episerver may break this example.
```cs
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
```
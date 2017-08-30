using EPiServer.Shell;

namespace eGandalf.Epi.PagePreview
{
    [LocalizedComponent(
        PlugInAreas = PlugInArea.Assets, 
        Categories = "cms", 
        WidgetType = "egandalf/PagePreview", 
        Title = "/egandalf/pagepreview/componentTitle", 
        SortOrder = 1000,
        Description = "/egandalf/pagepreview/componentDescription"
        )]
    public class ComponentDefinition
    {
    }
}

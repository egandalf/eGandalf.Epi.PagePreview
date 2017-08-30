using EPiServer.Framework.Localization;
using EPiServer.Shell.ViewComposition;
using System;

namespace eGandalf.Epi.PagePreview
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LocalizedComponentAttribute : ComponentAttribute
    {
        public override string Title
        {
            get
            {
                return LocalizationService.Current.GetString(base.Title);
            }
            set
            {
                base.Title = value;
            }
        }

        public override string Description
        {
            get
            {
                return LocalizationService.Current.GetString(base.Description);
            }
            set
            {
                base.Description = value;
            }
        }
    }
}

using EPiServer.Shell.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eGandalf.Epi.PagePreview
{
    public class PagePreviewModule : ShellModule
    {
        public PagePreviewModule(string name, string routeBasePath, string resourceBasePath)
            : base(name, routeBasePath, resourceBasePath)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gdocerous.Code;

namespace gdocerous.Controllers
{
    public class MiniProfiledController : Controller
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            if (!Session.GetGoogleDocsRepos().IsDeveloperAccount())
            {
                MvcMiniProfiler.MiniProfiler.Stop(discardResults: true);
            }
        }
    }
}

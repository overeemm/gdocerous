using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace gdocerous.Code
{
    public class RequiresValidSessionAttribute : ActionFilterAttribute
    {
        public RequiresValidSessionAttribute()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Session.IsValidSession())
            {
                filterContext.Result = new RedirectResult(new UrlHelper(filterContext.RequestContext).Action("Index", "Default"));
            }
        }
    }
}
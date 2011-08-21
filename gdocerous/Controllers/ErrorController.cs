using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace gdocerous.Controllers
{
    public class ErrorController : Controller
    {

        public ActionResult General(Exception exception)
        {
            return View("Exception", exception);
        }

        public ActionResult Http404()
        {
            return View("404");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace gdocerous.Controllers
{
    public class ErrorController : MiniProfiledController
    {
        public ActionResult General(Exception exception)
        {
            ViewBag.Exception = exception;
            return View();
        }

        public ActionResult Http404()
        {
            return View("404");
        }
    }
}

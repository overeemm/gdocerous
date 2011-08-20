using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gdocerous.Code;

namespace gdocerous.Controllers
{
    public class DefaultController : Controller
    {
        public string GoogleAuthToken { get { return Session["token"] as string; } set { Session["token"] = value; } }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login()
        {
            return Redirect(new GoogleApi().GetAuthUrl());
        }

        public ActionResult Authenticate()
        {
            GoogleAuthToken = new GoogleApi().GetSessionToken((String)Request["token"]);
            return RedirectToAction("Categories");
        }

        public ActionResult Categories()
        {
            ViewBag.Folders = new GoogleApi(GoogleAuthToken).GetFolders();
            return View();
        }

        public ActionResult Documents(string folder)
        {
            ViewBag.Folder = folder;
            ViewBag.Documents = new GoogleApi(GoogleAuthToken).GetDocuments(folder);
            return View();
        }

        public ActionResult Document(string folder, string document)
        {
            GoogleApi docs = new GoogleApi(GoogleAuthToken);

            ViewBag.Folder = folder;
            ViewBag.Document = docs.GetDocument(folder, document);
            ViewBag.DocumentHtml = docs.GetHtmlContent(ViewBag.Document);
            return View();
        }

        public ActionResult Send()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Send(string folder, string document, string tags)
        {
            using (Posterous post = new Posterous(new GoogleApi(GoogleAuthToken).GetDocumentContent(folder, document), document))
            {
                post.Send(tags);
            }

            return RedirectToAction("Send");
        }
    }
}

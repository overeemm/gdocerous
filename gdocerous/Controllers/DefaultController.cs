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
        public bool IsValidSession { get { return !string.IsNullOrEmpty(GoogleAuthToken); } }

        public ActionResult Index()
        {
            ViewBag.IsValidSession = false;
            return View();
        }

        [HttpPost]
        public ActionResult Login()
        {
            return Redirect(GoogleApi.GetAuthUrl(Url.Action("Authenticate", "Default", null, Request.Url.Scheme)));
        }

        public ActionResult Authenticate()
        {
            GoogleAuthToken = GoogleApi.GetSessionToken((String)Request["token"]);

            if (!new GoogleApi(GoogleAuthToken).AllowedAccount())
            {
                GoogleAuthToken = "";
                throw new InvalidOperationException("gdocerous is not yet open for public.");
            }
            return RedirectToAction("Folder");
        }

        public ActionResult Folder(string folder)
        {
            if(!IsValidSession)
                return RedirectToAction("Index");

            GoogleApi api = new GoogleApi(GoogleAuthToken);
            dynamic folderobj = api.GetFolder(folder);

            ViewBag.Folder = string.IsNullOrEmpty(folder) ? "root" : folderobj.Title;
            ViewBag.Documents = api.GetFolderContent(folderobj);
            ViewBag.IsValidSession = IsValidSession;
            return View();
        }

        public ActionResult Document(string document)
        {
            if (!IsValidSession)
                return RedirectToAction("Index");

            GoogleApi api = new GoogleApi(GoogleAuthToken);
            dynamic documetnobj = api.GetDocument(document);

            ViewBag.Document =  documetnobj.Title;
            ViewBag.DocumentId = document;
            ViewBag.DocumentHtml = api.GetHtmlContent(documetnobj);
            ViewBag.IsValidSession = IsValidSession;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.IsValidSession = IsValidSession;
            return View();
        }

        [HttpPost]
        public ActionResult Send(string document, string tags, string type, string receivecopy)
        {
            if (!IsValidSession)
                return RedirectToAction("Index");

            GoogleApi api = new GoogleApi(GoogleAuthToken);
            dynamic documetnobj = api.GetDocument(document);

            using (Posterous post = new Posterous(api.GetDocumentContent(document), documetnobj.Title))
            {
                post.Send(tags, "private".Equals(type) ? PostType.Private :
                                "public".Equals(type) ? PostType.Public : PostType.Draft
                              , "1".Equals(receivecopy));
            }

            return RedirectToAction("Folder");
        }
    }
}

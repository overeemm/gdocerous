using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gdocerous.Code;

namespace gdocerous.Controllers
{
    [RequireHttps]
    public class DefaultController : MiniProfiledController
    {
        public ActionResult Index()
        {
            ViewBag.IsValidSession = false;
            return View();
        }

        [HttpPost]
        public ActionResult Login()
        {
            return Redirect(GoogleDocsRepository.GetAuthUrl());
        }

        public ActionResult AuthCallback(string error, string code)
        {
            if (!string.IsNullOrEmpty(error))
            {
                return RedirectToAction("Index");
            }
            if (!string.IsNullOrEmpty(code))
            {
                Session.SetGoogleDocsRepos(new GoogleDocsRepository(code));

                return RedirectToAction("PosterousSender");
            }

            return RedirectToAction("Index");
        }

        [RequiresValidSession]
        public ActionResult PosterousSender()
        {
            GoogleDocs docs = Session.GetGoogleDocsRepos().GetGoogleDocs();
            ViewBag.EmailAddress = docs.GetgdocerousMailAddress();

            return View();
        }

        [RequiresValidSession]
        public ActionResult Folder(string folder)
        {
            GoogleDocs docs = Session.GetGoogleDocsRepos().GetGoogleDocs();
            var folderobj = docs.GetFolder(folder);

            ViewBag.Folder = string.IsNullOrEmpty(folder) ? "root" : folderobj.Title;
            ViewBag.Documents = docs.GetFolderContent(folderobj);
            ViewBag.IsValidSession = true;
            return View();
        }

        [RequiresValidSession]
        public ActionResult Document(string document)
        {
            GoogleDocs docs = Session.GetGoogleDocsRepos().GetGoogleDocs();
            var documentobj = docs.GetDocument(document);

            ViewBag.Document = documentobj.Title;
            ViewBag.DocumentId = document;
            ViewBag.DocumentHtml = docs.GetHtmlContent(documentobj);
            ViewBag.EmailAddress = docs.GetgdocerousMailAddress();
            ViewBag.IsValidSession = true;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.IsValidSession = Session.IsValidSession();
            return View();
        }

        [HttpPost,RequiresValidSession]
        public ActionResult Send(string document, string tags, string type, string receivecopy)
        {
            using (GoogleDocs docs = Session.GetGoogleDocsRepos().GetGoogleDocs())
            {
                var documentobj = docs.GetDocument(document);

                using (PosterousRepository post = new PosterousRepository(docs.Email, docs.GetgdocerousMailAddress(), Session.GetGoogleDocsRepos().OAuthToken, docs.GetDocumentContent(document), documentobj.Title))
                {
                    post.Send(tags, "private".Equals(type) ? PostType.Private :
                                    "public".Equals(type) ? PostType.Public : PostType.Draft
                                  , "1".Equals(receivecopy));
                }
            }

            return RedirectToAction("Folder");
        }
    }
}

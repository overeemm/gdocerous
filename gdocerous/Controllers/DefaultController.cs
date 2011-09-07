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

        public ActionResult TempIndex()
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
                return RedirectToAction("TempIndex");
            }
            if (!string.IsNullOrEmpty(code))
            {
                Session.SetGoogleDocsRepos(new GoogleDocsRepository(code));

                //if (!Session.GetGoogleDocsRepos().AllowedAccount())
                //{
                //    Session.SetGoogleDocsRepos(null);
                //    throw new InvalidOperationException("gdocerous is not yet open for public.");
                //}

                return RedirectToAction("Folder");
            }

            return RedirectToAction("TempIndex");
        }

        public ActionResult Folder(string folder)
        {
            if (!Session.IsValidSession())
                return RedirectToAction("Index");

            GoogleDocs docs = Session.GetGoogleDocsRepos().GetGoogleDocs();
            dynamic folderobj = docs.GetFolder(folder);

            ViewBag.Folder = string.IsNullOrEmpty(folder) ? "root" : folderobj.Title;
            ViewBag.Documents = docs.GetFolderContent(folderobj);
            ViewBag.IsValidSession = true;
            return View();
        }

        public ActionResult Document(string document)
        {
            if (!Session.IsValidSession())
                return RedirectToAction("Index");

            GoogleDocs docs = Session.GetGoogleDocsRepos().GetGoogleDocs();
            dynamic documentobj = docs.GetDocument(document);

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

        [HttpPost]
        public ActionResult Send(string document, string tags, string type, string receivecopy, string emailaddressregistered)
        {
            if (!Session.IsValidSession())
                return RedirectToAction("Index");

            if (!"1".Equals(emailaddressregistered))
                return RedirectToAction("Folder");

            using (GoogleDocs docs = Session.GetGoogleDocsRepos().GetGoogleDocs())
            {
                dynamic documentobj = docs.GetDocument(document);

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

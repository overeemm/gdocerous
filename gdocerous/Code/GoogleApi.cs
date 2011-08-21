using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.GData.Documents;
using Google.GData.Client;
using Google.Documents;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib;

namespace gdocerous.Code
{
    public class GoogleApi
    {
        private string m_token;
        private DocumentsRequest m_request;

        public GoogleApi(string token)
        {
            m_token = token;
            m_request = new DocumentsRequest(new RequestSettings(MvcApplication.APPNAME, Token));
        }

        public GoogleApi()
        {
        }

        private string Token { get { return m_token; } }

        public static string GetAuthUrl(string returnurl)
        {
            return AuthSubUtil.getRequestUrl(returnurl, "https://docs.google.com/feeds/ https://mail.google.com/", false, true);
        }

        public static string GetSessionToken(string requesttoken)
        {
            return AuthSubUtil.exchangeForSessionToken(requesttoken, null);
        }

        public IEnumerable<Document> GetFolderContent(Document folder)
        {
            if (folder == null)
                return GetRootFolders();
            else
                return GetSubFolderContent(folder);
        }

        private IEnumerable<Document> GetRootFolders()
        {
            foreach (Document doc in m_request.GetFolders().Entries)
                if (doc.ParentFolders.Count == 0)
                    yield return doc;
        }

        public Document GetFolder(string folderresourceid)
        {
            if (string.IsNullOrEmpty(folderresourceid))
                return null;

            foreach (Document doc in m_request.GetFolders().Entries)
                if (doc.ResourceId == "folder:" + folderresourceid)
                    return doc;

            return null;
        }

        internal Document GetDocument(string documentresourceid)
        {
            if (string.IsNullOrEmpty(documentresourceid))
                return null;

            foreach (Document doc in m_request.GetDocuments().Entries)
                if (doc.ResourceId == "document:" + documentresourceid)
                    return doc;

            return null;
        }

        private IEnumerable<Document> GetSubFolderContent(Document folder)
        {
            foreach (Document doc in m_request.GetFolderContent(folder).Entries)
                if (doc.ParentFolders.Count == folder.ParentFolders.Count + 1)
                    yield return doc;
        }

        /// <summary>
        /// Get the zip-html version, extract the html file and present it (to review, not to edit).
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public string GetHtmlContent(Document doc)
        {
            using (Stream str = GetDocExportStream(doc, Document.DownloadType.zip))
            using (ZipInputStream zipInputStream = new ZipInputStream(str))
            {
                ZipEntry zipEntry = zipInputStream.GetNextEntry();
                while (zipEntry != null)
                {
                    String entryFileName = zipEntry.Name;
                    if (entryFileName.ToLowerInvariant().EndsWith(".html"))
                    {
                        byte[] buffer = new byte[4096];
                        using (MemoryStream mstream = new MemoryStream())
                        using (StreamReader reader = new StreamReader(mstream))
                        {
                            StreamUtils.Copy(zipInputStream, mstream, buffer);
                            mstream.Position = 0;
                            return reader.ReadToEnd();
                        }
                    }
                    zipEntry = zipInputStream.GetNextEntry();
                }

                return "no HTML file found!";
            }
        }

        /// <summary>
        /// Instead of DocumentsRequest.Download ; that function keeps returning a 404.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="downloadtype"></param>
        /// <returns></returns>
        private Stream GetDocExportStream(Document doc, Document.DownloadType downloadtype)
        {
            string format = downloadtype.ToString();
            string url = doc.DocumentEntry.Content.AbsoluteUri + "&exportFormat=" + format + "&format=" + format;
            return m_request.Service.Query(new Uri(url));
        }

        public Stream GetDocumentContent(string document)
        {
            return GetDocExportStream(GetDocument(document), Document.DownloadType.zip);
        }
    }
}
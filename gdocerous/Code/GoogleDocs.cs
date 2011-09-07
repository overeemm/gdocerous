using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using Google.GData.Documents;
using Google.GData.Client;
using Google.Documents;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib;
using System.Text;
using System.Runtime.Serialization.Json;
using MvcMiniProfiler;

namespace gdocerous.Code
{
    public class GoogleDocs : IDisposable
    {
        private GoogleOAuthSession m_token;
        private DocumentsRequest m_request;
        private string m_email;

        public GoogleDocs(GoogleOAuthSession token, string email)
        {
            m_token = token;
            m_email = email;
            m_request = new DocumentsRequest(new RequestSettings(MvcApplication.APPNAME, m_token.access_token));
        }

        public string Email { get { return m_email; } }

        public dynamic GetgdocerousMailAddress()
        {
            return string.Concat(Email.Replace("@", "."), "@gdocerous.com");
        }

        public Document GetDocument(string documentresourceid)
        {
            using (MiniProfiler.Current.Step("GoogleDocs.GetDocument"))
            {
                if (string.IsNullOrEmpty(documentresourceid))
                    return null;

                foreach (Document doc in m_request.GetDocuments().Entries)
                    if (doc.ResourceId == "document:" + documentresourceid)
                        return doc;

                return null;
            }
        }

        private IEnumerable<Document> GetSubFolderContent(Document folder)
        {
            using (MiniProfiler.Current.Step("GoogleDocs.GetSubFolderContent"))
            {
                foreach (Document doc in m_request.GetFolderContent(folder).Entries)
                    if (doc.ParentFolders.Count == folder.ParentFolders.Count + 1)
                        yield return doc;
            }
        }

        /// <summary>
        /// Get the zip-html version, extract the html file and present it (to review, not to edit).
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public string GetHtmlContent(Document doc)
        {
            using (MiniProfiler.Current.Step("GoogleDocs.GetHtmlContent"))
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
        }

        /// <summary>
        /// Instead of DocumentsRequest.Download ; that function keeps returning a 404.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="downloadtype"></param>
        /// <returns></returns>
        private Stream GetDocExportStream(Document doc, Document.DownloadType downloadtype)
        {
            using (MiniProfiler.Current.Step("GoogleDocs.GetDocExportStream"))
            {
                string format = downloadtype.ToString();
                string url = doc.DocumentEntry.Content.AbsoluteUri + "&exportFormat=" + format + "&format=" + format;
                return m_request.Service.Query(new Uri(url));
            }
        }

        public Stream GetDocumentContent(string document)
        {
            return GetDocExportStream(GetDocument(document), Document.DownloadType.zip);
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
            using (MiniProfiler.Current.Step("GoogleDocs.GetRootFolders"))
            {
                foreach (Document doc in m_request.GetFolders().Entries)
                    if (doc.ParentFolders.Count == 0)
                        yield return doc;
            }
        }

        public Document GetFolder(string folder)
        {
            using (MiniProfiler.Current.Step("GoogleDocs.GetFolder"))
            {
                if (string.IsNullOrEmpty(folder))
                    return null;

                foreach (Document doc in m_request.GetFolders().Entries)
                    if (doc.ResourceId == "folder:" + folder)
                        return doc;

                return null;
            }
        }

        public void Dispose()
        {
            m_request = null;
        }
    }
}
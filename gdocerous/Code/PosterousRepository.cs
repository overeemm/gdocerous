using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib;

namespace gdocerous.Code
{
    public enum PostType { Draft, Private, Public }

    public class PosterousRepository : IDisposable
    {
        private Stream m_zipfile;
        private string m_html;
        private Dictionary<string, Stream> m_attachments;
        private string m_documenttitle;

        private string m_email;
        private string m_senderemail;
        private string m_oauthtoken;

        public PosterousRepository(string email, string senderemail, string oauthtoken, Stream htmlzipfile, string documenttitle)
        {
            m_documenttitle = documenttitle;
            m_zipfile = htmlzipfile;
            m_attachments = new Dictionary<string, Stream>();

            m_email = email;
            m_senderemail = senderemail;
            m_oauthtoken = oauthtoken;

            ExtractZipFile();
        }

        public void Send(string tags, PostType type, bool receivecopy)
        {
            using (MailMessage msg = new MailMessage())
            {
                foreach (KeyValuePair<string, Stream> attachment in m_attachments)
                {
                    msg.Attachments.Add(new Attachment(attachment.Value, attachment.Key));
                    m_html = m_html.Replace(attachment.Key, "cid:" + attachment.Key);
                }

                msg.From = new MailAddress(m_senderemail, "gdocerous");
                msg.ReplyToList.Add(new MailAddress(m_email));

                if (type == PostType.Private)
                    msg.To.Add(new MailAddress("private@posterous.com"));
                else if (type == PostType.Draft)
                    msg.To.Add(new MailAddress("draft@posterous.com"));
                else if (type == PostType.Public)
                    msg.To.Add(new MailAddress("post@posterous.com"));
                
                if (receivecopy)
                    msg.Bcc.Add(new MailAddress(m_email));

                msg.Subject = string.Format("{0} ((tag: {1}))", m_documenttitle, tags);

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(m_html, new System.Net.Mime.ContentType("text/html"));

                foreach (KeyValuePair<string, Stream> attachment in m_attachments)
                {
                    LinkedResource linkedRes = new LinkedResource(attachment.Value, GetMediaType(attachment.Key));
                    linkedRes.ContentId = attachment.Key;
                    htmlView.LinkedResources.Add(linkedRes);
                }

                msg.AlternateViews.Add(htmlView);
                
                var smtp = new SmtpClient
                       {
                           Host = "84.243.195.170",
                           Port = 25,
                           DeliveryMethod = SmtpDeliveryMethod.Network
                       };

                smtp.Send(msg);
            }
        }

        private System.Net.Mime.ContentType GetMediaType(string p)
        {
            string filename = p.ToLowerInvariant();

            if (filename.EndsWith(".jpg"))
                return new System.Net.Mime.ContentType("image/jpeg");
            else if (filename.EndsWith(".gif"))
                return new System.Net.Mime.ContentType("image/gif");
            else if (filename.EndsWith(".png"))
                return new System.Net.Mime.ContentType("image/png");
            else if (filename.EndsWith(".pdf"))
                return new System.Net.Mime.ContentType("application/pdf");

            return new System.Net.Mime.ContentType("application/octet-stream");
        }

        private void ExtractZipFile()
        {
            using (ZipInputStream zipInputStream = new ZipInputStream(m_zipfile))
            {
                ZipEntry zipEntry = zipInputStream.GetNextEntry();
                while (zipEntry != null)
                {
                    String entryFileName = zipEntry.Name;
                    if (entryFileName.ToLowerInvariant().EndsWith(".html"))
                    {
                        ExtractHtml(zipInputStream);
                    }
                    else
                    {
                        byte[] buffer = new byte[4096];
                        MemoryStream mstream = new MemoryStream();
                        StreamUtils.Copy(zipInputStream, mstream, buffer);
                        mstream.Position = 0;
                        m_attachments.Add(entryFileName, mstream);
                    }
                    zipEntry = zipInputStream.GetNextEntry();
                }
            }

            if (string.IsNullOrEmpty(m_html))
                throw new InvalidOperationException("No HTML found to post to your posterous.");
        }

        private void ExtractHtml(ZipInputStream zipInputStream)
        {
            byte[] buffer = new byte[4096];
            using (MemoryStream mstream = new MemoryStream())
            using (StreamReader reader = new StreamReader(mstream))
            {
                StreamUtils.Copy(zipInputStream, mstream, buffer);
                mstream.Position = 0;
                m_html = reader.ReadToEnd();
            }
        }

        public void Dispose()
        {
            if (m_zipfile != null)
            {
                m_zipfile.Dispose();
                m_zipfile = null;
            }
            foreach (Stream str in m_attachments.Values)
                str.Dispose();
            m_attachments.Clear();
        }
    }
}